using UnityEngine;

/// <summary>
/// Shape的生命周期配置, 生成shape时scale 从0到1
/// </summary>
[System.Serializable]
public struct LifecycleConfiguration
{
    #region 属性

    /// <summary>
    /// 随机时长集合，用于配置shape生成过程中的不同阶段时长。
    /// </summary>
    public Vector3 RandomDurations => new(
        m_growingDuration.RandomValueInRange,
        m_adultDuration.RandomValueInRange,
        m_dyingDuration.RandomValueInRange);

    #endregion

    #region 字段

    /// <summary>
    ///  生成时长范围
    /// </summary>
    [FloatRangeSlider(0f, 2f)]
    public FloatRange m_growingDuration;

    /// <summary>
    /// 成年时长范围
    /// </summary>
    [FloatRangeSlider(0f, 100f)]
    public FloatRange m_adultDuration;

    /// <summary>
    /// 死亡时长范围
    /// </summary>
    [FloatRangeSlider(0f, 2f)]
    public FloatRange m_dyingDuration;

    #endregion
}