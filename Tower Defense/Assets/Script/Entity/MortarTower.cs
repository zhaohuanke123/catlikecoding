using UnityEngine;
using UnityEngine.Serialization;

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

    public override void GameUpdate()
    {
        m_launchProgress += m_shotsPerSecond * Time.deltaTime;
        while (m_launchProgress >= 1f)
        {
            if (AcquireTarget(out TargetPoint target))
            {
                Launch(target);
                m_launchProgress -= 1f;
            }
            else
            {
                m_launchProgress = 0.999f;
            }
        }
    }

    #endregion

    #region 方法

    /// <summary>
    /// 
    /// </summary>
    public void Launch(TargetPoint target)
    {
        Vector3 launchPoint = m_mortar.position;
        Vector3 targetPoint = target.Position;
        targetPoint.y = 0f;

        Vector2 dir;
        dir.x = targetPoint.x - launchPoint.x;
        dir.y = targetPoint.z - launchPoint.z;
        float x = dir.magnitude;
        float y = -launchPoint.y;
        dir /= x;

        float g = 9.81f;
        float s = m_launchSpeed;
        float s2 = s * s;

        float r = s2 * s2 - g * (g * x * x + 2f * y * s2);

        Debug.Assert(r >= 0f, "Launch velocity insufficient for range!");

        float tanTheta = (s2 + Mathf.Sqrt(r)) / (g * x);
        float cosTheta = Mathf.Cos(Mathf.Atan(tanTheta));
        float sinTheta = cosTheta * tanTheta;

        Vector3 prev = launchPoint, next;
        for (int i = 1; i <= 10; i++)
        {
            float t = i / 10f;
            float dx = s * cosTheta * t;
            float dy = s * sinTheta * t - 0.5f * g * t * t;
            next = launchPoint + new Vector3(dir.x * dx, dy, dir.y * dx);
            Debug.DrawLine(prev, next, Color.blue);
            prev = next;
        }

        Debug.DrawLine(launchPoint, targetPoint, Color.yellow);
        Debug.DrawLine(
            new Vector3(launchPoint.x, 0.01f, launchPoint.z),
            new Vector3(launchPoint.x + dir.x * x, 0.01f, launchPoint.z + dir.y * x),
            Color.white
        );
    }

    #endregion

    #region 事件

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
    /// 
    /// </summary>
    private float m_launchSpeed;

    #endregion
}