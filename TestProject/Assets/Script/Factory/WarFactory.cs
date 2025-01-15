using UnityEngine;

/// <summary>
/// War工厂类，继承自GameObjectFactory，专门用于创建和管理War相关的游戏实体对象。
/// </summary>
[CreateAssetMenu]
public class WarFactory : GameObjectFactory
{
    #region 方法

    /// <summary>
    /// 获取由工厂创建的指定类型的War实体实例。
    /// </summary>
    /// <typeparam name="T">要创建的War实体类型，必须继承自<see cref="WarEntity"/>。</typeparam>
    /// <param name="prefab">用于实例化的War实体预制体。</param>
    /// <returns>根据预制体创建的War实体实例。</returns>
    private T Get<T>(T prefab) where T : WarEntity
    {
        T instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        return instance;
    }

    /// <summary>
    /// 回收指定的War实体。
    /// </summary>
    /// <param name="entity">要被回收的War实体实例。</param>
    public void Reclaim(WarEntity entity)
    {
        Debug.Assert(entity.OriginFactory == this, "Wrong factory reclaimed!");

        Destroy(entity.gameObject);
    }

    #endregion

    #region 属性

    /// <summary>
    /// 获取炮弹实例的预制体实例。
    /// </summary>
    public Shell Shell => Get(m_shellPrefab);

    public Explosion Explosion => Get(m_explosionPrefab);

    #endregion

    #region 字段

    /// <summary>
    /// 存储用于生成炮弹实例的预制体。
    /// </summary>
    [SerializeField]
    private Shell m_shellPrefab = default;

    /// <summary>
    /// 爆炸特效的预制体实例。
    /// </summary>
    [SerializeField]
    private Explosion m_explosionPrefab = default;

    #endregion
}