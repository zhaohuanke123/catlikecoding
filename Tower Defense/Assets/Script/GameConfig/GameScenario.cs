using UnityEngine;

[CreateAssetMenu]
public class GameScenario : ScriptableObject
{
    /// <summary>
    /// 游戏场景状态结构体，用于追踪游戏场景中敌人的生成波次状态。
    /// </summary>
    [System.Serializable]
    public struct State
    {
        #region 构造器

        /// <summary>
        ///  构造一个状态实例
        /// </summary>
        /// <param name="scenario"> 游戏创建会话 </param>
        public State(GameScenario scenario)
        {
            m_scenario = scenario;
            m_index = 0;
            m_cycle = 0;
            m_timeScale = 1f;

            Debug.Assert(scenario.m_waves.Length > 0, "Empty scenario!");

            m_wave = scenario.m_waves[0].Begin();
        }

        #endregion

        #region 方法

        /// <summary>
        /// 进行游戏状态的推进，根据时间变化更新敌人波次的状态。
        /// </summary>
        /// <returns>
        /// 返回布尔值，表示是否还有更多波次的敌人待生成。
        /// 当所有波次的敌人都已完成生成时返回false，否则返回true。
        /// </returns>
        public bool Progress()
        {
            // 1. 更新波次的生成进度
            float deltaTime = m_wave.Progress(m_timeScale * Time.deltaTime);
            while (deltaTime >= 0f)
            {
                // 2. 如果当前波次已完成，切换到下一个波次
                if (++m_index >= m_scenario.m_waves.Length)
                {
                    // 3. 如果循环 cycles 次数已达到，返回 false
                    if (++m_cycle >= m_scenario.m_cycles && m_scenario.m_cycles > 0)
                    {
                        return false;
                    }

                    m_index = 0;
                    m_timeScale += m_scenario.m_cycleSpeedUp;
                }

                m_wave = m_scenario.m_waves[m_index].Begin();
                deltaTime = m_wave.Progress(deltaTime);
            }

            return true;
        }

        #endregion

        #region 字段

        /// <summary>
        /// 当前游戏场景的实例，用于追踪和控制游戏场景中的敌人生成波次。
        /// </summary>
        private GameScenario m_scenario;

        /// <summary>
        /// 当前波次中的索引位置，用于追踪游戏中敌人生成序列的进度。
        /// </summary>
        private int m_index;

        /// <summary>
        /// 当前波次的状态，包含了关于敌人生成序列的详细信息。
        /// </summary>
        private EnemyWave.State m_wave;

        /// <summary>
        /// 当前循环次数 
        /// </summary>
        private int m_cycle;

        /// <summary>
        /// 当前时间缩放 
        /// </summary>
        private float m_timeScale;

        #endregion
    }

    #region 方法

    /// <summary>
    /// 开始游戏场景会话
    /// </summary>
    /// <returns></returns>
    public State Begin() => new State(this);

    #endregion

    #region 字段

    /// <summary>
    /// 敌人生成波次
    /// </summary>
    [SerializeField]
    private EnemyWave[] m_waves = { };

    /// <summary>
    /// 生成的循环次数
    /// </summary>
    [SerializeField]
    [Range(0, 10)]
    private int m_cycles = 1;

    /// <summary>
    /// 时间缩放加速，每次循环加速的比例
    /// </summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float m_cycleSpeedUp = 0.5f;

    #endregion
}