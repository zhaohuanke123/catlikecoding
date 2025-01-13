using System.Collections.Generic;

[System.Serializable]
public class EnemyCollection
{
    #region 方法

    /// <summary>
    ///  更新当前游戏中所有敌人的状态。
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
            }
        }
    }

    /// <summary>
    /// 向敌人类别集合中添加一个新的敌人实例。
    /// </summary>
    /// <param name="enemy">要添加到集合中的敌人实例。</param>
    public void Add(Enemy enemy)
    {
        m_enemies.Add(enemy);
    }

    #endregion

    #region 字段

    /// <summary>
    /// 用于存储当前游戏中所有的敌人实例。 
    /// </summary>
    private List<Enemy> m_enemies = new List<Enemy>();

    #endregion
}