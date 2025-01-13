using UnityEngine;


/// <summary>
/// 表示游戏中的每个方块
/// </summary>
public class GameTile : MonoBehaviour
{
    #region 方法

    /// <summary>
    /// 为两个相邻的GameTile对象建立东西方向的相邻关系。
    /// </summary>
    /// <param name="east">东方的GameTile对象。</param>
    /// <param name="west">西方的GameTile对象。</param>
    public static void MakeEastWestNeighbors(GameTile east, GameTile west)
    {
        Debug.Assert(west.m_east == null && east.m_west == null, "Redefined neighbors!");

        // 1. 设置西方向的相邻关系
        west.m_east = east;
        east.m_west = west;
    }

    /// <summary>
    /// 为两个游戏方块建立南北相邻关系。
    /// </summary>
    /// <param name="north">北方的游戏方块。</param>
    /// <param name="south">南方的游戏方块。</param>
    /// <remarks>
    /// 此方法会检查南北两个方块是否已定义了相邻关系，以避免重复定义。如果任何一方已经有关联的相邻方块，
    /// 则会抛出断言错误。
    /// </remarks>
    public static void MakeNorthSouthNeighbors(GameTile north, GameTile south)
    {
        Debug.Assert(south.m_north == null && north.m_south == null, "Redefined neighbors!");

        // 1. 设置南北方向的相邻关系
        south.m_north = north;
        north.m_south = south;
    }

    /// <summary>
    /// 清除当前方块在路径查找过程中的信息，重置距离为最大值且下一个路径方块为null。
    /// </summary>
    public void ClearPath()
    {
        // 1. 重置距离为最大值
        m_distance = int.MaxValue;
        // 2. 下一个路径方块为null
        m_nextOnPath = null;
    }

    /// <summary>
    /// 将当前GameTile标记为目标目的地，并清除其路径信息。
    /// </summary>
    public void BecomeDestination()
    {
        // 1. 清除路径信息
        m_distance = 0;
        m_nextOnPath = null;
        ExitPoint = transform.localPosition;
    }

    /// <summary>
    /// 从当前GameTile出发，沿着特定方向尝试扩展路径至相邻的GameTile。
    /// 如果目标相邻方块为空或者已有路径，则返回null。否则，更新目标方块的路径信息并返回该方块。
    /// </summary>
    /// <param name="neighbor">尝试延伸路径到达的相邻GameTile对象。</param>
    /// <param name="direction">需要获取地图块的方向</param>
    /// <returns>成功延伸路径后的相邻GameTile，如果无法延伸则返回null。</returns>
    private GameTile GrowPathTo(GameTile neighbor, Direction direction)
    {
        Debug.Assert(HasPath, "No path!");

        // 1. 如果相邻方块为空或已有路径，则返回null
        if (neighbor == null || neighbor.HasPath)
        {
            return null;
        }

        // 2. 更新相邻方块的路径信息
        neighbor.m_distance = m_distance + 1;
        neighbor.m_nextOnPath = this;

        // 3. 设置出口点
        neighbor.ExitPoint = neighbor.transform.localPosition + direction.GetHalfVector();

        // 4. 设置路径方向
        neighbor.PathDirection = direction;

        return neighbor.Content.BlocksPath ? null : neighbor;
    }

    /// <summary>
    /// 向北扩展当前路径。
    /// </summary>
    /// <returns>返回向北相邻的路径上的GameTile对象，如果无法向北扩展则返回null。</returns>
    public GameTile GrowPathNorth() => GrowPathTo(m_north, Direction.South);

    /// <summary>
    /// 从当前GameTile向东尝试扩展路径至相邻的GameTile。
    /// 如果东方的相邻方块为空或者已有路径，则不执行任何操作并返回null。否则，更新该相邻方块的路径信息，并返回该方块。
    /// </summary>
    /// <returns>成功扩展路径后的东方GameTile对象，或null（如果无法扩展）。</returns>
    public GameTile GrowPathEast() => GrowPathTo(m_east, Direction.West);

    /// <summary>
    /// 尝试从当前GameTile向南扩展路径至相邻的GameTile。
    /// </summary>
    /// <returns>如果成功延伸路径到南方的相邻GameTile，则返回该方块；否则返回null。</returns>
    public GameTile GrowPathSouth() => GrowPathTo(m_south, Direction.North);

    /// <summary>
    /// 尝试从当前GameTile向西扩展路径至相邻的GameTile。
    /// 如果西侧相邻方块为空或已有路径，则返回null。否则，更新该方块的路径信息并返回该方块。
    /// </summary>
    /// <returns>成功延伸路径后的西侧相邻GameTile，如果无法延伸则返回null。</returns>
    public GameTile GrowPathWest() => GrowPathTo(m_west, Direction.East);

    /// <summary>
    /// 显示当前GameTile对象在路径上的箭头指示。
    /// 如果该方块是路径的一部分，此方法将根据下一个路径方向调整箭头的方向并使其可见。
    /// 如果该方块不在路径上或距离为0，则箭头将被隐藏。
    /// </summary>
    public void ShowPath()
    {
        // 1. 如果不在路径上(即该块为终点)，隐藏箭头
        if (m_distance == 0)
        {
            m_arrow.gameObject.SetActive(false);
            return;
        }

        // 2. 显示箭头
        m_arrow.gameObject.SetActive(true);

        // 3. 根据下一个路径方向调整箭头的方向
        m_arrow.localRotation = m_nextOnPath == m_north ? s_northRotation :
            m_nextOnPath == m_east ? s_eastRotation :
            m_nextOnPath == m_south ? s_southRotation : s_westRotation;
    }

    /// <summary>
    /// 隐藏当前游戏方块在路径显示中的箭头指示。
    /// </summary>
    /// <remarks>
    /// 此方法用于在不需要显示路径时，隐藏游戏方块上的箭头图标，通常在路径寻找到达终点或需要重置显示时调用。
    /// </remarks>
    public void HidePath()
    {
        // 1. 隐藏箭头
        m_arrow.gameObject.SetActive(false);
    }

    #endregion

    #region 属性

    /// <summary>
    /// 指示该游戏方块是否位于当前搜索的路径上。
    /// </summary>
    /// <value><c>true</c> 如果该方块是路径的一部分，即其距离不是 <see cref="int.MaxValue"/>；否则为 <c>false</c>。</value>
    public bool HasPath => m_distance != int.MaxValue;

    /// <summary>
    /// 指示当前游戏方块使用的扩展顺序
    /// </summary>
    public bool IsAlternative { get; set; }

    /// <summary>
    /// 获取或设置当前游戏方块所展示的内容。
    /// </summary>
    /// <value>
    /// 类型为 <see cref="GameTileContent"/> 的对象，表示方块上的具体内容元素（例如草地、道路、目标等）。
    /// </value>
    /// <remarks>
    /// 当设置新的内容时，原有内容（如果存在）会被正确回收以保持资源的有效管理。
    /// </remarks>
    public GameTileContent Content
    {
        get => m_content;
        set
        {
            Debug.Assert(value != null, "Null assigned to content!");

            // 1. 如果当前内容不为空，回收
            if (m_content != null)
            {
                m_content.Recycle();
            }

            // 2. 设置新内容
            m_content = value;
            m_content.transform.localPosition = transform.localPosition;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public GameTile NextTileOnPath => m_nextOnPath;


    /// <summary>
    /// 
    /// </summary>
    public Vector3 ExitPoint { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public Direction PathDirection { get; private set; }

    #endregion

    #region 字段

    /// <summary>
    /// 存储当前游戏方块的内容组件。
    /// </summary>
    /// <remarks>
    /// 此字段用于持有代表方块上内容（如草地、道路或障碍物等）的`GameTileContent`实例。
    /// 它允许动态地更改方块表面的外观和行为，同时保持对原始内容工厂的引用以便于回收和重用。
    /// </remarks>
    private GameTileContent m_content;

    /// <summary>
    /// 箭头Transform组件。
    /// </summary>
    [SerializeField]
    private Transform m_arrow = default;

    /// <summary>
    /// 北方的GameTile对象，用于表示与当前方块北侧相邻的游戏方块。
    /// </summary>
    private GameTile m_north;

    /// <summary>
    /// 东方的GameTile对象，用于表示与当前方块东侧相邻的游戏方块。
    /// </summary>
    private GameTile m_east;

    /// <summary>
    /// 南方的GameTile对象，用于表示与当前方块南侧相邻的游戏方块。 
    /// </summary>
    private GameTile m_south;

    /// <summary>
    /// 西方的GameTile对象，用于表示与当前方块西侧相邻的游戏方块。
    /// </summary>
    private GameTile m_west;

    /// <summary>
    /// 指向路径上的下一个GameTile对象。此属性用于追踪当前位置到目标位置的路径中的下一个方块。
    /// 在路径寻找算法中，当一个方块被访问并扩展其相邻方块时，这个相邻方块将成为该方块的`nextOnPath`。
    /// </summary>
    private GameTile m_nextOnPath;

    /// <summary>
    /// 记录当前游戏方块到路径起点的距离。
    /// 初始值为 <see cref="int.MaxValue"/>，表示尚未计算距离或不在路径上。
    /// 当方块成为路径的一部分时，该值会被更新为从起点到当前方块的实际步数。
    /// </summary>
    private int m_distance;

    /// <summary>
    /// 代表朝向北方时的游戏方块旋转角度。 
    /// </summary>
    /// <value>一个Quaternion，表示绕Y轴旋转90度（即看向正北方向）的旋转状态。</value>
    private static Quaternion s_northRotation = Quaternion.Euler(90f, 0f, 0f);

    /// <summary>
    /// 代表朝向东方时的游戏方块旋转角度。 
    /// </summary>
    /// <value>一个静态的四元数，表示向东旋转90度（绕Y轴）。</value>
    private static Quaternion s_eastRotation = Quaternion.Euler(90f, 90f, 0f);

    /// <summary>
    ///  表示朝向南方时的游戏方块旋转角度。 
    /// </summary>
    private static Quaternion s_southRotation = Quaternion.Euler(90f, 180f, 0f);

    /// <summary>
    /// 表示朝向西方时的游戏方块旋转角度。
    /// </summary>
    private static Quaternion s_westRotation = Quaternion.Euler(90f, 270f, 0f);

    #endregion
}