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
        if (m_previousClipPort == -1)
        {
            if (m_currentClipPort != -1)
            {
                SetWeight(m_currentClipPort, 1);
            }

            return;
        }

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

    public void Init(Animator animator)
    {
        m_graph = PlayableGraph.Create();
        m_graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        m_mixer = AnimationMixerPlayable.Create(m_graph);

        var output = AnimationPlayableOutput.Create(m_graph, "Player", animator);
        output.SetSourcePlayable(m_mixer);
        // 不加这个没有动画？
        m_graph.Play();

        m_clipNameToIndex = new Dictionary<string, int>();
        m_previousClipPort = -1;
        m_currentClipPort = -1;
    }

    public void Play(AnimClipData clipData)
    {
        string animName = clipData.m_animName;
        AnimationClip clip = clipData.m_clip;
        if (!m_clipNameToIndex.TryGetValue(animName, out int inputPort))
        {
            inputPort = m_clipNameToIndex.Count;
            m_clipNameToIndex.Add(animName, inputPort);
            var clipPlayable = AnimationClipPlayable.Create(m_graph, clip);

            if (!clipData.m_loop)
            {
                clipPlayable.SetDuration(clip.length);
            }

            clipPlayable.Pause();
            m_mixer.SetInputCount(m_clipNameToIndex.Count);
            m_mixer.ConnectInput(inputPort, clipPlayable, 0);
        }

        BeginTransition(inputPort);
    }

    private Playable GetPlayable(int port)
    {
        return m_mixer.GetInput(port);
    }


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