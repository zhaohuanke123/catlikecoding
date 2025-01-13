﻿using UnityEngine;

public class Game : MonoBehaviour
{
    #region Unity 生命周期

    private void Update()
    {
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

        m_spawnProgress += m_spawnSpeed * Time.deltaTime;
        while (m_spawnProgress >= 1f)
        {
            m_spawnProgress -= 1f;
            SpawnEnemy();
        }

        m_enemies.GameUpdate();
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
        // 1. 初始化棋盘
        m_board.Initialize(m_boardSize, m_tileContentFactory);
        m_board.ShowGrid = true;
    }

    #endregion

    #region 方法

    /// <summary>
    /// 处理屏幕触碰(鼠标左键)事件，当玩家点击屏幕时调用此方法。
    /// 根据触碰位置找到对应的 GameTile，并切换该方块上的状态为wall。
    /// </summary>
    private void HandleTouch()
    {
        GameTile tile = m_board.GetTile(TouchRay);
        if (tile != null)
        {
            m_board.ToggleWall(tile);
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

    private void SpawnEnemy()
    {
        GameTile spawnPoint = m_board.GetSpawnPoint(Random.Range(0, m_board.SpawnPointCount));
        Enemy enemy = m_enemyFactory.Get();
        enemy.SpawnOn(spawnPoint);

        m_enemies.Add(enemy);
    }

    #endregion

    #region 属性

    /// <summary>
    /// 屏幕触碰（或鼠标点击）产生的射线，用于确定玩家在游戏界面上的触碰位置，
    /// 并进一步与游戏中的棋盘方块交互。此属性通过将屏幕坐标转换为世界坐标系下的射线实现。
    /// </summary>
    private Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

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
    /// 
    /// </summary>
    [SerializeField]
    private EnemyFactory m_enemyFactory = default;

    /// <summary>
    /// 
    /// </summary>
    [SerializeField]
    [Range(0.1f, 10f)]
    private float m_spawnSpeed = 1f;

    /// <summary>
    /// 
    /// </summary>
    private float m_spawnProgress;

    /// <summary>
    /// 
    /// </summary>
    private EnemyCollection m_enemies = new EnemyCollection();

    #endregion
}