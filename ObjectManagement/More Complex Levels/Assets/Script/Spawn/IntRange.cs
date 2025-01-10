using UnityEngine;

/// <summary>
///  用于生成随机值的整形范围
/// </summary>
[System.Serializable]
public struct IntRange
{
    /// <summary>
    ///  最小值
    /// </summary>
    public int m_min;

    /// <summary>
    ///  最大值
    /// </summary>
    public int m_max;

    /// <summary>
    ///  返回一个在范围内的随机值
    /// </summary>
    public int RandomValueInRange => Random.Range(m_min, m_max + 1);
}