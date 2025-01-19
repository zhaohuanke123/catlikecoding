using UnityEngine;

/// <summary>
/// 目标点类，用于标记enemy（Enemy）受击点位置。
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class TargetPoint : MonoBehaviour
{
    #region Unity 生命周期

    private void Awake()
    {
        Enemy = transform.root.GetComponent<Enemy>();

        Debug.Assert(Enemy != null, "Target point without Enemy root!", this);
        // Debug.Assert(GetComponent<SphereCollider>() != null, "Target point without sphere collider!", this);
        Debug.Assert(gameObject.layer == 9, "Target point on wrong layer!", this);

        Enemy.TargetPointCollider = GetComponent<Collider>();
    }

    #endregion

    #region 方法

    /// <summary>
    /// 静态方法，用于填充缓冲区，通过提供一个位置和范围来查找该区域内的所有目标点。
    /// </summary>
    /// <param name="position">搜索的中心位置。</param>
    /// <param name="range">搜索的半径范围。</param>
    /// <returns>返回布尔值，指示缓冲区是否至少包含一个目标点。</returns>
    public static bool FillBuffer(Vector3 position, float range)
    {
        Vector3 top = position;
        top.y += 3f;
        BufferedCount = Physics.OverlapCapsuleNonAlloc(position, top, range, s_buffer, EnemyLayerMask);
        return BufferedCount > 0;
    }

    /// <summary>
    /// 静态方法，从缓冲区中获取指定索引的目标点实例。
    /// </summary>
    /// <param name="index">缓冲区中的索引位置，用于检索目标点。</param>
    /// <returns>返回位于指定索引的目标点对象。</returns>
    public static TargetPoint GetBuffered(int index)
    {
        var target = s_buffer[index].GetComponent<TargetPoint>();

        Debug.Assert(target != null, "Targeted non-enemy!", s_buffer[0]);

        return target;
    }

    #endregion

    #region 属性

    /// <summary>
    ///  Enemy对象引用，表示当前TargetPoint所属的Enemy对象。
    /// </summary>
    public Enemy Enemy { get; private set; }

    /// <summary>
    /// 缓冲计数器，表示最近一次填充的缓冲区中的TargetPoint数量。
    /// </summary>
    public static int BufferedCount { get; private set; }

    /// <summary>
    ///  获取当前TargetPoint的位置。
    /// </summary>
    public Vector3 Position => transform.position;

    /// <summary>
    /// 随机获取缓冲区中的一个目标点实例。
    /// </summary>
    /// <returns>从已填充的目标点缓冲区中随机选择并返回一个目标点对象。</returns>
    public static TargetPoint RandomBuffered => GetBuffered(Random.Range(0, BufferedCount));

    #endregion

    #region 字段

    /// <summary>
    /// 敌人层遮罩常量。
    /// </summary>
    private const int EnemyLayerMask = 1 << 9;

    /// <summary>
    /// 静态缓存数组，用于存储与目标点碰撞检测相关的Collider组件。
    /// </summary>
    private static Collider[] s_buffer = new Collider[100];

    #endregion
}