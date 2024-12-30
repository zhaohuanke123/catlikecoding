using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

/// <summary>
///  性能检测的显示模式
/// </summary>
public enum DisplayMode
{
    FPS,
    MS
}

/// <summary>
///  帧率计数器
/// </summary>
public class FrameRateCounter : MonoBehaviour
{
    #region 方法

    private void Update()
    {
        // 1. 获取当前帧经过的时间, 并记录经过的帧数，时间
        float frameDuration = Time.unscaledDeltaTime;
        m_frames += 1;
        m_duration += frameDuration;

        // 2. 记录最好和最差的帧时间
        if (frameDuration < m_bestDuration)
        {
            m_bestDuration = frameDuration;
        }

        if (frameDuration > m_worstDuration)
        {
            m_worstDuration = frameDuration;
        }

        // 3. 如果经过的时间大于等于采样时间, 则更新显示性能数据
        // 计算公式：FPS = 1 / 帧时间
        if (m_duration >= m_sampleDuration)
        {
            if (m_displayMode == DisplayMode.FPS)
            {
                m_display.SetText(
                    "FPS\nbest: {0:1}\nave:  {1:1}\nworst: {2:1}",
                    1f / m_bestDuration,
                    m_frames / m_duration,
                    1f / m_worstDuration
                );
            }
            else
            {
                m_display.SetText(
                    "FPS\nbest: {0:1}\nave:  {1:1}\nworst: {2:1}",
                    1000f * m_bestDuration,
                    1000f * m_duration / m_frames,
                    1000f * m_worstDuration
                );
            }

            ResetData();
        }
    }

    /// <summary>
    ///  重置数据
    /// </summary>
    private void ResetData()
    {
        m_frames = 0;
        m_duration = 0f;
        m_bestDuration = float.MaxValue;
        m_worstDuration = 0f;
    }

    #endregion


    #region 内部字段

    /// <summary>
    ///  显示性能数据的TMP文本
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI m_display;

    /// <summary>
    ///  显示模式枚举
    /// </summary>
    [SerializeField]
    private DisplayMode m_displayMode = DisplayMode.FPS;

    /// <summary>
    ///  采样时间，更新显示数据的时间
    /// </summary>
    [SerializeField, Range(0.1f, 2f)]
    private float m_sampleDuration = 1f;

    /// <summary>
    ///  经过的帧数
    /// </summary>
    private int m_frames = 0;

    /// <summary>
    ///  经过时间
    /// </summary>
    private float m_duration = float.MaxValue;

    /// <summary>
    ///  最好帧时间
    /// </summary>
    private float m_bestDuration = float.MaxValue;

    /// <summary>
    ///  最差帧时间
    /// </summary>
    private float m_worstDuration = 0;

    #endregion
}