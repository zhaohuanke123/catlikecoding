using System.Collections.Generic;

[System.Serializable]
public class EnemyCollection
{
    #region 方法

    /// <summary>
    /// 更新Enemy类集合的状态。 在每一帧调用以遍历所有Enemy并执行它们的GameUpdate方法， 移除已失效（如到达终点或被移除）的Enemy。
    /// </summary>
    public void GameUpdate()
    {
        for (int i = 0; i < m_enemies.Count; i++)
        {
            if (!m_enemies[i].GameUpdate())
            {
                int lastIndex = m_enemies.Count - 1;
                m_enemies[i] = m_enemies[lastIndex];
                m_enemies.RemoveAt(lastIndex);
                i -= 1;
                Game.OnEnemyCountChanged(Count);
            }
        }
    }

    /// <summary>
    /// 向Collection中添加一个新的Enemy实例。
    /// </summary>
    /// <param name="enemy">要添加到集合中的Enemy对象。</param>
    public void Add(Enemy enemy)
    {
        m_enemies.Add(enemy);
        Game.OnEnemyCountChanged(Count);
    }

    /// <summary>
    /// 清空集合中的所有Enemy。
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < m_enemies.Count; i++)
        {
            m_enemies[i].Recycle();
        }

        Game.OnEnemyCountChanged(Count);
        m_enemies.Clear();
    }

    #endregion


    #region 属性

    /// <summary>
    ///  行为集合是否为空。
    /// </summary>
    /// <value>如果集合为空，则返回true；否则返回false。</value>
    public bool IsEmpty => m_enemies.Count == 0;

    /// <summary>
    ///  行为集合中的数量。 
    /// </summary>
    public int Count => m_enemies.Count;

    #endregion

    #region 字段

    /// <summary>
    /// Enemy列表，存储游戏中所有的Enemy实例。
    /// </summary>
    private List<Enemy> m_enemies = new List<Enemy>();

    #endregion
}