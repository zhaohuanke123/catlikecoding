using UnityEngine;

/// <summary>
/// 塔防游戏中的防御塔类，负责攻击进入射程的enemy。
/// </summary>
public class Tower : GameTileContent
{
    #region Unity 生命周期

    private void Awake()
    {
        m_laserBeamScale = m_laserBeam.localScale;
    }

    /// <summary>
    /// 可视化显示攻击范围和目标点。
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = transform.localPosition;
        position.y += 0.01f;
        Gizmos.DrawWireSphere(position, m_targetingRange);

        if (m_target != null)
        {
            Gizmos.DrawLine(position, m_target.Position);
        }
    }

    #endregion

    #region 方法

    public override void GameUpdate()
    {
        // 1. 如果有目标则搜索到目标点
        if (TrackTarget() || AcquireTarget())
        {
            Shoot();
        }
        else
        {
            m_laserBeam.localScale = Vector3.zero;
        }
    }

    private void Shoot()
    {
        // 1. 瞄准目标
        Vector3 point = m_target.Position;
        m_turret.LookAt(point);
        m_laserBeam.localRotation = m_turret.localRotation;

        // 2. 计算射线长度
        float d = Vector3.Distance(m_turret.position, point);
        m_laserBeamScale.z = d;
        // 3. 设置激光射线长度和位置
        m_laserBeam.localScale = m_laserBeamScale;
        m_laserBeam.localPosition = m_turret.localPosition + 0.5f * d * m_laserBeam.forward;

        m_target.Enemy.ApplyDamage(m_damagePerSecond * Time.deltaTime);
    }

    /// <summary>
    /// 尝试获取一个攻击目标。
    /// </summary>
    /// <returns>
    /// 如果成功找到并锁定目标，则返回 true；否则返回 false。
    /// </returns>
    private bool AcquireTarget()
    {
        Vector3 a = transform.localPosition;
        Vector3 b = a;
        b.y += 3f;
        // 1. overlap 检测周围的enemy
        int hits = Physics.OverlapCapsuleNonAlloc(a, b, m_targetingRange, s_targetsBuffer, EnemyLayerMask);
        if (hits > 0)
        {
            int randomIndex = Random.Range(0, hits);
            // 2. 锁定第一个enemy
            m_target = s_targetsBuffer[randomIndex].GetComponent<TargetPoint>();
            Debug.Assert(m_target != null, "Targeted non-enemy!", s_targetsBuffer[0]);

            return true;
        }

        m_target = null;
        return false;
    }

    /// <summary>
    /// 跟踪已锁定的目标。
    /// </summary>
    /// <returns>
    /// 如果目标有效且仍在跟踪范围内，则返回 true；否则返回 false。
    /// </returns>
    private bool TrackTarget()
    {
        // 1. 检查目标是否有效
        if (m_target == null)
        {
            return false;
        }

        // 2. 检查目标是否在范围内
        // 它依赖于毕达哥拉斯定理来计算二维距离，但省略了平方根。
        // 相反，它对半径进行平方，因此我们最终比较的是平方长度。这就足够了，因为我们只需要检查相对长度，而不需要精确的差异。
        Vector3 a = transform.localPosition;
        Vector3 b = m_target.Position;
        float x = a.x - b.x;
        float z = a.z - b.z;
        float r = m_targetingRange + 0.125f * m_target.Enemy.Scale;
        if (x * x + z * z > r * r)
        {
            m_target = null;
            return false;
        }

        return true;
    }

    #endregion

    #region 字段

    private const int EnemyLayerMask = 1 << 9;

    /// <summary>
    /// 攻击范围半径，决定塔楼可攻击的区域大小。
    /// </summary>
    [SerializeField]
    [Range(1.5f, 10.5f)]
    private float m_targetingRange = 1.5f;

    /// <summary>
    /// 当前锁定的目标点，表示塔楼正在瞄准的enemy位置。
    /// 如果没有目标则为 null。
    /// </summary>
    private TargetPoint m_target;

    /// <summary>
    ///  用于缓存 Physics.OverlapCapsule 的结果。
    /// </summary>
    private static Collider[] s_targetsBuffer = new Collider[100];

    /// <summary>
    /// 塔楼模型的旋转部件Transform组件，负责调整炮塔的朝向以瞄准目标。
    /// </summary>
    [SerializeField]
    private Transform m_turret = default;

    /// <summary>
    /// 激光光束的Transform组件，用于在场景中显示激光效果。
    /// </summary>
    [SerializeField]
    private Transform m_laserBeam = default;

    /// <summary>
    /// 激光光束的缩放比例，用于控制激光的视觉长度。
    /// 在<see cref="Shoot"/>方法中计算得出，根据到目标的距离实时调整激光的长度。
    /// </summary>
    private Vector3 m_laserBeamScale;

    /// <summary>
    /// 每秒造成的伤害值，决定了塔楼攻击的强度。
    /// </summary>
    [SerializeField]
    [Range(1f, 100f)]
    private float m_damagePerSecond = 10f;

    #endregion
}