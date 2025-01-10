/// <summary>
///  卫星配置
/// </summary>
[System.Serializable]
public struct SatelliteConfiguration
{
    /// <summary>
    ///  相对缩放范围 
    /// </summary>
    [FloatRangeSlider(0.1f, 1f)]
    public FloatRange m_relativeScale;

    /// <summary>
    ///  卫星环绕半径范围   
    /// </summary>
    public FloatRange m_orbitRadius;

    /// <summary>
    ///  卫星环绕频率范围
    /// </summary>
    public FloatRange m_orbitFrequency;

    /// <summary>
    ///  生成数量范围
    /// </summary>
    public IntRange m_amount;

    /// <summary>
    /// 是否为所有卫星启用统一生命周期管理
    /// </summary>
    public bool m_uniformLifecycles;
}