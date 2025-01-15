using UnityEngine;

/// <summary>
/// 定制的范围滑条属性 
/// </summary>
public class FloatRangeSliderAttribute : PropertyAttribute
{
    #region 构造器

    /// <summary>
    /// 最小值和最大值作为参数，用于初始化属性。为保证范围合理，强制要求最大值不小于最小值。
    /// </summary>
    /// <param name="min">最小值</param>
    /// <param name="max">最大值</param>
    public FloatRangeSliderAttribute(float min, float max)
    {
        if (max < min)
        {
            max = min;
        }

        Min = min;
        Max = max;
    }

    #endregion


    #region 属性

    /// <summary>
    ///  最小值
    /// </summary>
    public float Min { get; private set; }

    /// <summary>
    ///  最大值
    /// </summary>
    public float Max { get; private set; }

    #endregion
}