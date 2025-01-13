using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu]
public class GameTileContentFactory : GameObjectFactory
{
    #region 方法

    /// <summary>
    /// 重新回收并处理游戏格子内容对象，通常涉及实例的销毁与资源释放。
    /// </summary>
    /// <param name="content">待回收的游戏格子内容对象。必须是本工厂创建的实例。</param>
    public void Reclaim(GameTileContent content)
    {
        Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(content.gameObject);
    }

    /// <summary>
    /// 从预设体创建并初始化一个新的 GameTileContent 实例。
    /// </summary>
    /// <param name="prefab">要实例化的 GameTileContent 预设体。</param>
    /// <returns>新创建的 GameTileContent 实例，已设置其 OriginFactory 为当前工厂，并移动到工厂场景中。</returns>
    private GameTileContent Get(GameTileContent prefab)
    {
        // 1. 实例化预设体
        GameTileContent instance = CreateGameObjectInstance(prefab);
        // 2. 设置来源工厂
        instance.OriginFactory = this;

        // 3. 移动到工厂场景
        return instance;
    }

    /// <summary>
    /// 根据给定的游戏格子类型获取对应的预置体实例。
    /// </summary>
    /// <param name="type">要获取的游戏格子内容类型。</param>
    /// <returns>与指定类型对应的游戏格子内容实例。如果类型不支持，则返回null并记录错误日志。</returns>
    public GameTileContent Get(GameTileContentType type)
    {
        switch (type)
        {
            case GameTileContentType.Destination: return Get(m_destinationPrefab);
            case GameTileContentType.Empty: return Get(m_emptyPrefab);
            case GameTileContentType.Wall: return Get(m_wallPrefab);
            case GameTileContentType.SpawnPoint: return Get(m_spawnPointPrefab);
        }

        Debug.Assert(false, "Unsupported type: " + type);
        return null;
    }

    /// <summary>
    /// 根据给定的炮塔类型获取对应的炮塔预置体实例。
    /// </summary>
    /// <param name="type">要获取的炮塔类型。</param>
    /// <returns>与指定类型对应的炮塔实例。</returns>
    public Tower Get(TowerType type)
    {
        Debug.Assert((int)type < m_towerPrefabs.Length, "Unsupported tower type!");

        Tower prefab = m_towerPrefabs[(int)type];

        Debug.Assert(type == prefab.TowerType, "Tower prefab at wrong index!");

        return Get(prefab);
    }

    /// <summary>
    /// 根据给定的游戏格子内容类型，获取一个新的游戏格子内容实例。
    /// 此方法通过工厂模式确保实例化过程符合预期，并处理类型与预制体之间的映射关系。
    /// </summary>
    ///  <typeparam name="T">所需游戏格子内容的类型。</typeparam>
    /// <param name="prefab"> 要获取的游戏格子内容预制体实例。</param>
    /// <returns>与指定类型对应的新创建的游戏格子内容实例。</returns>
    private T Get<T>(T prefab) where T : GameTileContent
    {
        T instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        return instance;
    }

    #endregion

    #region 字段

    /// <summary>
    /// 存储场景实例，用于管理由 GameTileContentFactory 创建的游戏对象。
    /// </summary>
    private Scene m_contentScene;

    /// <summary>
    /// 目标点预设体，用于生成游戏地图上的目标位置内容。
    /// </summary>
    [SerializeField]
    private GameTileContent m_destinationPrefab = default;

    /// <summary>
    /// 空白格子的预制体引用。
    /// </summary>
    [SerializeField]
    private GameTileContent m_emptyPrefab = default;

    /// <summary>
    /// 障碍物预制体，用于生成游戏地图上的墙类型格子内容。
    /// </summary>
    [SerializeField]
    private GameTileContent m_wallPrefab = default;

    /// <summary>
    /// 预制体实例，代表游戏中的Enemy出生点。
    /// </summary>
    [SerializeField]
    private GameTileContent m_spawnPointPrefab = default;

    /// <summary>
    /// 预制体实例，代表游戏中可构建的Tower。
    /// </summary>
    [SerializeField]
    private Tower[] m_towerPrefabs = default;

    #endregion
}