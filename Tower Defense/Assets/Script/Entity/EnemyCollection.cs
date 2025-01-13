using System.Collections.Generic;

[System.Serializable]
public class EnemyCollection
{
    #region Unity 生命周期

    #endregion

    #region 方法

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

    public void Add(Enemy enemy)
    {
        m_enemies.Add(enemy);
    }

    #endregion

    #region 事件

    #endregion

    #region 属性

    #endregion

    #region 字段

    private List<Enemy> m_enemies = new List<Enemy>();

    #endregion
}