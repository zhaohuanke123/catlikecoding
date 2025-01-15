using System.Collections.Generic;
using UnityEngine;

// 游戏棋盘，用于可视化 
public class GameBoard : MonoBehaviour
{
    #region 方法

    public void GameUpdate()
    {
        for (int i = 0; i < m_updatingContent.Count; i++)
        {
            m_updatingContent[i].GameUpdate();
        }
    }

    /// <summary>
    /// 初始化游戏棋盘，根据给定的尺寸创建并布局游戏方块。
    /// </summary>
    /// <param name="size">棋盘的尺寸，一个包含宽度（x轴）和高度（y轴）的二维整数向量。</param>
    /// <param name="contentFactory"> 游戏方块内容工厂，用于创建和管理不同类型的GameTileContent对象。 </param>
    public void Initialize(Vector2Int size, GameTileContentFactory contentFactory)
    {
        // 1. 保存棋盘尺寸 设置地面的缩放
        m_size = size;
        m_contentFactory = contentFactory;
        m_ground.localScale = new Vector3(size.x, size.y, 1f);

        // 2. 计算距离中心点的偏移量
        var offset = new Vector2((size.x - 1) * 0.5f, (size.y - 1) * 0.5f);
        m_tiles = new GameTile[size.x * size.y];

        // 3. 循环创建游戏方块 
        for (int i = 0, y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++, i++)
            {
                // 1. 创建游戏方块
                GameTile tile = m_tiles[i] = Instantiate(m_tilePrefab, transform, false);

                // 2. 设置标志，用于交替遍历顺序
                tile.IsAlternative = ((x & 1) == 0);
                if ((y & 1) == 0)
                {
                    tile.IsAlternative = !tile.IsAlternative;
                }

                // 3. 设置位置
                tile.transform.localPosition = new Vector3(x - offset.x, 0f, y - offset.y);

                // 4. 设置相邻关系, 边界的方块不设置相邻关系
                if (x > 0)
                {
                    GameTile.MakeEastWestNeighbors(tile, m_tiles[i - 1]);
                }

                if (y > 0)
                {
                    GameTile.MakeNorthSouthNeighbors(tile, m_tiles[i - size.x]);
                }

                // 5. 设置初始内容 为空
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
            }
        }

        // 3. 设置终点
        ToggleDestination(m_tiles[m_tiles.Length / 2]);

        // 4. 设置怪物生成点
        ToggleSpawnPoint(m_tiles[0]);
    }

    /// <summary>
    /// 查找从起点到终点的所有路径。
    /// </summary>
    /// <returns>如果成功找到至少一条路径返回true，否则返回false。</returns>
    private bool FindPaths()
    {
        // 1. 遍历游戏棋盘上的每个方块，找到所有终点
        foreach (GameTile tile in m_tiles)
        {
            if (tile.Content.Type == GameTileContentType.Destination)
            {
                tile.BecomeDestination();
                m_searchFrontier.Enqueue(tile);
            }
            else
            {
                tile.ClearPath();
            }
        }

        // 2. 如果没有终点，返回false
        if (m_searchFrontier.Count == 0)
        {
            Debug.Log("Not Path");
            return false;
        }

        // 3. 通过广度优先搜索算法查找路径
        while (m_searchFrontier.Count > 0)
        {
            GameTile tile = m_searchFrontier.Dequeue();
            if (tile == null)
            {
                continue;
            }

            // 4. 尝试向四周扩展路径
            if (tile.IsAlternative)
            {
                m_searchFrontier.Enqueue(tile.GrowPathNorth());
                m_searchFrontier.Enqueue(tile.GrowPathSouth());
                m_searchFrontier.Enqueue(tile.GrowPathEast());
                m_searchFrontier.Enqueue(tile.GrowPathWest());
            }
            else
            {
                m_searchFrontier.Enqueue(tile.GrowPathWest());
                m_searchFrontier.Enqueue(tile.GrowPathEast());
                m_searchFrontier.Enqueue(tile.GrowPathSouth());
                m_searchFrontier.Enqueue(tile.GrowPathNorth());
            }
        }

        // 5. 检查是否所有方块都有路径, 找不到返回false
        foreach (GameTile tile in m_tiles)
        {
            if (!tile.HasPath)
            {
                return false;
            }
        }

        // 6. 显示路径
        if (m_showPaths)
        {
            foreach (GameTile tile in m_tiles)
            {
                tile.ShowPath();
            }
        }

        // 7. 返回true
        return true;
    }

    /// <summary>
    /// 根据射线获取游戏棋盘上的方块。
    /// </summary>
    /// <param name="ray">用于检测的游戏世界中的射线。</param>
    /// <returns>被射线命中的游戏方块，如果没有命中则返回null。</returns>
    public GameTile GetTile(Ray ray)
    {
        //  1. 射线检测
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, 1))
        {
            // 2. 计算射线命中的方块索引
            int x = (int)(hit.point.x + m_size.x * 0.5f);
            int y = (int)(hit.point.z + m_size.y * 0.5f);
            if (x >= 0 && x < m_size.x && y >= 0 && y < m_size.y)
            {
                return m_tiles[x + y * m_size.x];
            }

            return m_tiles[x + y * m_size.x];
        }

        return null;
    }

    /// <summary>
    /// 切换游戏方块的Destination状态。
    /// 如果方块当前是Destination，则将其恢复为空白方块；如果方块是空白方块，则将其设为Destination。
    /// 并在状态改变后尝试重新查找路径。
    /// </summary>
    /// <param name="tile">要切换Destination状态的游戏方块。</param>
    public void ToggleDestination(GameTile tile)
    {
        // 1. 切换Destination状态
        if (tile.Content.Type == GameTileContentType.Destination)
        {
            // 2. 如果是Destination，设置为空
            tile.Content = m_contentFactory.Get(GameTileContentType.Empty);
            // 3。 确保有一条路径
            if (!FindPaths())
            {
                tile.Content = m_contentFactory.Get(GameTileContentType.Destination);
                FindPaths();
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            // 3. 如果是Empty，设置为Destination
            tile.Content = m_contentFactory.Get(GameTileContentType.Destination);
            FindPaths();
        }
    }

    /// <summary>
    /// 切换当前游戏方块的内容为墙或空地，并在必要时重新查找路径。
    /// </summary>
    /// <param name="tile">要更改内容的游戏方块实例。</param>
    public void ToggleWall(GameTile tile)
    {
        // 1. 切换Wall 状态
        if (tile.Content.Type == GameTileContentType.Wall)
        {
            // 2. 如果是Wall，设置为空
            tile.Content = m_contentFactory.Get(GameTileContentType.Empty);
            FindPaths();
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            // 3. 如果是Empty，设置为Wall
            tile.Content = m_contentFactory.Get(GameTileContentType.Wall);
            // 4. 确保有一条路径
            if (!FindPaths())
            {
                tile.Content = m_contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tile">要进行Tower状态切换的游戏方块实例。</param>
    /// <param name="towerType"></param>
    public void ToggleTower(GameTile tile, TowerType towerType)
    {
        // 1. 切换Tower 状态
        if (tile.Content.Type == GameTileContentType.Tower)
        {
            m_updatingContent.Remove(tile.Content);
            if (((Tower)tile.Content).TowerType == towerType)
            {
                tile.Content = m_contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
            else
            {
                tile.Content = m_contentFactory.Get(towerType);
                m_updatingContent.Add(tile.Content);
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            // 2. 如果是Empty，设置为Tower
            tile.Content = m_contentFactory.Get(towerType);

            // 3. 确保有一条路径
            if (FindPaths())
            {
                m_updatingContent.Add(tile.Content);
            }
            else
            {
                tile.Content = m_contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
        }
        // 4. 如果是Wall，直接替换为Tower，不需要重新计算路径
        else if (tile.Content.Type == GameTileContentType.Wall)
        {
            tile.Content = m_contentFactory.Get(towerType);
            m_updatingContent.Add(tile.Content);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tile"></param>
    public void ToggleSpawnPoint(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.SpawnPoint)
        {
            if (m_spawnPoints.Count > 1)
            {
                m_spawnPoints.Remove(tile);
                tile.Content = m_contentFactory.Get(GameTileContentType.Empty);
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = m_contentFactory.Get(GameTileContentType.SpawnPoint);
            m_spawnPoints.Add(tile);
        }
    }

    #endregion

    public GameTile GetSpawnPoint(int index)
    {
        return m_spawnPoints[index];
    }

    #region 属性

    /// <summary>
    /// 是否显示路径。当设置为 true 时，会显示所有游戏方块的路径；设置为 false 时，则隐藏这些路径。
    /// 此属性修改时，会遍历所有游戏方块并调用相应的 ShowPath 或 HidePath 方法来更新显示状态。
    /// </summary>
    public bool ShowPaths
    {
        get => m_showPaths;
        set
        {
            // 1. 设置是否显示路径
            m_showPaths = value;
            if (m_showPaths)
            {
                // 2. 显示路径
                foreach (GameTile tile in m_tiles)
                {
                    tile.ShowPath();
                }
            }
            else
            {
                // 3. 隐藏路径
                foreach (GameTile tile in m_tiles)
                {
                    tile.HidePath();
                }
            }
        }
    }

    /// <summary>
    /// 是否显示网格。此属性控制游戏棋盘网格的显示与隐藏。
    /// 当设置为 true 时，会在游戏棋盘上显示网格纹理；设置为 false 时，则移除网格纹理。
    /// 修改此属性将直接影响棋盘的视觉效果，适用于用户界面切换需求或调试场景。
    /// </summary>
    public bool ShowGrid
    {
        get => m_showGrid;
        set
        {
            // 1. 设置是否显示网格
            m_showGrid = value;
            Material m = m_ground.GetComponent<MeshRenderer>().material;

            // 2. 显示网格
            if (m_showGrid)
            {
                m.mainTexture = m_gridTexture;
                m.SetTextureScale(MainTexID, m_size);
            }
            else
            {
                m.mainTexture = null;
            }
        }
    }

    /// <summary>
    /// 怪物生成点的数量。
    /// </summary>
    public int SpawnPointCount => m_spawnPoints.Count;

    #endregion

    #region 字段

    /// <summary>
    /// 地面的变换组件，用于控制游戏棋盘的整体缩放和平移。
    /// 在初始化时根据棋盘大小调整地面的局部缩放，确保与棋盘尺寸相匹配。
    /// </summary>
    [SerializeField]
    private Transform m_ground = default;

    /// <summary>
    /// 棋盘的尺寸，用二维整数表示，包含宽度（x轴）和高度（y轴）。
    /// 在初始化时设置，用于决定棋盘的大小以及游戏方块的布局。
    /// </summary>
    private Vector2Int m_size;

    /// <summary>
    /// 预制体引用，用于实例化游戏棋盘上的每个格子。
    /// 这个预制体应当包含`GameTile`组件，以便于管理棋盘上格子的逻辑与相邻关系。
    /// </summary>
    [SerializeField]
    private GameTile m_tilePrefab = default;

    /// <summary>
    /// 存储游戏棋盘上所有 GameTile 实例的数组，按初始化时的顺序排列。
    /// 每个元素代表棋盘上的一个格子，包含其状态、位置信息以及与相邻格子的关系。
    /// </summary>
    private GameTile[] m_tiles;

    /// <summary>
    /// 存储待搜索的游戏格子的队列。
    /// 在寻找路径的过程中，此队列用于按顺序处理各个GameTile对象，通过广度优先搜索算法探索可达路径。
    /// </summary>
    private Queue<GameTile> m_searchFrontier = new Queue<GameTile>();

    /// <summary>
    /// 游戏方块内容工厂，用于创建和管理不同类型的GameTileContent对象。
    /// 通过此工厂可以获取指定类型的游戏方块内容，如空地、终点或墙壁等。
    /// </summary>
    private GameTileContentFactory m_contentFactory;

    /// <summary>
    /// 是否显示游戏棋盘上各游戏方块的路径。
    /// </summary>
    private bool m_showPaths;

    /// <summary>
    /// 是否显示网格。当设置为 true 时，会在游戏棋盘上显示网格纹理；设置为 false 时，则隐藏网格。
    /// </summary>
    private bool m_showGrid;

    /// <summary>
    /// 棋盘网格的纹理，用于在视觉上显示游戏棋盘的网格结构。
    /// 当 <see cref="ShowGrid"/> 属性设置为 true 时，此纹理将应用于棋盘的地面材质上，
    /// 以实现网格的可视化展示。其显示效果会根据棋盘的实际尺寸进行适配调整。
    /// </summary>
    [SerializeField]
    private Texture2D m_gridTexture = default;

    /// <summary>
    /// 主纹理的Shader属性ID。用于在运行时通过脚本访问材质的主纹理属性。
    /// </summary>
    private static readonly int MainTexID = Shader.PropertyToID("_MainTex");

    /// <summary>
    /// 存储游戏中的怪物生成点列表。
    /// </summary>
    private List<GameTile> m_spawnPoints = new List<GameTile>();

    /// <summary>
    ///  存储需要更新的GameTileContent
    /// </summary>
    private List<GameTileContent> m_updatingContent = new List<GameTileContent>();

    #endregion
}