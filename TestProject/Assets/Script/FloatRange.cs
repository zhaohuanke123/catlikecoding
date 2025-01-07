using UnityEngine;

/// <summary>
///  用于生成随机值的范围
/// </summary>
[System.Serializable]
public struct FloatRange
{
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