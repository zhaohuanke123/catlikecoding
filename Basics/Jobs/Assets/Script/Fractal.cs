using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using float4x4 = Unity.Mathematics.float4x4;
using quaternion = Unity.Mathematics.quaternion;

public struct FractalPart
{
    public float3 direction;
    public float3 worldPosition;
    public quaternion rotation;
    public quaternion worldRotation;
    public float spinAngle;
}

//  FloatMode.Fast允许  Burst 重新排序数学运算，例如将 a + b * c 重写为 b * c + a 。这可以提高性能，因为存在
//  madd（乘加）指令，它比单独的加法指令后再进行乘法运算更快。着色器编译器默认情况下会执行此操作。通常重新排序运算不会产生逻辑差异，但是由于浮点数的限制，更改顺序会产生略微不同的结果。
// CompileSynchronously强制编辑器在需要时立即编译作业的 Burst 版本
[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
struct UpdateFractalLevelJob : IJobFor
{
    public float spinAngleDelta;
    public float scale;

    [ReadOnly]
    public NativeArray<FractalPart> parents;

    public NativeArray<FractalPart> parts;

    [WriteOnly]
    public NativeArray<float4x4> matrices;

    public void Execute(int i)
    {
        FractalPart parent = parents[i / 5];
        FractalPart part = parts[i];
        part.spinAngle += spinAngleDelta;
        part.worldRotation = mul(parent.worldRotation,
            mul(part.rotation, quaternion.RotateY(part.spinAngle))
        );
        part.worldPosition =
            parent.worldPosition +
            mul(parent.worldRotation, 1.5f * scale * part.direction);
        parts[i] = part;

        matrices[i] = float4x4.TRS(
            part.worldPosition, part.worldRotation, float3(scale)
        );
    }
}

public class Fractal : MonoBehaviour
{
    #region Unity 生命周期

    private void OnEnable()
    {
        parts = new NativeArray<FractalPart>[depth];
        matrices = new NativeArray<float4x4>[depth];

        matricesBuffers = new ComputeBuffer[depth];
        int stride = 16 * 4;
        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
            matrices[i] = new NativeArray<float4x4>(length, Allocator.Persistent);
            matricesBuffers[i] = new ComputeBuffer(length, stride);
        }

        parts[0][0] = CreatePart(0);
        for (int li = 1; li < parts.Length; li++)
        {
            NativeArray<FractalPart> levelParts = parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi += 5)
            {
                for (int ci = 0; ci < 5; ci++)
                {
                    levelParts[fpi + ci] = CreatePart(ci);
                }
            }
        }

        propertyBlock ??= new MaterialPropertyBlock();
    }

    private void OnDisable()
    {
        for (int i = 0; i < matricesBuffers.Length; i++)
        {
            matricesBuffers[i].Release();
            parts[i].Dispose();
            matrices[i].Dispose();
        }

        parts = null;
        matrices = null;
        matricesBuffers = null;
    }

    private void OnValidate()
    {
        if (parts != null)
        {
            OnDisable();
            OnEnable();
        }
    }

    private void Update()
    {
        float spinAngleDelta = 0.125f * PI * Time.deltaTime;
        FractalPart rootPart = parts[0][0];
        rootPart.spinAngle += spinAngleDelta;
        rootPart.worldRotation = mul(transform.rotation,
            mul(rootPart.rotation, quaternion.RotateY(rootPart.spinAngle))
        );
        rootPart.worldPosition = transform.position;
        parts[0][0] = rootPart;
        float objectScale = transform.lossyScale.x;
        matrices[0][0] = float4x4.TRS(
            rootPart.worldPosition, rootPart.worldRotation, float3(objectScale)
        );

        float scale = objectScale;
        JobHandle jobHandle = default;
        for (int li = 1; li < parts.Length; li++)
        {
            scale *= 0.5f;
            jobHandle = new UpdateFractalLevelJob
            {
                spinAngleDelta = spinAngleDelta,
                scale = scale,
                parents = parts[li - 1],
                parts = parts[li],
                matrices = matrices[li]
            }.Schedule(parts[li].Length, jobHandle);
        }

        jobHandle.Complete();

        for (int i = 0; i < matricesBuffers.Length; i++)
        {
            matricesBuffers[i].SetData(matrices[i]);
        }

        var bounds = new Bounds(rootPart.worldPosition, 3f * objectScale * Vector3.one);
        for (int i = 0; i < matricesBuffers.Length; i++)
        {
            ComputeBuffer buffer = matricesBuffers[i];
            buffer.SetData(matrices[i]);
            propertyBlock.SetBuffer(matricesId, buffer);
            Graphics.DrawMeshInstancedProcedural(
                mesh, 0, material, bounds, buffer.count, propertyBlock
            );
        }
    }

    #endregion

    #region 方法

    FractalPart CreatePart(int childIndex) => new FractalPart
    {
        direction = directions[childIndex],
        rotation = rotations[childIndex]
    };

    #endregion

    #region 事件

    #endregion

    #region 属性

    #endregion


    #region 字段

    [Range(1, 8)]
    [SerializeField]
    private int depth = 4;

    [SerializeField]
    private Mesh mesh;

    [SerializeField]
    private Material material;

    private NativeArray<FractalPart>[] parts;
    private NativeArray<float4x4>[] matrices;

    static float3[] directions =
    {
        up(), right(), left(), forward(), back()
    };

    static quaternion[] rotations =
    {
        quaternion.identity,
        quaternion.RotateZ(-0.5f * PI), quaternion.RotateZ(0.5f * PI),
        quaternion.RotateX(0.5f * PI), quaternion.RotateX(-0.5f * PI)
    };

    private static MaterialPropertyBlock propertyBlock;

    #endregion

    #region Computer Shader 相关

    private ComputeBuffer[] matricesBuffers;

    private static readonly int matricesId = Shader.PropertyToID("_Matrices");

    #endregion
}