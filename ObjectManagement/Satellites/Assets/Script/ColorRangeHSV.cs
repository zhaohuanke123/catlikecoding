using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
///  生成随机颜色, HSV 定义方式
/// </summary>
[Serializable]
public struct ColorRangeHSV
{
    /// <summary>
    ///  色相范围
    /// </summary>

    [FloatRangeSlider(0f, 1f)]
    public FloatRange m_hue;

    /// <summary>
    ///  饱和度范围
    /// </summary>

    [FloatRangeSlider(0f, 1f)]
    public FloatRange m_saturation;

    /// <summary>
    ///  亮度范围
    /// </summary>

    [FloatRangeSlider(0f, 1f)]
    public FloatRange m_value;

    #region 属性

    /// <summary>
    ///  随机颜色
    /// </summary>
    public Color RandomInRange => Random.ColorHSV(
        m_hue.m_min, m_hue.m_max,
        m_saturation.m_min, m_saturation.m_max,
        m_value.m_min, m_value.m_max,
        1f, 1f
    );

    #endregion
}