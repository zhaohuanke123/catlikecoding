using UnityEngine;

/// <summary>
/// 迫击炮塔类，远距离曲射攻击。
/// 通过计算抛物线轨迹，适用于覆盖较广区域或攻击遮挡后的目标。
/// </summary>
public class MortarTower : Tower
{
    #region Unity 生命周期

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        float x = m_targetingRange + 0.25001f;
        float y = -m_mortar.position.y;
        m_launchSpeed = Mathf.Sqrt(9.81f * (y + Mathf.Sqrt(x * x + y * y)));
    }

    #endregion

    public override void GameUpdate()
    {
        // 1. 更新发射进度
        m_launchProgress += m_shotsPerSecond * Time.deltaTime;
        while (m_launchProgress >= 1f)
        {
            // 2. 获取目标
            if (AcquireTarget(out TargetPoint target))
            {
                // 3. 发射炮弹
                Launch(target);
                m_launchProgress -= 1f;
            }
            // 4. 减少一点，下次循环继续发射
            else
            {
                m_launchProgress = 0.999f;
            }
        }
    }

    #region 方法

    /// <summary>
    /// 发射一枚炮弹至指定的目标点。
    /// </summary>
    /// <param name="target">目标点对象，包含了敌人的受击位置信息。</param>
    public void Launch(TargetPoint target)
    {
        // 1. 获取发射点和目标点
        Vector3 launchPoint = m_mortar.position;
        Vector3 targetPoint = target.Position;
        targetPoint.y = 0f;

        // 2. 计算发射方向
        Vector2 dir;
        dir.x = targetPoint.x - launchPoint.x;
        dir.y = targetPoint.z - launchPoint.z;
        float x = dir.magnitude;
        float y = -launchPoint.y;
        dir /= x;

        // 3. 计算发射角度
        float g = 9.81f;
        float s = m_launchSpeed;
        float s2 = s * s;

        float r = s2 * s2 - g * (g * x * x + 2f * y * s2);

        Debug.Assert(r >= 0f, "Launch velocity insufficient for range!");

        float tanTheta = (s2 + Mathf.Sqrt(r)) / (g * x);
        float cosTheta = Mathf.Cos(Mathf.Atan(tanTheta));
        float sinTheta = cosTheta * tanTheta;

        // 4. Debug绘制发射轨迹
        Vector3 prev = launchPoint, next;
        for (int i = 1; i <= 10; i++)
        {
            float t = i / 10f;
            float dx = s * cosTheta * t;
            float dy = s * sinTheta * t - 0.5f * g * t * t;
            next = launchPoint + new Vector3(dir.x * dx, dy, dir.y * dx);
            Debug.DrawLine(prev, next, Color.blue, 1f);
            prev = next;
        }

        // 5. 设置Tower旋转
        m_mortar.localRotation = Quaternion.LookRotation(new Vector3(dir.x, tanTheta, dir.y));

        Debug.DrawLine(launchPoint, targetPoint, Color.yellow, 1f);
        Debug.DrawLine(
            new Vector3(launchPoint.x, 0.01f, launchPoint.z),
            new Vector3(launchPoint.x + dir.x * x, 0.01f, launchPoint.z + dir.y * x),
            Color.white,
            1f
        );

        // 6. 实例化炮弹并初始化
        Game.SpawnShell().Initialize(launchPoint, targetPoint,
            new Vector3(s * cosTheta * dir.x, s * sinTheta, s * cosTheta * dir.y),
            m_shellBlastRadius, m_shellDamage);
    }

    #endregion

    #region 属性

    public override TowerType TowerType => TowerType.Mortar;

    #endregion

    #region 字段

    /// <summary>
    /// 发射次数，表示MortarTower单位时间内发射炮弹的频率。
    /// </summary>
    [SerializeField]
    [Range(0.5f, 2f)]
    private float m_shotsPerSecond = 1f;

    /// <summary>
    /// Transform 类型，代表炮塔的发射器位置变换组件。
    /// </summary>
    [SerializeField]
    private Transform m_mortar = default;

    /// <summary>
    /// 发射进度，表示当前积累的发射准备时间与一次发射所需时间的比例。
    /// 当此值达到1时，MortarTower将发射一次炮弹，并重置此进度值。
    /// </summary>
    private float m_launchProgress;

    /// <summary>
    /// 发射速度，表示炮弹的发射速度。 
    /// </summary>
    private float m_launchSpeed;

    /// <summary>
    /// 炮弹爆炸半径配置
    /// </summary>
    [SerializeField]
    [Range(0.5f, 3f)]
    private float m_shellBlastRadius = 1f;

    /// <summary>
    /// 炮弹伤害配置 
    /// </summary>
    [SerializeField]
    [Range(1f, 100f)]
    private float m_shellDamage = 10f;

    #endregion
}