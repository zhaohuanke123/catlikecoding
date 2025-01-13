using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Enemy对象工厂，继承自GameObjectFactory，用于创建和回收Enemy实例。
/// </summary>
[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory
{
    #region 方法

    /// <summary>
    /// 创建并初始化一个新的Enemy实例，并将其与当前工厂关联。
    /// </summary>
    /// <returns>一个新创建并初始化的Enemy实例。</returns>
    public Enemy Get()
    {
        // 1. 创建Enemy实例
        Enemy instance = CreateGameObjectInstance(m_prefab);
        instance.OriginFactory = this;
        // 2. 初始化Enemy实例
        instance.Initialize(m_scale.RandomValueInRange, m_speed.RandomValueInRange, m_pathOffset.RandomValueInRange);
        return instance;
    }

    /// <summary>
    /// 回收指定的Enemy实例。
    /// 确保Enemy是从当前工厂创建的，并销毁对应的GameObject。
    /// </summary>
    /// <param name="enemy">要回收的Enemy实例。</param>
    public void Reclaim(Enemy enemy)
    {
        Debug.Assert(enemy.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(enemy.gameObject);
    }

    #endregion

    #region 字段

    /// <summary>
    /// Enemy预设体引用，用于实例化新的Enemy对象。
    /// </summary>
    [SerializeField]
    private Enemy m_prefab = default;

    /// <summary>
    /// Enemy实例的尺寸缩放范围设置。
    /// </summary>
    [SerializeField]
    [FloatRangeSlider(0.5f, 2f)]
    private FloatRange m_scale = new FloatRange(1f);

    /// <summary>
    ///  Enemy实例的移动速度范围设置。
    /// </summary>
    [SerializeField]
    [FloatRangeSlider(-0.4f, 0.4f)]
    private FloatRange m_pathOffset = new FloatRange(0f);

    /// <summary>
    ///  Enemy实例的移动速度范围设置。
    /// </summary>
    [SerializeField]
    [FloatRangeSlider(0.2f, 5f)]
    private FloatRange m_speed = new FloatRange(1f);

    #endregion
}