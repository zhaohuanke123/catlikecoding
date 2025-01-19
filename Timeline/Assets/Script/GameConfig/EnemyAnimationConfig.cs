using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class EnemyAnimationConfig : ScriptableObject
{
    #region 属性

    /// <summary>
    /// 获取敌人的移动动画
    /// </summary>
    public AnimationClip Move => m_move;

    /// <summary>
    /// 获取敌人的进场动画
    /// </summary>
    public AnimationClip Intro => m_intro;

    /// <summary>
    /// 获取敌人的退场动画
    /// </summary>
    public AnimationClip Outro => m_outro;

    /// <summary>
    /// 获取敌人的死亡动画
    /// </summary>
    public AnimationClip Dying => m_dying;

    /// <summary>
    /// 敌人移动动画的速度
    /// </summary>
    public float MoveAnimationSpeed => m_moveAnimationSpeed;

    /// <summary>
    /// 获取敌人的出现动画
    /// </summary>
    public AnimationClip Appear => m_appear;

    /// <summary>
    ///  获取敌人的消失动画
    /// </summary>
    public AnimationClip Disappear => m_disappear;

    #endregion

    #region 字段

    /// <summary>
    ///  Enemy的移动动画
    /// </summary>
    [SerializeField]
    private AnimationClip m_move = default;

    /// <summary>
    ///  Enemy的进场动画
    /// </summary>
    [SerializeField]
    private AnimationClip m_intro = default;

    /// <summary>
    ///  Enemy的退场动画
    /// </summary>
    [SerializeField]
    private AnimationClip m_outro = default;

    /// <summary>
    ///  Enemy的死亡动画
    /// </summary>
    [SerializeField]
    private AnimationClip m_dying = default;

    /// <summary>
    /// 敌人移动动画的速度
    /// </summary>
    [SerializeField]
    private float m_moveAnimationSpeed = 1f;

    /// <summary>
    ///  出现动画
    /// </summary>
    [SerializeField]
    private AnimationClip m_appear = default;

    /// <summary>
    /// 消失动画 
    /// </summary>
    [SerializeField]
    private AnimationClip m_disappear = default;

    #endregion
}