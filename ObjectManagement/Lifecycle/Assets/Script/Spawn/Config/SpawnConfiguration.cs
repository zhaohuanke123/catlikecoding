using UnityEngine.Serialization;

/// <summary>
/// 生成点配置
/// </summary>
[System.Serializable]
public struct SpawnConfiguration
{
    /// <summary>
    ///  生成物体的工厂数组
    /// </summary>
    public ShapeFactory[] m_factories;

    /// <summary>
    ///  移动方向
    /// </summary>
    public MovementDirection m_movementDirection;

    /// <summary>
    ///  速度范围
    /// </summary>
    public FloatRange m_speed;

    /// <summary>
    ///  角速度范围
    /// </summary>
    public FloatRange m_angularSpeed;

    /// <summary>
    ///  缩放范围
    /// </summary>
    public FloatRange m_scale;

    /// <summary>
    ///  颜色范围
    /// </summary>
    public ColorRangeHSV m_color;

    /// <summary>
    ///  组合物体各个部分是否使用相同的颜色
    /// </summary>
    public bool m_uniformColor;

    /// <summary>
    ///  震动方向
    /// </summary>
    public MovementDirection m_oscillationDirection;

    /// <summary>
    ///  震动幅度
    /// </summary>
    public FloatRange m_oscillationAmplitude;

    /// <summary>
    ///  震动频率
    /// </summary>
    public FloatRange m_oscillationFrequency;

    /// <summary>
    ///  卫星配置
    /// </summary>
    public SatelliteConfiguration m_satellite;

    /// <summary>
    ///  生命周期配置
    /// </summary>
    public LifecycleConfiguration m_lifecycle;
}