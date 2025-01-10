using UnityEngine;

/// <summary>
/// 成长行为类，用于控制shape对象随时间逐渐增大的行为。
/// </summary>
public class GrowingShapeBehavior : ShapeBehavior
{
    #region 方法

    public override bool GameUpdate(Shape shape)
    {
        // 1. 小于持续时间，就继续变大 
        if (shape.Age < m_duration)
        {
            float s = shape.Age / m_duration;
            s = (3f - 2f * s) * s * s;
            shape.transform.localScale = s * m_originalScale;
            return true;
        }

        // 2. 如果shape已完成增长，将其缩放直接设置成初始设置的大小
        shape.transform.localScale = m_originalScale;
        // 3. 返回false表示shape已结束增长
        return false;
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(m_originalScale);
        writer.Write(m_duration);
    }

    public override void Load(GameDataReader reader)
    {
        m_originalScale = reader.ReadVector3();
        m_duration = reader.ReadFloat();
    }

    public override void Recycle()
    {
        ShapeBehaviorPool<GrowingShapeBehavior>.Reclaim(this);
    }

    /// <summary>
    /// 初始化shape的行为状态，设置其原始缩放大小及成长持续时间，并将当前缩放设为零以开始成长过程。
    /// </summary>
    /// <param name="shape">要应用成长行为的shape对象。</param>
    /// <param name="duration">shape从缩放为零增长至原始大小所需的时间（秒）。</param>
    public void Initialize(Shape shape, float duration)
    {
        m_originalScale = shape.transform.localScale;
        m_duration = duration;
        shape.transform.localScale = Vector3.zero;
    }

    #endregion

    #region 属性

    public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Growing;

    #endregion

    #region 字段

    /// <summary>
    /// 生成shape的初始大小，最终shape的scale会从 0 变成  这个初始大小
    /// </summary>
    private Vector3 m_originalScale;

    /// <summary>
    /// 变大的持续时间 
    /// </summary>
    private float m_duration;

    #endregion
}

public class DyingShapeBehavior : ShapeBehavior
{
    #region 方法

    public override bool GameUpdate(Shape shape)
    {
        // 1. 计算衰减持续时间
        float dyingDuration = shape.Age - m_dyingAge;

        // 2. 如果衰减持续时间小于指定的衰减总时长，则继续衰减过程
        if (dyingDuration < m_duration)
        {
            float s = 1f - dyingDuration / m_duration;
            s = (3f - 2f * s) * s * s;
            // 应用衰减后的缩放比例
            shape.transform.localScale = s * m_originalScale;
            return true;
        }

        // 2. 衰减完成
        shape.Die();
        return true;
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(m_originalScale);
        writer.Write(m_duration);
        writer.Write(m_dyingAge);
    }

    public override void Load(GameDataReader reader)
    {
        m_originalScale = reader.ReadVector3();
        m_duration = reader.ReadFloat();
        m_dyingAge = reader.ReadFloat();
    }

    public override void Recycle()
    {
        ShapeBehaviorPool<DyingShapeBehavior>.Reclaim(this);
    }

    /// <summary>
    /// 初始化shape的行为状态，设置其原始缩放大小及成长持续时间，并将当前缩放设为零以开始成长过程。
    /// </summary>
    /// <param name="shape">要应用成长行为的shape对象。</param>
    /// <param name="duration">shape从缩放为零增长至原始大小所需的时间（秒）。</param>
    public void Initialize(Shape shape, float duration)
    {
        m_originalScale = shape.transform.localScale;
        m_duration = duration;
        m_dyingAge = shape.Age;
        shape.MarkAsDying();
    }

    #endregion

    #region 属性

    public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Dying;

    #endregion

    #region 字段

    /// <summary>
    /// 初始shape的缩放大小。
    /// 在shape成长过程中，其缩放将从零逐渐变为该初始大小。
    /// </summary>
    private Vector3 m_originalScale;

    /// <summary>
    /// shape增长持续的时间（秒）。
    /// 表示shape从开始生长到达到其初始大小所需的时间。
    /// </summary>
    private float m_duration;

    /// <summary>
    /// 生命终止age，用于标记shape应当消失的逻辑age。
    /// </summary>
    /// <remarks>
    /// 此属性可以用于控制shape在游戏中的生命周期，当shape的<see cref="Shape.Age"/>达到或超过此值时，
    /// 可能会触发shape的销毁或其他相关逻辑。
    /// </remarks>
    private float m_dyingAge;

    #endregion
}