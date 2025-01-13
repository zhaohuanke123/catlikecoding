using UnityEngine;

public class FloatRangeSliderAttribute : PropertyAttribute
{
    #region 构造器

    public FloatRangeSliderAttribute(float min, float max)
    {
        Min = min;
        Max = max < min ? min : max;
    }

    #endregion

    #region 属性

    public float Min { get; private set; }

    public float Max { get; private set; }

    #endregion
}