using System;
using UnityEngine;

/// <summary>
/// 敌人生成序列类，用于存储敌人生成的相关属性。 
/// </summary>
[Serializable]
public class EnemySpawnSequence
{
    #region 嵌套类

    /// <summary>
    /// 状态结构体，用于表示场景的生成状态
    /// </summary>
    [Serializable]
    public struct State
    {
        #region 构造器

        /// <summary>
        ///  构造一个状态实例
        /// </summary>
        public State(EnemySpawnSequence sequence)
        {
            m_sequence = sequence;
            m_count = 0;
            m_cooldown = sequence.m_cooldown;
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
            // 1. 更新生成时间计数
            m_cooldown += deltaTime;
            while (m_cooldown >= m_sequence.m_cooldown)
            {
                // 2. 如果生成时间计数超过生成 CD 时间，生成一个敌人
                m_cooldown -= m_sequence.m_cooldown;
                if (m_count >= m_sequence.m_amount)
                {
                    return m_cooldown;
                }

                m_count += 1;
                Game.SpawnEnemy(m_sequence.m_factory, m_sequence.m_type);
            }

            return -1;
        }

        #endregion

        #region 字段

        /// <summary>
        /// 敌人生成序列的实例，用于在状态类中存储和操作当前场景的敌人生成配置。
        /// </summary>
        private EnemySpawnSequence m_sequence;

        /// <summary>
        /// 已生成敌人的数量 
        /// </summary>
        private int m_count;

        /// <summary>
        ///  生成敌人的时间计数
        /// </summary>
        private float m_cooldown;

        #endregion
    }

    #endregion

    #region 属性

    /// <summary>
    /// 开始一个新的生成序列状态。
    /// </summary>
    /// <returns>
    /// 返回一个新的<see cref="State"/>实例，表示生成序列的起始状态。
    /// </returns>
    public State Begin() => new State(this);

    #endregion

    #region 字段

    /// <summary>
    ///  生成敌人的工厂
    /// </summary>
    [SerializeField]
    private EnemyFactory m_factory = default;

    /// <summary>
    ///  生成敌人的类型
    /// </summary>
    [SerializeField]
    private EnemyType m_type = EnemyType.Medium;

    /// <summary>
    ///  生成敌人的数量
    /// </summary>
    [SerializeField]
    [Range(1, 100)]
    private int m_amount = 1;

    /// <summary>
    ///  生成敌人的 CD 时间
    /// </summary>
    [SerializeField]
    [Range(0.1f, 10f)]
    private float m_cooldown = 1f;

    #endregion
}