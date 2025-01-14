using UnityEngine;

/// <summary>
/// 敌人对象工厂类，专为游戏中的敌人生成与资源回收服务。
/// 该类继承自GameObjectFactory，实现了敌人类型的配置、实例化及回收功能，
/// 确保游戏能够高效地管理不同类型的敌人实例。
/// </summary>
[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory
{
    #region 嵌套类

    /// <summary>
    /// 敌人配置类，用于存储单个类型敌人的预制体及其属性范围。
    /// </summary>
    [System.Serializable]
    private class EnemyConfig
    {
        /// <summary>
        ///  Enemy预设体引用，用于实例化新的Enemy对象。
        /// </summary>
        public Enemy m_prefab = default;

        /// <summary>
        ///  Enemy实例的尺寸缩放范围设置。
        /// </summary>
        [FloatRangeSlider(0.5f, 2f)]
        public FloatRange m_scale = new FloatRange(1f);

        /// <summary>
        ///  Enemy实例的移动速度范围设置。
        /// </summary>
        [FloatRangeSlider(0.2f, 5f)]
        public FloatRange m_speed = new FloatRange(1f);

        /// <summary>
        ///  Enemy实例的移动偏移范围设置。
        /// </summary>
        [FloatRangeSlider(-0.4f, 0.4f)]
        public FloatRange m_pathOffset = new FloatRange(0f);

        /// <summary>
        ///  Enemy实例的生命值范围设置。
        /// </summary>
        [FloatRangeSlider(10f, 1000f)]
        public FloatRange m_health = new FloatRange(100f);
    }

    #endregion

    #region 方法

    /// <summary>
    /// 创建并初始化一个新的Enemy实例，并将其与当前工厂关联。
    /// </summary>
    /// <returns>一个新创建并初始化的Enemy实例。</returns>
    public Enemy Get(EnemyType type = EnemyType.Medium)
    {
        EnemyConfig config = GetConfig(type);
        // 1. 创建Enemy实例
        Enemy instance = CreateGameObjectInstance(config.m_prefab);
        instance.OriginFactory = this;

        // 2. 初始化Enemy实例
        instance.Initialize(
            config.m_scale.RandomValueInRange,
            config.m_speed.RandomValueInRange,
            config.m_pathOffset.RandomValueInRange,
            config.m_health.RandomValueInRange);
        return instance;
    }

    private EnemyConfig GetConfig(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Small: return m_small;
            case EnemyType.Medium: return m_medium;
            case EnemyType.Large: return m_large;
        }

        Debug.Assert(false, "Unsupported enemy type!");
        return null;
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
    /// 小型敌人的配置信息
    /// </summary>
    [SerializeField]
    private EnemyConfig m_small = default;

    /// <summary>
    /// 中型敌人配置引用
    /// </summary>
    [SerializeField]
    private EnemyConfig m_medium = default;

    /// <summary>
    /// 大型敌人配置引用 
    /// </summary>
    [SerializeField]
    private EnemyConfig m_large = default;

    #endregion
}