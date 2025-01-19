using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    #region Unity 生命周期

    private void OnEnable()
    {
        if (Game.Instance)
        {
            Game.Instance.EventOnEnemyCountChanged += OnEnemyCountChanged;
            Game.Instance.EventOnCycleChanged += OnCycleChanged;
            Game.Instance.EventOnWaveChanged += OnWaveChanged;
            Game.Instance.EventOnPlayerHealthChanged += OnPlayerHealthChanged;
            Game.Instance.EventOnGameEnd += OnEventOnGameEnd;
        }

        m_startButton.onClick.AddListener(OnStartButtonClick);
    }

    private void OnDisable()
    {
        if (Game.Instance)
        {
            Game.Instance.EventOnEnemyCountChanged -= OnEnemyCountChanged;
            Game.Instance.EventOnCycleChanged -= OnCycleChanged;
            Game.Instance.EventOnWaveChanged -= OnWaveChanged;
            Game.Instance.EventOnPlayerHealthChanged -= OnPlayerHealthChanged;
            Game.Instance.EventOnGameEnd -= OnEventOnGameEnd;
        }

        m_startButton.onClick.RemoveListener(OnStartButtonClick);
    }

    #endregion

    #region 方法

    /// <summary>
    /// 敌人数量变化时触发的方法，用于更新UI显示的敌人数量信息。
    /// </summary>
    /// <param name="count">当前敌人的数量。</param>
    private void OnEnemyCountChanged(int count)
    {
        string message = $"当前的敌人数量：{count}";
        m_enemyCountText.text = message;
    }

    /// <summary>
    /// 游戏循环次数变化时触发的方法，用于更新UI上显示的当前循环次数信息。
    /// </summary>
    /// <param name="cycle">当前的循环次数。</param>
    /// <param name="allCycle">总循环</param>
    private void OnCycleChanged(int cycle, int allCycle)
    {
        string message = $"当前的轮次：{cycle + 1}/{allCycle}";
        m_cycleText.text = message;
    }

    /// <summary>
    /// 游戏波次变化时触发的方法，用于更新UI上显示的当前波次信息。
    /// </summary>
    /// <param name="wave">波次</param>
    /// <param name="allWave">总波次</param>
    private void OnWaveChanged(int wave, int allWave)
    {
        string message = $"当前的波次：{wave + 1}/{allWave}";
        m_waveText.text = message;
    }

    /// <summary>
    /// 玩家生命值变化时触发的方法，用于更新UI上显示的玩家生命值信息。
    /// </summary>
    /// <param name="health">当前玩家的生命值。</param>
    /// <param name="allHp">总的Hp</param>
    private void OnPlayerHealthChanged(int health, int allHp)
    {
        string message = $"HP：{health}/{allHp}";
        m_hpText.text = message;
        if (!Mathf.Approximately(m_hpSlider.maxValue, allHp))
        {
            m_hpSlider.maxValue = allHp;
        }

        m_hpSlider.value = health;
    }

    /// <summary>
    /// 开始游戏按钮点击时触发的方法，用于开始游戏。 
    /// </summary>
    private void OnStartButtonClick()
    {
        Game.StartGame();
        m_startButton.gameObject.SetActive(false);
        m_gameResImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// 游戏结束时触发的事件处理方法，负责更新游戏结果的UI展示。
    /// </summary>
    /// <param name="isWin">表示游戏结果的布尔值，true为胜利，false为失败。</param>
    private void OnEventOnGameEnd(bool isWin)
    {
        m_gameResImage.gameObject.SetActive(true);
        m_gameResImage.color = isWin ? Color.green : Color.red;
        m_gameResText.text = isWin ? "胜利" : "失败";

        m_startButton.gameObject.SetActive(true);
    }

    #endregion

    #region 字段

    /// <summary>
    /// 显示当前敌人数量的文本组件。
    /// </summary>
    [SerializeField]
    private Text m_enemyCountText;

    /// <summary>
    /// 当前循环次数信息的文本组件。
    /// </summary>
    [SerializeField]
    private Text m_cycleText;

    /// <summary>
    /// 表示当前波次信息的文本组件。
    /// </summary>
    [SerializeField]
    private Text m_waveText;

    /// <summary>
    /// 显示玩家生命值的文本组件。 
    /// </summary>
    [SerializeField]
    private Text m_hpText;

    /// <summary>
    ///  开始游戏的按钮组件。
    /// </summary>
    [SerializeField]
    private Button m_startButton;

    /// <summary>
    ///  游戏结束时显示的图片组件。
    /// </summary>
    [SerializeField]
    private Image m_gameResImage;

    /// <summary>
    ///  游戏结束时显示的文本组件。
    /// </summary>
    [SerializeField]
    private Text m_gameResText;

    /// <summary>
    /// 表示玩家生命值的滑动条组件。
    /// </summary>
    [SerializeField]
    private Slider m_hpSlider;

    #endregion
}