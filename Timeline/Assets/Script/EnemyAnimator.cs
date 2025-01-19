using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[System.Serializable]
public struct EnemyAnimator
{
    #region 嵌套类型

    /// <summary>
    ///  敌人动画片段的类型枚举
    /// </summary>
    public enum Clip
    {
        Move,
        Intro,
        Outro,
        Dying,
        Appear,
        Disappear
    }

    #endregion

    #region 方法

    public void GameUpdate()
    {
        if (m_transitionProgress >= 0f)
        {
            // 1. 更新动画播放器混合进度
            m_transitionProgress += Time.deltaTime * TransitionSpeed;
            // 2. 如果过渡进度大于1，表示过渡完成
            if (m_transitionProgress >= 1f)
            {
                m_transitionProgress = -1f;
                SetWeight(CurrentClip, 1f);
                SetWeight(m_previousClip, 0f);
                GetPlayable(m_previousClip).Pause();
            }
            // 3. 否则，根据过渡进度设置动画片段的权重
            else
            {
                SetWeight(CurrentClip, m_transitionProgress);
                SetWeight(m_previousClip, 1f - m_transitionProgress);
            }
        }

#if UNITY_EDITOR
        m_clipTime = GetPlayable(CurrentClip).GetTime();
#endif
    }

    /// <summary>
    /// 配置Enemy的动画播放器，根据传入的Animator组件和动画配置初始化动画混合器。
    /// </summary>
    /// <param name="animator">敌人的Animator组件，用于控制动画播放。</param>
    /// <param name="config">包含敌人的各种动画片段的配置对象。</param>
    public void Configure(Animator animator, EnemyAnimationConfig config)
    {
        m_hasAppearClip = config.Appear;
        m_hasDisappearClip = config.Disappear;
        // 1. 创建PlayableGraph对象和动画混合器
        m_graph = PlayableGraph.Create();
        m_graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        m_mixer = AnimationMixerPlayable.Create(m_graph, m_hasAppearClip || m_hasDisappearClip ? 6 : 4);
        // 2. 创建动画并连接到混合器
        // 移动动画
        var clip = AnimationClipPlayable.Create(m_graph, config.Move);
        // 这是因为无论权重如何，所有Clip的时间都会流逝。
        clip.Pause();
        m_mixer.ConnectInput((int)Clip.Move, clip, 0);

        // 进场动画
        clip = AnimationClipPlayable.Create(m_graph, config.Intro);
        clip.SetDuration(config.Intro.length);
        clip.Pause();
        m_mixer.ConnectInput((int)Clip.Intro, clip, 0);

        // 退场动画
        clip = AnimationClipPlayable.Create(m_graph, config.Outro);
        clip.SetDuration(config.Outro.length);
        clip.Pause();
        m_mixer.ConnectInput((int)Clip.Outro, clip, 0);

        // 死亡动画
        clip = AnimationClipPlayable.Create(m_graph, config.Dying);
        clip.SetDuration(config.Dying.length);
        clip.Pause();
        m_mixer.ConnectInput((int)Clip.Dying, clip, 0);

        // 出现动画
        if (m_hasAppearClip)
        {
            clip = AnimationClipPlayable.Create(m_graph, config.Appear);
            clip.SetDuration(config.Appear.length);
            clip.Pause();
            m_mixer.ConnectInput((int)Clip.Appear, clip, 0);
        }

        // 消失动画
        if (m_hasDisappearClip)
        {
            clip = AnimationClipPlayable.Create(m_graph, config.Disappear);
            clip.SetDuration(config.Disappear.length);
            clip.Pause();
            m_mixer.ConnectInput((int)Clip.Disappear, clip, 0);
        }

        // 3. 创建PlayableOutput对象并连接到混合器
        var output = AnimationPlayableOutput.Create(m_graph, "Enemy", animator);
        output.SetSourcePlayable(m_mixer);
    }

    public void PlayDying()
    {
        BeginTransition(Clip.Dying);

        if (m_hasDisappearClip)
        {
            PlayDisappearFor(Clip.Dying);
        }
    }

    /// <summary>
    ///  播放敌人的进场动画
    /// </summary>
    public void PlayIntro()
    {
        SetWeight(Clip.Intro, 1f);
        GetPlayable(Clip.Intro).Play();
        CurrentClip = Clip.Intro;
        m_graph.Play();
        m_transitionProgress = -1f;

        if (m_hasAppearClip)
        {
            GetPlayable(Clip.Appear).Play();
            SetWeight(Clip.Appear, 1f);
        }
    }

    /// <summary>
    ///  播放敌人的退场动画
    /// </summary>
    public void PlayOutro()
    {
        BeginTransition(Clip.Outro);

        if (m_hasDisappearClip)
        {
            PlayDisappearFor(Clip.Outro);
        }
    }

    /// <summary>
    /// 播放敌人的移动动画，并允许设置动画播放速度。
    /// </summary>
    /// <param name="speed">移动动画的播放速度，可以大于1加速播放，小于1减速播放。</param>
    public void PlayMove(float speed)
    {
        GetPlayable(Clip.Move).SetSpeed(speed);
        BeginTransition(Clip.Move);

        if (m_hasAppearClip)
        {
            SetWeight(Clip.Appear, 0f);
        }
    }

    /// <summary>
    /// 播放"消失"动画片段，该方法会在当前其他动画片段（由otherClip指定）结束前的一段时间开始播放消失动画，
    /// 以实现平滑过渡效果。
    /// </summary>
    /// <param name="otherClip">当前正在播放或即将播放的其他动画片段类型，用于计算消失动画的起始时间点。</param>
    private void PlayDisappearFor(Clip otherClip)
    {
        var clip = GetPlayable(Clip.Disappear);
        clip.Play();
        clip.SetDelay(GetPlayable(otherClip).GetDuration() - clip.GetDuration());
        SetWeight(Clip.Disappear, 1f);
    }

    /// <summary>
    /// 获取与指定敌人动画片段关联的Playable对象。
    /// </summary>
    /// <param name="clip">要获取其Playable对象的动画片段类型。</param>
    /// <returns>与指定动画片段关联的Playable对象。</returns>
    private Playable GetPlayable(Clip clip)
    {
        return m_mixer.GetInput((int)clip);
    }

    /// <summary>
    /// 开始进行动画之间的过渡。
    /// </summary>
    /// <param name="nextClip">即将播放的下一个动画片段。</param>
    private void BeginTransition(Clip nextClip)
    {
        m_previousClip = CurrentClip;
        CurrentClip = nextClip;
        m_transitionProgress = 0f;
        GetPlayable(nextClip).Play();
    }

    /// <summary>
    ///  停止播放敌人的动画
    /// </summary>
    public void Stop()
    {
        m_graph.Stop();
    }

    /// <summary>
    ///  销毁敌人的动画播放器
    /// </summary>
    public void Destroy()
    {
        m_graph.Destroy();
    }

    /// <summary>
    /// 设置指定敌人动画片段在混合器中的权重，控制动画的淡入淡出效果。
    /// </summary>
    /// <param name="clip">要设置权重的动画片段类型。</param>
    /// <param name="weight">动画片段的权重值，范围通常在0到1之间，用于控制动画的混合程度。</param>
    private void SetWeight(Clip clip, float weight)
    {
        m_mixer.SetInputWeight((int)clip, weight);
    }

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器热重载后恢复Enemy Animator的状态。
    /// 此方法确保在Unity编辑器热重载时，Enemy的动画状态能得到正确的重建，
    /// 由于Playable不可序列化，因此需要在热重载后重新配置动画播放器。
    /// </summary>
    /// <param name="animator">敌人的Animator组件引用，用于重新配置动画播放器。</param>
    /// <param name="config">动画配置对象，包含所有动画片段的引用。</param>
    /// <param name="speed">播放特定动画片段（如移动）时的速度。</param>
    public void RestoreAfterHotReload(Animator animator, EnemyAnimationConfig config, float speed)
    {
        // 1. 重新配置动画播放器
        Configure(animator, config);
        
        // 2. 恢复动画播放器的状态
        GetPlayable(Clip.Move).SetSpeed(speed);
        SetWeight(CurrentClip, 1f);
        var clip = GetPlayable(CurrentClip);
        clip.SetTime(m_clipTime);
        clip.Play();
        m_graph.Play();

        // 3. 恢复出现和消失动画
        if (CurrentClip == Clip.Intro && m_hasAppearClip)
        {
            clip = GetPlayable(Clip.Appear);
            clip.SetTime(m_clipTime);
            clip.Play();
            SetWeight(Clip.Appear, 1f);
        }
        else if (CurrentClip >= Clip.Outro && m_hasDisappearClip)
        {
            clip = GetPlayable(Clip.Disappear);
            clip.Play();
            double delay = GetPlayable(CurrentClip).GetDuration() - clip.GetDuration() - m_clipTime;
            if (delay >= 0f)
            {
                clip.SetDelay(delay);
            }
            else
            {
                clip.SetTime(-delay);
            }

            SetWeight(Clip.Disappear, 1f);
        }
    }
#endif

    #endregion

    #region 属性

    /// <summary>
    /// 当前正在播放的动画片段。
    /// </summary>
    public Clip CurrentClip { get; private set; }

    /// <summary>
    /// 表示当前播放的动画片段是否已完成。
    /// </summary>
    /// <value><c>true</c> 如果当前播放的动画片段已经结束；否则为 <c>false</c>。</value>
    public bool IsDone => GetPlayable(CurrentClip).IsDone();

#if UNITY_EDITOR
    /// <summary>
    /// 表示当前EnemyAnimator实例是否有效，这依赖于内部PlayableGraph的状态。
    /// </summary>
    /// <value><c>true</c> 如果PlayableGraph有效，即可以正常处理动画；否则，<c>false</c>。</value>
    public bool IsValid => m_graph.IsValid();
#endif

    #endregion

    #region 字段

    /// <summary>
    /// playableGraph图
    /// </summary>
    private PlayableGraph m_graph;

    /// <summary>
    ///  Enemy的动画混合器
    /// </summary>
    private AnimationMixerPlayable m_mixer;

    /// <summary>
    ///  需要过度的上一个动画
    /// </summary>
    private Clip m_previousClip;

    /// <summary>
    /// 过度进度
    /// </summary>
    private float m_transitionProgress;

    /// <summary>
    /// 动画过度速度
    /// </summary>
    private const float TransitionSpeed = 5f;

    /// <summary>
    ///  是否有出现动画
    /// </summary>
    private bool m_hasAppearClip;

    /// <summary>   
    /// 是否有消失动画 
    /// </summary>
    private bool m_hasDisappearClip;

#if UNITY_EDITOR
    /// <summary>
    /// 动画的当前播放时间。
    /// 此属性用于在热重载后恢复动画到之前的时间点，以保持动画连续性。
    /// </summary>
    private double m_clipTime;
#endif

    #endregion
}