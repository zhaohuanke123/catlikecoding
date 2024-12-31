using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

public struct FractalPart
{
    /// <summary>
    /// 方向向量
    /// </summary>
    public float3 direction;

    /// <summary>
    /// 世界坐标
    /// </summary>
    public float3 worldPosition;

    /// <summary>
    /// 本地旋转
    /// </summary>
    public quaternion rotation;

    /// <summary>
    /// 世界旋转）
    /// </summary>
    public quaternion worldRotation;

    /// <summary>
    /// 旋转角度
    /// </summary>
    public float spinAngle;
}

//  FloatMode.Fast允许  Burst 重新排序数学运算，例如将 a + b * c 重写为 b * c + a 。这可以提高性能，因为存在
//  madd（乘加）指令，它比单独的加法指令后再进行乘法运算更快。着色器编译器默认情况下会执行此操作。通常重新排序运算不会产生逻辑差异，但是由于浮点数的限制，更改顺序会产生略微不同的结果。
// CompileSynchronously强制编辑器在需要时立即编译作业的 Burst 版本
/// <summary>
/// 用于更新分形层级的工作任务，通过计算每个FractalPart的旋转、位置和变换矩阵来构建分形结构。
/// 该任务在每一层分形中执行，利用父节点的旋转和位置信息计算当前节点的旋转和位置。
/// </summary>
[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
struct UpdateFractalLevelJob : IJobFor
{
    /// <summary>
    /// 执行更新任务，计算FractalPart的旋转和位置，并更新相关数据。
    /// </summary>
    /// <param name="i">当前任务的索引。</param>
    public void Execute(int i)
    {
        // 1. 获取当前FractalPart和当前FractalPart的父节点
        var parent = m_parents[i / 5];
        var part = m_parts[i];

        // 2. 更新旋转角度
        part.spinAngle += m_spinAngleDelta;
        // 3. 计算当前FractalPart的世界旋转，世界位置
        part.worldRotation = mul(parent.worldRotation,
            mul(part.rotation, quaternion.RotateY(part.spinAngle))
        );
        part.worldPosition =
            parent.worldPosition +
            mul(parent.worldRotation, 1.5f * m_scale * part.direction);
        m_parts[i] = part;

        // 4. 计算当前FractalPart的变换矩阵（旋转 + 位移）
        var r = float3x3(part.worldRotation) * m_scale;
        m_matrices[i] = float3x4(r.c0, r.c1, r.c2, part.worldPosition);
    }

    /// <summary>
    /// 每个FractalPart的旋转增量，用于更新旋转角度。
    /// </summary>
    public float m_spinAngleDelta;

    /// <summary>
    /// 每个FractalPart的缩放因子，用于缩放FractalPart。
    /// </summary>
    public float m_scale;

    /// <summary>
    /// 存储父节点数据的数组，表示上一层的FractalPart。
    /// </summary>
    [ReadOnly]
    public NativeArray<FractalPart> m_parents;

    /// <summary>
    /// 存储当前FractalPart数据的数组，每个FractalPart的数据包括旋转、位置等信息。
    /// </summary>
    public NativeArray<FractalPart> m_parts;

    /// <summary>
    /// 存储变换矩阵（旋转 + 位移）的数组，用于渲染每个FractalPart的位置和旋转。
    /// </summary>
    [WriteOnly]
    public NativeArray<float3x4> m_matrices;
}

public class Fractal : MonoBehaviour
{
    #region Unity 生命周期

    private void OnEnable()
    {
        // 1. 初始化分形深度相关的数据结构
        m_parts = new NativeArray<FractalPart>[m_depth];
        m_matrices = new NativeArray<float3x4>[m_depth];

        // 2. 初始化计算缓冲区
        m_matricesBuffers = new ComputeBuffer[m_depth];
        int stride = 12 * 4; // 每个矩阵的字节大小
        for (int i = 0, length = 1; i < m_parts.Length; i++, length *= 5)
        {
            m_parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
            m_matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
            m_matricesBuffers[i] = new ComputeBuffer(length, stride);
        }

        // 3. 设置根部分形
        m_parts[0][0] = CreatePart(0);

        // 4. 初始化每一层的FractalPart
        for (int li = 1; li < m_parts.Length; li++)
        {
            NativeArray<FractalPart> levelParts = m_parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi += 5)
            {
                for (int ci = 0; ci < 5; ci++)
                {
                    levelParts[fpi + ci] = CreatePart(ci);
                }
            }
        }

        // 5. 初始化材质属性块
        m_propertyBlock ??= new MaterialPropertyBlock();
    }

    private void OnDisable()
    {
        // 1. 释放计算缓冲区和NativeArray
        for (int i = 0; i < m_matricesBuffers.Length; i++)
        {
            m_matricesBuffers[i].Release();
            m_parts[i].Dispose();
            m_matrices[i].Dispose();
        }

        // 2. 清理引用
        m_parts = null;
        m_matrices = null;
        m_matricesBuffers = null;
    }

    private void OnValidate()
    {
        if (m_parts != null)
        {
            OnDisable();
            OnEnable();
        }
    }

    private void Update()
    {
        // 1. 计算旋转角度增量
        float spinAngleDelta = 0.125f * PI * Time.deltaTime;

        // 2. 计算根FractalPart的世界旋转和世界位置
        FractalPart rootPart = m_parts[0][0];
        rootPart.spinAngle += spinAngleDelta;
        rootPart.worldRotation = mul(transform.rotation,
            mul(rootPart.rotation, quaternion.RotateY(rootPart.spinAngle))
        );
        rootPart.worldPosition = transform.position;
        m_parts[0][0] = rootPart;

        // 3. 计算根FractalPart的矩阵
        float objectScale = transform.lossyScale.x;
        float3x3 r = float3x3(rootPart.worldRotation) * objectScale;
        m_matrices[0][0] = float3x4(r.c0, r.c1, r.c2, rootPart.worldPosition);

        // 4. 设置更新子FractalPart的作业, 并等待作业完成
        float scale = objectScale;
        JobHandle jobHandle = default;
        for (int li = 1; li < m_parts.Length; li++)
        {
            scale *= 0.5f; // 每一层的缩放系数
            jobHandle = new UpdateFractalLevelJob
            {
                m_spinAngleDelta = spinAngleDelta,
                m_scale = scale,
                m_parents = m_parts[li - 1],
                m_parts = m_parts[li],
                m_matrices = m_matrices[li]
            }.ScheduleParallel(m_parts[li].Length, 5, jobHandle);
        }

        jobHandle.Complete();

        // 5. 更新计算缓冲区数据
        for (int i = 0; i < m_matricesBuffers.Length; i++)
        {
            m_matricesBuffers[i].SetData(m_matrices[i]);
        }

        // 6. 绘制分形
        var bounds = new Bounds(rootPart.worldPosition, 3f * objectScale * Vector3.one);
        for (int i = 0; i < m_matricesBuffers.Length; i++)
        {
            ComputeBuffer buffer = m_matricesBuffers[i];
            buffer.SetData(m_matrices[i]);
            m_propertyBlock.SetBuffer(MatricesId, buffer);
            Graphics.DrawMeshInstancedProcedural(
                m_mesh, 0, m_material, bounds, buffer.count, m_propertyBlock
            );
        }
    }

    #endregion

    #region 方法

    /// <summary>
    /// 创建一个新的FractalPart（FractalPart），用于构建分形树的每个节点。
    /// </summary>
    /// <param name="childIndex">子节点的索引，用于选择FractalPart的方向和旋转。</param>
    /// <returns>返回一个新的FractalPart（FractalPart），包含方向和旋转信息。</returns>
    private FractalPart CreatePart(int childIndex) => new FractalPart
    {
        direction = m_directions[childIndex],
        rotation = m_rotations[childIndex]
    };

    #endregion

    #region 字段

    /// <summary>
    ///  分形的递归深度
    /// </summary>
    [Range(1, 8)]
    [SerializeField]
    private int m_depth = 4;

    /// <summary>
    ///  分形使用的网格
    /// </summary>
    [SerializeField]
    private Mesh m_mesh;

    /// <summary>
    ///  分形使用的材质
    /// </summary>
    [SerializeField]
    private Material m_material;

    /// <summary>
    ///  分形的材质属性块,
    /// </summary>
    private static MaterialPropertyBlock m_propertyBlock;

    #endregion

    #region Computer Shader 相关字段

    /// <summary>
    ///  分形的矩阵缓冲区
    /// </summary>
    private ComputeBuffer[] m_matricesBuffers;

    /// <summary>
    /// Shader中 分形的矩阵属性ID
    /// </summary>
    private static readonly int MatricesId = Shader.PropertyToID("_Matrices");

    #endregion

    #region Jobs System 相关

    /// <summary>
    /// 存储每一层FractalPart的数组。每一层的FractalPart都被单独存储在一个数组中。
    /// </summary>
    private NativeArray<FractalPart>[] m_parts;

    /// <summary>
    /// 存储每一层FractalPart的变换矩阵（float3x4）的数组，用于渲染FractalPart的位置和旋转。
    /// </summary>
    private NativeArray<float3x4>[] m_matrices;

    /// <summary>
    /// FractalPart的方向数组，用于表示分形的每个子节点方向。包含了六个方向：上、右、左、前、后。
    /// </summary>
    private static float3[] m_directions =
    {
        up(), right(), left(), forward(), back()
    };

    /// <summary>
    /// FractalPart的旋转数组，定义了不同方向上的旋转。包括默认旋转、绕Z轴旋转的角度和绕X轴旋转的角度。
    /// </summary>
    private static quaternion[] m_rotations =
    {
        quaternion.identity,
        quaternion.RotateZ(-0.5f * PI),
        quaternion.RotateZ(0.5f * PI),
        quaternion.RotateX(0.5f * PI),
        quaternion.RotateX(-0.5f * PI)
    };

    #endregion
}