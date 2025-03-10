﻿using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
///  Enemy类，塔防游戏中的敌人实体类。 
/// </summary>
public class Enemy : GameBehavior
{
    private void Awake()
    {
        m_animator.Configure(m_model.GetChild(0).gameObject.AddComponent<Animator>(), m_animationConfig);
    }

    private void OnDestroy()
    {
        m_animator.Destroy();
    }

    #region 方法

    public override bool GameUpdate()
    {
#if UNITY_EDITOR
        if (!m_animator.IsValid)
        {
            m_animator.RestoreAfterHotReload(m_model.GetChild(0).GetComponent<Animator>(), m_animationConfig,
                m_animationConfig.MoveAnimationSpeed * m_speed / Scale);
        }
#endif
   
        m_animator.GameUpdate();
        // 1. 动画相关检查
        if (m_animator.CurrentClip == EnemyAnimator.Clip.Intro)
        {
            if (!m_animator.IsDone)
            {
                return true;
            }

            m_animator.PlayMove(m_animationConfig.MoveAnimationSpeed * m_speed / Scale);
            m_targetPointCollider.enabled = true;
        }
        // 退出或死亡动画
        else if (m_animator.CurrentClip >= EnemyAnimator.Clip.Outro)
        {
            if (m_animator.IsDone)
            {
                Recycle();
                return false;
            }

            return true;
        }

        // 2. 检查Enemy是否已经死亡
        if (Health <= 0f)
        {
            m_animator.PlayDying();
            m_targetPointCollider.enabled = false;
            return true;
        }

        // 3. 更新移动进度
        m_progress += Time.deltaTime * m_progressFactor;
        while (m_progress >= 1f)
        {
            if (m_tileTo == null)
            {
                Game.EnemyReachedDestination();
                m_animator.PlayOutro();
                m_targetPointCollider.enabled = false;
                return true;
            }

            m_progress = (m_progress - 1f) / m_progressFactor;
            PrepareNextState();
            m_progress *= m_progressFactor;
        }

        // 4. 更新Enemy位置
        if (m_directionChange == DirectionChange.None)
        {
            transform.localPosition = Vector3.LerpUnclamped(m_positionFrom, m_positionTo, m_progress);
        }
        // 5. 更新Enemy旋转
        else
        {
            float angle = Mathf.LerpUnclamped(m_directionAngleFrom, m_directionAngleTo, m_progress);
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);
        }

        return true;
    }

    public override void Recycle()
    {
        m_animator.Stop();
        m_originFactory.Reclaim(this);
    }

    /// <summary>
    /// 初始化Enemy对象的基本属性
    /// </summary>
    /// <param name="scale">Enemy模型的缩放因子，决定模型在场景中的大小。</param>
    /// <param name="speed">Enemy移动的速度。</param>
    /// <param name="pathOffset">Enemy沿路径移动时的偏移距离。</param>
    /// <param name="health">Enemy的生命值</param>
    public void Initialize(float scale, float speed, float pathOffset, float health)
    {
        Scale = scale;
        m_model.localScale = new Vector3(scale, scale, scale);
        m_speed = speed;
        m_pathOffset = pathOffset;
        Health = health;

        m_animator.PlayIntro();
        m_targetPointCollider.enabled = false;
    }

    /// <summary>
    /// 在指定的游戏方块上生成Enemy对象。
    /// </summary>
    /// <param name="tile">Enemy将要被初始化并开始移动的起始方块。</param>
    public void SpawnOn(GameTile tile)
    {
        // transform.localPosition = tile.transform.localPosition;
        Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go!", this);

        // 1. 获取下一个路径方块
        m_tileFrom = tile;
        m_tileTo = tile.NextTileOnPath;
        m_progress = 0f;

        // 2. 准备开始移动
        PrepareIntro();
    }

    /// <summary>
    /// 设置Enemy下一状态的数据。
    /// 此方法在Enemy到达路径上的一个新方块时被调用，用于更新Enemy的位置、方向以及相关动画状态。
    /// </summary>
    private void PrepareNextState()
    {
        m_tileFrom = m_tileTo;
        m_tileTo = m_tileTo.NextTileOnPath;
        m_positionFrom = m_positionTo;
        if (m_tileTo == null)
        {
            PrepareOutro();
            return;
        }

        // 1. 更新下一个移动状态
        m_positionTo = m_tileFrom.ExitPoint;
        m_directionChange = m_direction.GetDirectionChangeTo(m_tileFrom.PathDirection);
        m_direction = m_tileFrom.PathDirection;
        m_directionAngleFrom = m_directionAngleTo;

        // 2. 更新下一个旋转状态
        switch (m_directionChange)
        {
            case DirectionChange.None:
                PrepareForward();
                break;
            case DirectionChange.TurnRight:
                PrepareTurnRight();
                break;
            case DirectionChange.TurnLeft:
                PrepareTurnLeft();
                break;
            default:
                PrepareTurnAround();
                break;
        }
    }

    /// <summary>
    ///Enemy的前进方向状态。
    /// </summary>
    private void PrepareForward()
    {
        transform.localRotation = m_direction.GetRotation();
        m_directionAngleTo = m_direction.GetAngle();
        m_model.localPosition = new Vector3(m_pathOffset, 0f);
        m_progressFactor = m_speed;
    }

    /// <summary>
    /// 准备Enemy向右转的状态。
    /// </summary>
    private void PrepareTurnRight()
    {
        m_directionAngleTo = m_directionAngleFrom + 90f;
        m_model.localPosition = new Vector3(m_pathOffset - 0.5f, 0f);
        transform.localPosition = m_positionFrom + m_direction.GetHalfVector();
        m_progressFactor = m_speed / (Mathf.PI * 0.5f * (0.5f - m_pathOffset));
    }

    /// <summary>
    /// 准备向左转弯的状态。
    /// 计算向左转弯所需的朝向角度变更、模型位置调整以及平滑旋转的进度因子。
    /// </summary>
    private void PrepareTurnLeft()
    {
        m_directionAngleTo = m_directionAngleFrom - 90f;
        m_model.localPosition = new Vector3(m_pathOffset + 0.5f, 0f);
        transform.localPosition = m_positionFrom + m_direction.GetHalfVector();
        m_progressFactor = m_speed / (Mathf.PI * 0.5f * (0.5f + m_pathOffset));
    }

    /// <summary>
    /// 准备Enemy的转身动画状态，用于当方向改变需要完全掉头时调用。
    /// </summary>
    private void PrepareTurnAround()
    {
        m_directionAngleTo = m_directionAngleFrom + (m_pathOffset < 0f ? 180f : -180f);
        m_model.localPosition = new Vector3(m_pathOffset, 0f);
        transform.localPosition = m_positionFrom;
        m_progressFactor = m_speed / (Mathf.PI * Mathf.Max(Mathf.Abs(m_pathOffset), 0.2f));
    }

    /// <summary>
    /// 准备Enemy的初始进场状态。
    /// 在Enemy被放置到游戏场景中的特定方块后调用，
    /// </summary>
    private void PrepareIntro()
    {
        // 1. 设置初始位置和目标位置
        m_positionFrom = m_tileFrom.transform.localPosition;
        transform.localPosition = m_positionFrom;
        m_positionTo = m_tileFrom.ExitPoint;

        // 2. 设置初始方向和目标方向
        m_direction = m_tileFrom.PathDirection;
        m_directionChange = DirectionChange.None;
        m_directionAngleFrom = m_directionAngleTo = m_direction.GetAngle();

        // 3. 设置初始模型位置
        m_model.localPosition = new Vector3(m_pathOffset, 0f);

        // 4. 设置初始旋转
        transform.localRotation = m_direction.GetRotation();

        // 5. 设置进度因子
        m_progressFactor = 2f * m_speed;
    }

    /// <summary>
    /// 准备Enemy的退出动画或逻辑。
    /// 用于当Enemy到达路径终点或遇到特定条件时调用，
    /// </summary>
    private void PrepareOutro()
    {
        m_positionTo = m_tileFrom.transform.localPosition;
        m_directionChange = DirectionChange.None;
        m_directionAngleTo = m_direction.GetAngle();
        m_model.localPosition = new Vector3(m_pathOffset, 0f);
        transform.localRotation = m_direction.GetRotation();
        m_progressFactor = 2f * m_speed;
    }

    /// <summary>
    /// 使Enemy对象承受伤害并减少其生命值。
    /// </summary>
    /// <param name="damage">施加给Enemy的伤害量，必须为非负数。</param>
    public void ApplyDamage(float damage)
    {
        Debug.Assert(damage >= 0f, "Negative damage applied.");

        Health -= damage;
    }

    #endregion

    #region 属性

    /// <summary>
    /// 表示创建该Enemy实例的工厂引用。
    /// 用于确保当Enemy不再需要时，能被其创建的工厂正确地回收和处理。
    /// </summary>
    public EnemyFactory OriginFactory
    {
        get => m_originFactory;
        set
        {
            Debug.Assert(m_originFactory == null, "Redefined origin factory!");
            m_originFactory = value;
        }
    }

    /// <summary>
    ///  Enemy模型的缩放因子，用于调整Enemy在场景中的大小。
    /// </summary>
    public float Scale { get; private set; }

    /// <summary>
    ///  Enemy的生命值。
    /// </summary>
    private float Health { get; set; }

    /// <summary>
    /// 敌人目标点碰撞器设置属性。
    /// 允许外部设置敌人用于检测目标点碰撞的Collider组件，
    /// </summary>
    /// <value>待设置的目标点碰撞器。</value>
    public Collider TargetPointCollider
    {
        set
        {
            Debug.Assert(m_targetPointCollider == null, "Redefined collider!");

            m_targetPointCollider = value;
        }
    }

    /// <summary>
    /// 表示当前敌人是否为有效的攻击目标。
    /// </summary>
    public bool IsValidTarget => m_animator.CurrentClip == EnemyAnimator.Clip.Move;

    #endregion

    #region 字段

    /// <summary>
    /// Enemy的原始工厂实例，用于跟踪创建该Enemy的工厂。
    /// 用于确保对象由正确的工厂进行回收处理。
    /// </summary>
    private EnemyFactory m_originFactory;

    /// <summary>
    /// Enemy模型的Transform组件，用于在场景中表示Enemy的显示层
    /// </summary>
    [SerializeField]
    private Transform m_model = default;

    /// <summary>
    /// 当前Enemy所处的方块，用于追踪Enemy移动的起点。
    /// </summary>
    private GameTile m_tileFrom;

    /// <summary>
    /// 表示Enemy即将移动到的目标方块。
    /// </summary>
    private GameTile m_tileTo;

    /// <summary>
    /// Enemy当前位置的三维向量，表示Enemy当前所在位置的坐标信息。
    /// </summary>
    private Vector3 m_positionFrom;

    /// <summary>
    /// 表示Enemy即将移动到的目标位置坐标。
    /// </summary>
    private Vector3 m_positionTo;

    /// <summary>
    /// 表示Enemy在当前路径段或者（转身）上移动的进度（取值范围：0.0 到 1.0）。
    /// </summary>
    private float m_progress;

    /// <summary>
    /// 进度因子，用于调整Enemy 不同阶段的速度
    /// </summary>
    private float m_progressFactor;

    /// <summary>
    /// 表示Enemy的当前行进方向。
    /// </summary>
    private Direction m_direction;

    /// <summary>
    /// 表示Enemy移动方向的变化状态。
    /// 用来控制Enemy在路径上转向或继续直行的动作。
    /// </summary>
    private DirectionChange m_directionChange;

    /// <summary>
    /// 表示Enemy当前朝向的角度起始值，用于计算平滑的旋转过渡。
    /// </summary>
    private float m_directionAngleFrom;

    /// <summary>
    /// 表示Enemy转向目标时的角度。
    /// </summary>
    private float m_directionAngleTo;

    /// <summary>
    /// 路径偏移量，用于在Enemy转弯或前进时调整模型在路径上的初始位置。
    /// </summary>
    private float m_pathOffset;

    /// <summary>
    /// Enemy移动速度。
    /// </summary>
    private float m_speed;

    /// <summary>
    /// Enemy的动画配置 
    /// </summary>
    [SerializeField]
    private EnemyAnimationConfig m_animationConfig = default;

    /// <summary>
    /// Enemy的动画控制器
    /// </summary>
    private EnemyAnimator m_animator;

    /// <summary>
    /// 敌人目标点碰撞器引用。
    /// 用于检测敌人到达特定目标点的碰撞事件，比如完成路径上的一个阶段或触发某些逻辑。
    /// </summary>
    private Collider m_targetPointCollider;

    #endregion
}