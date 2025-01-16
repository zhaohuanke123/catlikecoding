using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))] //确保物体有Animator组件，有这个才可以播放动画
public class MixAnimationSample : MonoBehaviour
{
    /// <summary>
    /// 初始化并播放动画片段
    /// </summary>
    void Awake()
    {
        m_graph = PlayableGraph.Create();
        m_graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime); //设置时间更新模式

        m_mixer = AnimationMixerPlayable.Create(m_graph, 2); //要混合的片段数量
        //m_mixer = AnimationMixerPlayable.Create(m_graph);          //两种方法都可以创建AnimationMixerPlayable
        //m_mixer.SetInputCount(2);

        var clipPlayable1 = AnimationClipPlayable.Create(m_graph, m_clip1); //创建一个AnimationClipPlayable对象
        var clipPlayable2 = AnimationClipPlayable.Create(m_graph, m_clip2);

        // m_graph.Connect(clipPlayable1, 0, m_mixer, 0);
        // m_graph.Connect(clipPlayable2, 0, m_mixer, 1);

        //可以使用AddInput代替Connect和SetInputWeight  但为什么使用这个后，调整weight就不好用了？？？
        int p1 = m_mixer.AddInput(clipPlayable1, 0);
        int p2 = m_mixer.AddInput(clipPlayable2, 0, 1f);
        clipPlayable1.Play();
        clipPlayable2.Play();
        m_mixer.SetInputWeight(p1, 1f);
        m_mixer.SetInputWeight(p2, 0f);
        Debug.Log(p1);
        Debug.Log(p2);


        //创建输出节点，并设置输出源   将graph管理的动画输出到当前物体的组件Animator中
        var output = AnimationPlayableOutput.Create(m_graph, "Anim", GetComponent<Animator>());
        output.SetSourcePlayable(m_mixer); //输出的片段为mixer混合后的片段

        m_graph.Play(); //播放clipPlayable
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) //暂停播放
        {
            if (m_mixer.GetPlayState() == PlayState.Playing) //正在播放
            {
                m_mixer.Pause(); //暂停播放
            }
            else if (m_mixer.GetPlayState() == PlayState.Paused) //暂停状态
            {
                m_mixer.Play(); //播放
                //m_mixer.SetTime(0f); //重新播放
            }
        }

        // 监听键盘输入进行动画的过渡
        if (Input.GetKey(KeyCode.A)) // 按下A键
        {
            // A到B的过渡
            m_weight = Mathf.Lerp(m_weight, 0f, Time.deltaTime * 5f); // 淡出A，淡入B
        }
        else if (Input.GetKey(KeyCode.B)) // 按下B键
        {
            // B到A的过渡
            m_weight = Mathf.Lerp(m_weight, 1f, Time.deltaTime * 5f); // 淡出B，淡入A
        }

        m_weight = Mathf.Clamp01(m_weight);
        m_mixer.SetInputWeight(0, 1.0f - m_weight);
        m_mixer.SetInputWeight(1, m_weight);
        Debug.Log("weight = " + m_weight);
    }

    /// <summary>
    /// 销毁播放图
    /// </summary>
    void OnDisable()
    {
        //销毁该图创建的所有可播放项和 PlayableOutput。
        m_graph.Destroy();
        //Debug.Log($"({transform.position.x},{transform.position.y},{transform.position.z},)");
    }


    public AnimationClip m_clip1, m_clip2; //动画片段  可以在inspector中赋值
    private AnimationMixerPlayable m_mixer;
    private PlayableGraph m_graph; //用于管理和播放动画

    [Range(0f, 1f)]
    public float m_weight = 0.5f; //播放权重比例
}