using UnityEngine;

/// <summary>
/// 游戏格子内容基类，用于表示游戏地图上每个格子可变化的内容，如地面、障碍物或目标点等。
/// 每个实例都与一个特定的 GameTileContentFactory 关联，以支持对象池和资源复用。
/// </summary>
[SelectionBase]
public class GameTileContent : MonoBehaviour
{
    #region 方法

    /// <summary>
    /// 在每一帧游戏更新时被调用，用于执行与游戏逻辑相关的更新操作。
    /// 子类可以重写此方法以实现具体的更新行为。
    /// </summary>
    public virtual void GameUpdate()
    {
    }

    /// <summary>
    /// 将当前 GameTileContent 实例返回给其关联的工厂以便复用或销毁。
    /// 当不再需要某个格子内容时，应调用此方法以确保资源得到妥善管理。
    /// </summary>
    public void Recycle()
    {
        m_originFactory.Reclaim(this);
    }

    #endregion

    #region 属性

    /// <summary>
    /// 格子内容的原始工厂。
    /// 此属性指定了该 GameTileContent 实例所归属的 GameTileContentFactory，用于资源管理和对象池功能。
    /// </summary>
    public GameTileContentFactory OriginFactory
    {
        get => m_originFactory;
        set
        {
            Debug.Assert(m_originFactory == null, "Redefined origin factory!");
            m_originFactory = value;
        }
    }

    /// <summary>
    ///  指示当前格子内容是否阻挡了路径。
    /// </summary>
    public bool BlocksPath => Type == GameTileContentType.Wall || Type == GameTileContentType.Tower;

    #endregion

    #region 字段

    /// <summary>
    /// 格子内容类型，表示游戏地图格子所承载的具体内容，例如空地或目标位置。
    /// </summary>
    [SerializeField]
    private GameTileContentType m_type = default;

    /// <summary>
    /// 获取当前游戏格子内容的类型。
    /// </summary>
    /// <value>
    /// 游戏格子内容的类型，源自 <see cref="GameTileContentType"/> 枚举，例如 Empty 或 Destination。
    /// </value>
    /// <remarks>
    /// 此属性提供了游戏地图上每个格子内容的类型信息，是了解和操作格子状态的基础。
    /// </remarks>
    public GameTileContentType Type => m_type;

    /// <summary>
    /// 游戏格子内容的原始工厂引用。
    /// 用于资源管理和对象池处理，确保内容能够被正确回收和复用。
    /// </summary>
    private GameTileContentFactory m_originFactory;

    #endregion
}