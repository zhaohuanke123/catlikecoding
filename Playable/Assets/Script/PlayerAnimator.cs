using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// <summary>
/// 玩家动画控制器
/// </summary>
[System.Serializable]
public struct PlayerAnimator
{
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
                SetWeight(m_currentClipPort, 1f);
                SetWeight(m_previousClipPort, 0f);
                GetPlayable(m_previousClipPort).Pause();
            }
            // 3. 否则，根据过渡进度设置动画片段的权重
            else
            {
                SetWeight(m_currentClipPort, m_transitionProgress);
                SetWeight(m_previousClipPort, 1f - m_transitionProgress);
            }
        }
    }

    /// <summary>
    /// 初始化玩家动画控制器与给定的动画组件。
    /// </summary>
    /// <param name="animator">用于初始化Graph的 Animator 。</param>
    public void Init(Animator animator)
    {
        m_graph = PlayableGraph.Create();
        m_graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        m_mixer = AnimationMixerPlayable.Create(m_graph);

        var output = AnimationPlayableOutput.Create(m_graph, "Player" + Random.Range(0, 10), animator);
        output.SetSourcePlayable(m_mixer);
        m_graph.Play();

        m_clipNameToIndex = new Dictionary<string, int>();
        m_previousClipPort = -1;
        m_currentClipPort = -1;
        m_transitionProgress = -1;

        output.SetWeight(0.5f);
    }

    /// <summary>
    /// 播放指定动画数据的动画。
    /// </summary>
    /// <param name="clipData">要播放的动画片段数据，包含动画名称、是否循环和动画片段。</param>
    public void Play(AnimClipData clipData)
    {
        string animName = clipData.m_animName;
        AnimationClip clip = clipData.m_clip;

        // 1. 如果动画片段名称不存在，则添加到字典中
        if (!m_clipNameToIndex.TryGetValue(animName, out int inputPort))
        {
            // 1. 添加新的输入端口
            inputPort = m_clipNameToIndex.Count;
            m_clipNameToIndex.Add(animName, inputPort);
            var clipPlayable = AnimationClipPlayable.Create(m_graph, clip);

            // 2. 设置动画片段是否循环
            if (!clipData.m_loop)
            {
                clipPlayable.SetDuration(clip.length);
            }

            clipPlayable.Pause();
            m_mixer.SetInputCount(m_clipNameToIndex.Count);
            m_mixer.ConnectInput(inputPort, clipPlayable, 0);
        }

        // 2. 如果当前没有播放动画片段，则直接播放
        if (m_currentClipPort == -1)
        {
            m_currentClipPort = inputPort;
            GetPlayable(inputPort).Play();
            SetWeight(inputPort, 1);
        }
        // 3. 否则，开始过渡
        else
        {
            BeginTransition(inputPort);
        }
    }

    /// <summary>
    /// 获取与给定端口关联的可播放对象。
    /// </summary>
    /// <param name="port">要获取其关联可播放对象的输入端口。</param>
    /// <returns>与指定端口关联的可播放对象。</returns>
    private Playable GetPlayable(int port)
    {
        return m_mixer.GetInput(port);
    }

    /// <summary>
    /// 开始从当前动画向指定的新动画过渡。
    /// </summary>
    /// <param name="nextClip">要过渡到的下一个动画的输入端口。</param>
    private void BeginTransition(int nextClip)
    {
        m_previousClipPort = m_currentClipPort;
        m_currentClipPort = nextClip;
        m_transitionProgress = 0f;
        var playable = GetPlayable(nextClip);
        playable.SetTime(0);
        playable.Play();
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
    /// 设置动画片段的播放权重。
    /// </summary>
    /// <param name="port">要设置权重的动画输入端口索引。</param>
    /// <param name="weight">要分配给指定端口的权重值，范围从0（不播放）到1（完全播放）。</param>
    private void SetWeight(int port, float weight)
    {
        m_mixer.SetInputWeight(port, weight);
    }

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
    /// 过度进度
    /// </summary>
    private float m_transitionProgress;

    /// <summary>
    /// 动画过度速度
    /// </summary>
    private const float TransitionSpeed = 5f;

    /// <summary>
    /// 动画片段名称到输入端口索引的映射
    /// Key: 动画片段名称
    /// Value: 输入端口索引
    /// </summary>
    private Dictionary<string, int> m_clipNameToIndex;

    /// <summary>
    /// 上一个播放的动画端口。
    /// </summary>
    private int m_previousClipPort;

    /// <summary>
    /// 当前正在播放或即将播放的动画端口。
    /// </summary>
    private int m_currentClipPort;

    #endregion
}