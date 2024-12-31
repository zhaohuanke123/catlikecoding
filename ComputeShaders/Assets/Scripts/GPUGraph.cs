using UnityEngine;
using UnityEngine.Serialization;


/// <summary>
///  切换图像函数索引的模式
/// </summary>
public enum TransitionMode
{
    Cycle,
    Random
}

/// <summary>
/// 使用GPU生成图形生成类
/// </summary>
public class GPUGraph : MonoBehaviour
{
    #region Unity生命周期

    private void Awake()
    {
        m_positionsBuffer = new ComputeBuffer(MaxResolution * MaxResolution, 3 * 4);
    }

    private void Update()
    {
        // 1. 统计经过时间
        m_duration += Time.deltaTime;

        // 2. 判断是否在过渡
        if (m_transitioning)
        {
            if (m_duration >= m_transitionDuration)
            {
                m_duration -= m_transitionDuration;
                m_transitioning = false;
            }
        }
        // 3. 不在过度，判断是否切换函数
        else if (m_duration >= m_functionDuration)
        {
            m_duration -= m_functionDuration;
            m_transitioning = true;
            m_transitionFunctionIndex = m_functionIndex;
            // 4. 计时获取下一个函数索引
            PickNextFunction();
        }

        UpdateFunctionOnGPU();
    }

    private void OnDisable()
    {
        m_positionsBuffer.Release();
        m_positionsBuffer = null;
    }

    #endregion

    #region 方法

    /// <summary>
    ///  选择下一个函数
    /// </summary>
    private void PickNextFunction()
    {
        m_functionIndex = m_transitionMode == TransitionMode.Cycle
            ? FunctionLibrary.GetNextFunctionIndex(m_functionIndex)
            : FunctionLibrary.GetRandomFunctionIndex(m_functionIndex);
    }

    private void UpdateFunctionOnGPU()
    {
        // 1. 设置参数
        // 计算每个采样点的步长，步长为 2 / 分辨率
        float step = 2f / m_resolution;

        m_computeShader.SetInt(ResolutionId, m_resolution);
        m_computeShader.SetFloat(StepId, step);
        m_computeShader.SetFloat(TimeId, Time.time);

        // 2. 设置过渡动画进度（如果有过渡）
        if (m_transitioning)
        {
            // 使用平滑步进函数计算过渡进度
            m_computeShader.SetFloat(
                TransitionProgressId,
                Mathf.SmoothStep(0f, 1f, m_duration / m_transitionDuration)
            );
        }

        // 3. 设置 positionsBuffer 到 Compute Shader 的输入缓存
        m_computeShader.SetBuffer(0, PositionsId, m_positionsBuffer);

        // 4. 选择当前函数和过渡函数（如果有过渡）
        int function = m_functionIndex % FunctionLibrary.FunctionCount;
        int transitionFunction = m_transitionFunctionIndex % FunctionLibrary.FunctionCount;
        // 根据是否过渡，选择对应的函数
        var kernelIndex =
            function + (m_transitioning ? transitionFunction : function) * FunctionLibrary.FunctionCount;
        m_computeShader.SetBuffer(kernelIndex, PositionsId, m_positionsBuffer);

        // 5. 计算并设置 Dispatch 的线程组数量
        int groups = Mathf.CeilToInt(m_resolution / 8f);
        m_computeShader.Dispatch(kernelIndex, groups, groups, 1);

        // 6. 设置材质的参数
        m_material.SetBuffer(PositionsId, m_positionsBuffer);
        m_material.SetFloat(StepId, step);

        // 7. 绘制实例化网格（使用 GPU 计算结果）
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / m_resolution));
        Graphics.DrawMeshInstancedProcedural(m_mesh, 0, m_material, bounds, m_resolution * m_resolution);
    }

    #endregion

    #region 字段

    /// <summary>
    ///  函数索引
    /// </summary>
    [HideInInspector]
    public int m_functionIndex;

    /// <summary>
    ///  最大分辨率
    /// </summary>
    private const int MaxResolution = 1000;

    /// <summary>
    /// 函数分辨率
    /// </summary>
    [SerializeField]
    [Range(10, MaxResolution)]
    private int m_resolution = 10;

    /// <summary>
    ///  函数切换模式
    /// </summary>
    [SerializeField]
    private TransitionMode m_transitionMode;

    /// <summary>
    ///  函数持续时间
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float m_functionDuration = 1f;

    /// <summary>
    ///  函数切换过程时间
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float m_transitionDuration = 1f;

    /// <summary>
    /// 单个函数持续时间计时器
    /// </summary>
    private float m_duration;

    /// <summary>
    ///  是否正在过渡
    /// </summary>
    private bool m_transitioning;

    /// <summary>
    ///  正在过渡的函数索引
    /// </summary>
    private int m_transitionFunctionIndex;

    #endregion

    #region ComputerShader 相关字段

    /// <summary>
    ///  位置缓存
    /// </summary>
    private ComputeBuffer m_positionsBuffer;

    /// <summary>
    ///  Compute Shader实例
    /// </summary>
    [SerializeField]
    private ComputeShader m_computeShader;

    /// <summary>
    ///  材质参数-位置
    /// </summary>
    private static readonly int PositionsId = Shader.PropertyToID("_Positions");

    /// <summary>
    ///   材质参数-分辨率
    /// </summary>
    private static readonly int ResolutionId = Shader.PropertyToID("_Resolution");

    /// <summary>
    ///  材质参数-步长
    /// </summary>
    private static readonly int StepId = Shader.PropertyToID("_Step");

    /// <summary>
    ///  材质参数-时间
    /// </summary>
    private static readonly int TimeId = Shader.PropertyToID("_Time");

    /// <summary>
    ///  材质参数-过渡进度
    /// </summary>
    private static readonly int TransitionProgressId = Shader.PropertyToID("_TransitionProgress");

    /// <summary>
    ///  GPU Instance 使用的 Material 
    /// </summary>
    [SerializeField]
    private Material m_material;

    /// <summary>
    ///  GPU Instance 使用的 Mesh
    /// </summary>
    [SerializeField]
    private Mesh m_mesh;

    #endregion
}