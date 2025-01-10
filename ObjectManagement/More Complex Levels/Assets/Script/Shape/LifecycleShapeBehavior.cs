using UnityEngine;

/// <summary>
/// 生命周期shape行为类，用于控制shape对象从成年到衰亡的各个阶段。
/// </summary>
public class LifecycleShapeBehavior : ShapeBehavior
{
    #region 方法

    public override bool GameUpdate(Shape shape)
    {
        // 1. 成年阶段
        if (shape.Age >= m_dyingAge)
        {
            // 1.1 如果衰减持续时间小于等于0，则直接销毁shape
            if (m_dyingDuration <= 0f)
            {
                shape.Die();
                return true;
            }

            // 1.2 如果shape未标记为死亡，则添加死亡行为
            if (!shape.IsMarkedAsDying)
            {
                shape.AddBehavior<DyingShapeBehavior>().Initialize(shape, m_dyingDuration + m_dyingAge - shape.Age);
            }

            return false;
        }

        return true;
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(m_adultDuration);
        writer.Write(m_dyingDuration);
        writer.Write(m_dyingAge);
    }

    public override void Load(GameDataReader reader)
    {
        m_adultDuration = reader.ReadFloat();
        m_dyingDuration = reader.ReadFloat();
        m_dyingAge = reader.ReadFloat();
    }

    public override void Recycle()
    {
        ShapeBehaviorPool<LifecycleShapeBehavior>.Reclaim(this);
    }

    /// <summary>
    /// 初始化shape的行为以控制其生命周期的不同阶段。
    /// 此方法设置了shape的成年、衰减持续时间，并根据需要添加了<see cref="GrowingShapeBehavior"/>。
    /// </summary>
    /// <param name="shape">shape实例，该行为将应用于这个shape。</param>
    /// <param name="growingDuration">成长阶段的持续时间。如果大于0，则会为shape添加成长行为。</param>
    /// <param name="adultDuration">成年阶段的持续时间。指定了shape维持成熟状态的时长。</param>
    /// <param name="dyingDuration">衰减阶段的持续时间。定义了shape在标记为死亡后，直至消失的持续时间。</param>
    public void Initialize(Shape shape, float growingDuration, float adultDuration, float dyingDuration)
    {
        // 1. 保存组件数据
        m_adultDuration = adultDuration;
        m_dyingDuration = dyingDuration;
        m_dyingAge = growingDuration + adultDuration;

        // 2. 添加成长组件行为
        if (growingDuration > 0f)
        {
            shape.AddBehavior<GrowingShapeBehavior>().Initialize(shape, growingDuration);
        }
    }

    #endregion

    #region 属性

    public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Lifecycle;

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
    /// 成熟阶段持续时间。
    /// 指定了shape在生命周期中保持成熟状态的时间长度。
    /// </summary>
    private float m_adultDuration;

    /// <summary>
    /// 生命衰减持续时间。
    /// 指定shape在标记为死亡后，持续衰减直至消失的时长。
    /// </summary>
    private float m_dyingDuration;

    /// <summary>
    /// 生命结束age。
    /// 当shape的age达到此值时，将开始进入死亡阶段或直接销毁。
    /// </summary>
    private float m_dyingAge;

    #endregion
}