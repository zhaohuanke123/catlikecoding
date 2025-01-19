using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Random = UnityEngine.Random;

/// <summary>
/// 游戏核心控制类，负责管理游戏的整体运行流程，包括初始化、场景管理、游戏状态控制等。
/// </summary>
public class Game : MonoBehaviour
{
    #region Unity 生命周期

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            m_isGamePlaying = false;
            m_enemies.Clear();
            m_nonEnemies.Clear();
            m_board.Clear();
            m_playableDirector.Play();
        }

        if (!m_isGamePlaying)
        {
            return;
        }

        // 1. 处理玩家左键
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch();
        }
        // 2. 处理玩家右键
        else if (Input.GetMouseButtonDown(1))
        {
            HandleAlternativeTouch();
        }

        // 3. 按键切换显示路径
        if (Input.GetKeyDown(KeyCode.V))
        {
            m_board.ShowPaths = !m_board.ShowPaths;
        }

        // 4. 按键切换显示网格
        if (Input.GetKeyDown(KeyCode.G))
        {
            m_board.ShowGrid = !m_board.ShowGrid;
        }

        // 5. 按键切换显示生成Tower的类型
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            m_selectedTowerType = TowerType.Laser;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            m_selectedTowerType = TowerType.Mortar;
        }

        // 6. 按键切换暂停
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = Time.timeScale > PausedTimeScale ? PausedTimeScale : m_playSpeed;
        }
        else if (Time.timeScale > PausedTimeScale)
        {
            Time.timeScale = m_playSpeed;
        }

        // 7. 开始新游戏
        if (Input.GetKeyDown(KeyCode.B))
        {
            BeginNewGame();
        }

        // 8. 处理游戏胜利和失败
        if (m_playerHealth <= 0 && m_startingPlayerHealth > 0)
        {
            OnGameEnd(false);
        }

        if (!m_activeScenario.Progress() && m_enemies.IsEmpty)
        {
            OnGameEnd(true);
        }

        // 9. 驱动enemy和棋盘的Update, 以及非敌人实体的Update
        m_enemies.GameUpdate();

        // 可以瞄准棋盘中心的塔能够获取本应超出射程的目标。它们无法跟踪这些目标，因此每个目标只会被锁定一个帧。
        // 所有enemy都在世界原点实例化，这与棋盘的中心重合。然后我们将它们移动到它们的生成点，但物理引擎并没有立即意识到这一点。 
        // 可以通过将 Physics.autoSyncTransforms 设置为 true 来强制在对象的变换发生变化时立即同步。
        // 但默认情况下它是关闭的，因为仅在需要时一次性同步所有内容效率更高。
        // 在我们的情况下，我们只需要在更新塔时进行同步。我们可以通过在 Game.Update 中更新enemy和棋盘之间调用 Physics.SyncTransforms 来实现这一点。
        Physics.SyncTransforms();

        m_board.GameUpdate();
        m_nonEnemies.GameUpdate();
    }

    private void OnEnable()
    {
        s_instance = this;
    }

    /// <summary>
    /// 强制规定最小尺寸为 2×2
    /// 如果存在OnValidate方法，Unity 编辑器会在组件可能发生更改后调用它。这包括将它们添加到游戏对象时、场景加载后、重新编译后、通过检查器编辑后、撤消/重做后以及组件重置后。
    /// </summary>
    private void OnValidate()
    {
        // 1. 强制规定最小尺寸为 2×2
        if (m_boardSize.x < 2)
        {
            m_boardSize.x = 2;
        }

        if (m_boardSize.y < 2)
        {
            m_boardSize.y = 2;
        }
    }

    private void Awake()
    {
        // 1. 初始化游戏
        m_playerHealth = m_startingPlayerHealth;
        m_board.Initialize(m_boardSize, m_tileContentFactory);
        m_board.ShowGrid = true;
        // m_activeScenario = m_scenario.Begin();
    }

    #endregion

    #region 方法

    public static void StartGame()
    {
        s_instance.m_isGamePlaying = true;
        s_instance.BeginNewGame();
    }

    /// <summary>
    ///  清除所有，然后开始一个新场景。
    /// </summary>
    private void BeginNewGame()
    {
        m_playerHealth = m_startingPlayerHealth;
        OnPlayerHealthChanged(m_playerHealth);
        m_enemies.Clear();
        m_nonEnemies.Clear();
        m_board.Clear();
        m_activeScenario = m_scenario.Begin();
    }

    /// <summary>
    /// 处理屏幕触碰(鼠标左键)事件，当玩家点击屏幕时调用此方法。
    /// </summary>
    private void HandleTouch()
    {
        // 1. 获取触碰位置对应的格子
        GameTile tile = m_board.GetTile(TouchRay);
        if (tile != null)
        {
            // 2. 切换塔或者墙
            if (Input.GetKey(KeyCode.LeftShift))
            {
                m_board.ToggleTower(tile, m_selectedTowerType);
            }
            else
            {
                m_board.ToggleWall(tile);
            }
        }
    }

    /// <summary>
    /// 处理替代触摸输入事件，主要关注鼠标右键操作。
    /// </summary>
    private void HandleAlternativeTouch()
    {
        // 1. 获取触碰位置对应的格子
        GameTile tile = m_board.GetTile(TouchRay);
        if (tile != null)
        {
            // 2. 切换生成点或者目标点
            if (Input.GetKey(KeyCode.LeftShift))
            {
                m_board.ToggleDestination(tile);
            }
            else
            {
                m_board.ToggleSpawnPoint(tile);
            }
        }
    }

    /// <summary>
    /// 根据当前游戏状态和配置生成Enemy
    /// </summary>
    public static void SpawnEnemy(EnemyFactory factory, EnemyType type)
    {
        GameTile spawnPoint = s_instance.m_board.GetSpawnPoint(Random.Range(0, s_instance.m_board.SpawnPointCount));
        Enemy enemy = factory.Get(type);
        enemy.SpawnOn(spawnPoint);
        s_instance.m_enemies.Add(enemy);
    }

    /// <summary>
    /// 创建并初始化一个新的炮弹实体
    /// </summary>
    /// <returns>新创建的炮弹实体。</returns>
    public static Shell SpawnShell()
    {
        Shell shell = s_instance.m_warFactory.Shell;
        s_instance.m_nonEnemies.Add(shell);
        return shell;
    }

    /// <summary>
    /// 创建并初始化一个爆炸效果实例
    /// 此方法用于模拟炮弹爆炸、产生视觉特效及处理相关逻辑。
    /// </summary>
    /// <returns>新创建的爆炸实例。</returns>
    public static Explosion SpawnExplosion()
    {
        Explosion explosion = s_instance.m_warFactory.Explosion;
        s_instance.m_nonEnemies.Add(explosion);
        return explosion;
    }

    /// <summary>
    /// 敌人到达终点, 玩家生命值减1
    /// </summary>
    public static void EnemyReachedDestination()
    {
        s_instance.m_playerHealth -= 1;
        OnPlayerHealthChanged(s_instance.m_playerHealth);
    }

    /// <summary>
    /// 敌人数量变化时触发的事件处理方法。
    /// 此方法用于通知所有订阅者敌人数量的变更情况，通常在添加或移除敌人时被调用。
    /// </summary>
    /// <param name="count">当前敌人的数量。</param>
    public static void OnEnemyCountChanged(int count)
    {
        s_instance.EventOnEnemyCountChanged?.Invoke(count);
    }

    /// <summary>
    /// 游戏周期变化时触发的事件处理方法。
    /// 此方法用于通知所有订阅者当前游戏周期的变化，通常在一个完整的游戏循环结束时被调用。
    /// </summary>
    /// <param name="cycle">当前游戏周期的序号。</param>
    /// <param name="allCycle">总的循环</param>
    public static void OnCycleChanged(int cycle, int allCycle)
    {
        s_instance.EventOnCycleChanged?.Invoke(cycle, allCycle);
    }

    /// <summary>
    /// 波次变化时触发的事件处理方法。
    /// 此方法用于通知所有订阅者当前波次的变化，通常在新的一波敌人开始时被调用。
    /// </summary>
    /// <param name="wave">当前波次的编号。</param>
    /// <param name="allWave">总波次</param>
    public static void OnWaveChanged(int wave, int allWave)
    {
        s_instance.EventOnWaveChanged?.Invoke(wave, allWave);
    }

    /// <summary>
    /// 玩家生命值变化时触发的事件处理方法。
    /// 此方法用于通知所有订阅者玩家生命值得变更情况，通常在敌人到达终点或其它影响玩家生命值得操作后被调用。
    /// </summary>
    /// <param name="health">当前玩家的剩余生命值。</param>
    public static void OnPlayerHealthChanged(int health)
    {
        s_instance.EventOnPlayerHealthChanged?.Invoke(health, s_instance.m_startingPlayerHealth);
    }

    /// <summary>
    /// 游戏结束事件处理方法。
    /// 根据游戏结果（胜利或失败）触发相应的逻辑，例如显示游戏结果界面、统计得分、重置游戏状态等。
    /// </summary>
    /// <param name="isWin">是否为胜利。true 表示胜利，false 表示失败。</param>
    public static void OnGameEnd(bool isWin)
    {
        s_instance.m_isGamePlaying = false;
        s_instance.EventOnGameEnd?.Invoke(isWin);
    }

    #endregion

    #region 事件

    /// <summary>
    /// 敌人数量变化事件。当游戏中的敌人数量发生变化时，此事件会被触发，并传递当前的敌人数量作为参数。
    /// </summary>
    public event UnityAction<int> EventOnEnemyCountChanged;

    /// <summary>
    /// 游戏周期改变事件，当游戏进行到新的一轮时触发。
    /// </summary>
    public event UnityAction<int, int> EventOnCycleChanged;

    /// <summary>
    /// 游戏波次变化事件，当游戏中敌人的波次发生变化时触发。
    /// </summary>
    public event UnityAction<int, int> EventOnWaveChanged;

    /// <summary>
    /// 玩家生命值变化事件。当玩家的生命值发生变化时，此事件被触发。
    /// </summary>
    public event UnityAction<int, int> EventOnPlayerHealthChanged;

    /// <summary>
    /// 游戏结束事件。当游戏结束时，此事件被触发。
    /// </summary>
    public event UnityAction<bool> EventOnGameEnd;

    #endregion

    #region 属性

    /// <summary>
    /// 屏幕触碰（或鼠标点击）产生的射线，用于确定玩家在游戏界面上的触碰位置，
    /// 并进一步与游戏中的棋盘方块交互。此属性通过将屏幕坐标转换为世界坐标系下的射线实现。
    /// </summary>
    private Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    /// <summary>
    /// 获取全局唯一的Game实例。
    /// </summary>
    public static Game Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = FindObjectOfType<Game>();
            }

            return s_instance;
        }
    }

    #endregion

    #region 字段

    /// <summary>
    /// 棋盘的尺寸，表示为列数（x）和行数（y）的二维整数向量。
    /// </summary>
    [SerializeField]
    private Vector2Int m_boardSize = new Vector2Int(11, 11);

    /// <summary>
    /// 游戏棋盘实例，负责存储和管理游戏中的所有棋盘格子以及它们之间的关系。
    /// </summary>
    [SerializeField]
    private GameBoard m_board = default;

    /// <summary>
    /// 游戏格子内容工厂实例，用于生成和回收不同类型的GameTileContent对象。
    /// </summary>
    [SerializeField]
    private GameTileContentFactory m_tileContentFactory = default;

    /// <summary>
    /// 存储并管理游戏中所有Enemy的集合。
    /// </summary>
    private EnemyCollection m_enemies = new EnemyCollection();

    /// <summary>
    ///  非敌人的GameBehavior集合，子弹等
    /// </summary>
    private GameBehaviorCollection m_nonEnemies = new GameBehaviorCollection();

    /// <summary>
    ///  当前选择的Tower类型。
    /// </summary>
    private TowerType m_selectedTowerType;

    /// <summary>
    /// 塔防游戏entity工厂实例，用于生成和回收游戏中的实体对象。
    /// </summary>
    [SerializeField]
    private WarFactory m_warFactory = default;

    /// <summary>
    ///  全局唯一的Game实例。
    /// </summary>
    private static Game s_instance;

    /// <summary>
    /// 当前游戏场景的配置实例
    /// </summary>
    [SerializeField]
    private GameScenario m_scenario = default;

    /// <summary>
    ///  当前游戏场景的状态 
    /// </summary>
    private GameScenario.State m_activeScenario;

    /// <summary>
    ///  玩家初始生命值
    /// </summary>
    [SerializeField]
    [Range(0, 100)]
    private int m_startingPlayerHealth = 10;

    /// <summary>
    ///  玩家当前生命值
    /// </summary>
    private int m_playerHealth;

    /// <summary>
    /// 控制游戏暂停的时间缩放值。
    /// </summary>
    private const float PausedTimeScale = 0f;

    /// <summary>
    /// 游戏播放速度
    /// </summary>
    [SerializeField]
    [Range(1f, 10f)]
    private float m_playSpeed = 1f;

    /// <summary>
    /// 游戏是否正在进行中
    /// </summary>
    private bool m_isGamePlaying;

    /// <summary>
    /// TimeLine播放器
    /// </summary>
    [SerializeField]
    private PlayableDirector m_playableDirector;

    #endregion
}