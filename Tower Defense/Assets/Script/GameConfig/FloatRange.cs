using UnityEngine;

/// <summary>
///  用于生成随机值的范围
/// </summary>
[System.Serializable]
public struct FloatRange
{
    #region 构造器

    /// <summary>
    /// 构造一个固定值的范围
    /// </summary>
    /// <param name="value">生成的固定值</param>
    public FloatRange(float value)
    {
        m_min = m_max = value;
    }

    /// <summary>
    /// 构造一个范围 
    /// </summary>
    /// <param name="min">最小值</param>
    /// <param name="max">最大值</param>
    public FloatRange(float min, float max)
    {
        m_min = min;
        m_max = max < min ? min : max;
    }

    #endregion

    #region 属性

    /// <summary>
    ///  返回一个在范围内的随机值
    /// </summary>
    public float RandomValueInRange => Random.Range(m_min, m_max);

    #endregion

    #region 字段

    /// <summary>
    ///  最小值
    /// </summary>
    public float m_min;

    /// <summary>
    ///  最大值
    /// </summary>
    public float m_max;

    #endregion
}