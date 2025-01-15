using UnityEngine;

/// <summary>
/// 塔防游戏中的防御塔类，负责攻击进入射程的enemy。
/// </summary>
public abstract class Tower : GameTileContent
{
    #region Unity 生命周期

    /// <summary>
    /// 可视化显示攻击范围和目标点。
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = transform.localPosition;
        position.y += 0.01f;
        Gizmos.DrawWireSphere(position, m_targetingRange);
    }

    #endregion

    #region 方法

    /// <summary>
    /// 尝试获取一个攻击目标。
    /// </summary>
    /// <returns>
    /// 如果成功找到并锁定目标，则返回 true；否则返回 false。
    /// </returns>
    protected bool AcquireTarget(out TargetPoint target)
    {
        if (TargetPoint.FillBuffer(transform.localPosition, m_targetingRange))
        {
            target = TargetPoint.RandomBuffered;
            return true;
        }

        target = null;
        return false;
    }

    /// <summary>
    /// 跟踪已锁定的目标。
    /// </summary>
    /// <returns>
    /// 如果目标有效且仍在跟踪范围内，则返回 true；否则返回 false。
    /// </returns>
    protected bool TrackTarget(ref TargetPoint target)
    {
        // 1. 检查目标是否有效
        if (target == null || !target.Enemy.IsValidTarget)
        {
            return false;
        }

        // 2. 检查目标是否在范围内
        // 它依赖于毕达哥拉斯定理来计算二维距离，但省略了平方根。
        // 相反，它对半径进行平方，因此我们最终比较的是平方长度。这就足够了，因为我们只需要检查相对长度，而不需要精确的差异。
        Vector3 a = transform.localPosition;
        Vector3 b = target.Position;
        float x = a.x - b.x;
        float z = a.z - b.z;
        float r = m_targetingRange + 0.125f * target.Enemy.Scale;
        if (x * x + z * z > r * r)
        {
            target = null;
            return false;
        }

        return true;
    }

    #endregion

    #region 字段

    /// <summary>
    /// 攻击范围半径，决定塔楼可攻击的区域大小。
    /// </summary>
    [SerializeField]
    [Range(1.5f, 10.5f)]
    protected float m_targetingRange = 1.5f;

    /// <summary>
    ///  当前tower的 类型
    /// </summary>
    public abstract TowerType TowerType { get; }

    #endregion
}