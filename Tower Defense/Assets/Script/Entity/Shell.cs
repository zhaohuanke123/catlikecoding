using UnityEngine;

/// <summary>
///  炮弹实体类，负责处理炮弹的行为逻辑。
/// </summary>
public class Shell : WarEntity
{
    #region 方法

    /// <summary>
    /// 初始化炮弹的发射参数。
    /// </summary>
    /// <param name="launchPoint">发射位置的三维坐标。</param>
    /// <param name="targetPoint">目标位置的三维坐标。</param>
    /// <param name="launchVelocity">炮弹的初始发射速度向量。</param>
    /// <param name="blastRadius">爆炸半径。</param>
    /// <param name="damage">炮弹造成的伤害值。</param>
    public void Initialize(Vector3 launchPoint, Vector3 targetPoint, Vector3 launchVelocity, float blastRadius,
        float damage)
    {
        m_launchPoint = launchPoint;
        m_targetPoint = targetPoint;
        m_launchVelocity = launchVelocity;
        m_blastRadius = blastRadius;
        m_damage = damage;
    }

    public override bool GameUpdate()
    {
        // 1. 更新炮弹的存在时间
        m_age += Time.deltaTime;

        // 2. 计算炮弹的位置
        Vector3 p = m_launchPoint + m_launchVelocity * m_age;
        p.y -= 0.5f * 9.81f * m_age * m_age;
        transform.localPosition = p;

        // 3. 计算炮弹的旋转
        Vector3 d = m_launchVelocity;
        d.y -= 9.81f * m_age;

        // 4. 判断炮弹是否已经落地
        if (p.y <= 0f)
        {
            // 5. 炮弹爆炸检查受击 Enemy
            Game.SpawnExplosion().Initialize(m_targetPoint, m_blastRadius, m_damage);
            OriginFactory.Reclaim(this);
            return false;
        }

        transform.localRotation = Quaternion.LookRotation(d);

        Game.SpawnExplosion().Initialize(p, 0.1f);
        return true;
    }

    #endregion

    #region 字段

    /// <summary>
    /// 发射点的坐标。
    /// </summary>
    private Vector3 m_launchPoint;

    /// <summary>
    /// 目标点的坐标，用于计算炮弹的飞行轨迹及方向。
    /// </summary>
    private Vector3 m_targetPoint;

    /// <summary>
    /// 发射速度。
    /// </summary>
    private Vector3 m_launchVelocity;

    /// <summary>
    /// 炮弹存在的时间（秒）。
    /// </summary>
    private float m_age;

    /// <summary>
    /// 爆炸半径
    /// </summary>
    private float m_blastRadius;

    /// <summary>
    /// 炮弹造成的伤害值。
    /// </summary>
    private float m_damage;

    #endregion
}