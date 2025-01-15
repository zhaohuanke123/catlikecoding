using UnityEngine;

/// <summary>
/// 塔防游戏中的防御塔类，负责攻击进入射程的enemy。
/// </summary>
public class LaserTower : Tower
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
        if (TrackTarget(ref m_target) || AcquireTarget(out m_target))
        {
            Shoot();
        }
        else
        {
            m_laserBeam.localScale = Vector3.zero;
        }
    }

    /// <summary>
    ///  激光塔的攻击方法，负责发射激光光束攻击敌人。
    /// </summary>
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

    #endregion

    #region 属性

    public override TowerType TowerType => TowerType.Laser;

    #endregion

    #region 字段

    /// <summary>
    /// 当前锁定的目标点，表示塔楼正在瞄准的enemy位置。
    /// 如果没有目标则为 null。
    /// </summary>
    private TargetPoint m_target;

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