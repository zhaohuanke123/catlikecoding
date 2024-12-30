using UnityEngine;

public class GPUGraph : MonoBehaviour
{
    #region 方法

    private void Awake()
    {
        m_points = new Transform[m_resolution * m_resolution];

        // 1. 根据分辨率创建函数点，计算步长和缩放 
        float step = 2f / m_resolution;
        var scale = Vector3.one * step;

        // 2. 创建函数点
        for (int i = 0; i < m_points.Length; i++)
        {
            Transform point = Instantiate(m_pointPrefab, transform, false);
            m_points[i] = point;

            point.localScale = scale;
        }
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

        // 5. 判断是否在过渡
        if (m_transitioning)
        {
            UpdateFunctionTransition();
        }
        else
        {
            UpdateFunction();
        }
    }

    /// <summary>
    ///  选择下一个函数
    /// </summary>
    private void PickNextFunction()
    {
        m_functionIndex = m_transitionMode == TransitionMode.Cycle
            ? FunctionLibrary.GetNextFunctionIndex(m_functionIndex)
            : FunctionLibrary.GetRandomFunctionIndex(m_functionIndex);
    }

    /// <summary>
    /// 驱动函数图核心逻辑,用于显示单个函数
    /// </summary>
    private void UpdateFunction()
    {
        // 1. 获取当前时间
        var time = Time.time;

        // 2. 计算函数点的位置
        float step = 2f / m_resolution;
        float v = 0.5f * step - 1f;

        for (int i = 0, x = 0, z = 0; i < m_points.Length; i++, x++)
        {
            // 3. 如果x等于分辨率，则重新计算v
            if (x == m_resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }

            float u = (x + 0.5f) * step - 1f;
            // 4. 计算函数点的位置
            m_points[i].localPosition = FunctionLibrary.GetFunctionValue(m_functionIndex, u, v, time);
        }
    }


    /// <summary>
    ///  驱动函数图核心逻辑,用于显示函数过渡
    /// </summary>
    private void UpdateFunctionTransition()
    {
        // 1. 获取当前时间
        float time = Time.time;

        // 2. 计算函数点的位置
        float step = 2f / m_resolution;
        float v = 0.5f * step - 1f;
        float progress = m_duration / m_transitionDuration;

        for (int i = 0, x = 0, z = 0; i < m_points.Length; i++, x++)
        {
            // 3. 如果x等于分辨率，则重新计算v
            if (x == m_resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }

            float u = (x + 0.5f) * step - 1f;
            // 4. 计算函数点的位置, 过渡函数
            m_points[i].localPosition =
                FunctionLibrary.Morph(u, v, time, m_transitionFunctionIndex, m_functionIndex, progress);
        }
    }

    #endregion

    #region 字段

    /// <summary>
    ///  函数索引
    /// </summary>
    [HideInInspector]
    public int m_functionIndex;


    /// <summary>
    ///  函数点预制体
    /// </summary>
    [SerializeField]
    private Transform m_pointPrefab;

    /// <summary>
    /// 函数分辨率
    /// </summary>
    [SerializeField]
    [Range(10, 1000)]
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
    ///  函数点数组
    /// </summary>
    private Transform[] m_points;

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
}