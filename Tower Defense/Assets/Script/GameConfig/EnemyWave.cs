using UnityEngine;

/// <summary>
///  敌人生成波次类
/// </summary>
[CreateAssetMenu]
public class EnemyWave : ScriptableObject
{
    #region 嵌套类

    /// <summary>
    ///  敌人生成波次状态
    /// </summary>
    [System.Serializable]
    public struct State
    {
        #region 构造器

        /// <summary>
        /// 构造一个状态实例
        /// </summary>
        public State(EnemyWave wave)
        {
            m_wave = wave;
            m_index = 0;

            Debug.Assert(wave.m_spawnSequences.Length > 0, "Empty wave!");

            m_sequence = wave.m_spawnSequences[0].Begin();
        }

        #endregion

        #region 方法

        /// <summary>
        /// 计算并返回当前生成进度，根据给定的时间差更新状态。
        /// </summary>
        /// <param name="deltaTime">自上次调用以来经过的时间，单位为秒。</param>
        /// <returns>
        /// 如果生成序列已完成，则返回剩余的时间计数；如果序列未完成，则返回-1。
        /// </returns>
        public float Progress(float deltaTime)
        {
            // 1. 更新生成时间计数, 并检查是否需要切换到下一个生成序列
            deltaTime = m_sequence.Progress(deltaTime);
            while (deltaTime >= 0f)
            {
                // 2. 如果生成序列已完成，切换到下一个生成序列
                if (++m_index >= m_wave.m_spawnSequences.Length)
                {
                    return deltaTime;
                }

                // 3. 切换到下一个生成序列
                m_sequence = m_wave.m_spawnSequences[m_index].Begin();
                deltaTime = m_sequence.Progress(deltaTime);
            }

            return -1f;
        }

        #endregion

        #region 字段

        /// <summary>
        /// 当前敌人生成波次的引用。
        /// </summary>
        private EnemyWave m_wave;

        /// <summary>
        /// 当前敌人生成序列索引位置。
        /// </summary>
        private int m_index;

        /// <summary>
        /// 敌人生成序列的状态，表示当前生成过程中的某个特定点。
        /// 此变量用于追踪生成序列的内部状态，包括时间冷却、已完成的生成次数等。
        /// </summary>
        private EnemySpawnSequence.State m_sequence;

        #endregion
    }

    #endregion


    #region 方法

    /// <summary>
    /// 开始生成波次
    /// </summary>
    /// <returns></returns>
    public State Begin() => new State(this);

    #endregion


    #region 字段

    /// <summary>
    ///  敌人生成序列数组
    /// </summary>
    [SerializeField]
    private EnemySpawnSequence[] m_spawnSequences = { new EnemySpawnSequence() };

    #endregion
}