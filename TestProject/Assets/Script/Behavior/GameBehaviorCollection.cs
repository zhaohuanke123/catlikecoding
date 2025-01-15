using System.Collections.Generic;

public class GameBehaviorCollection
{
    #region 方法

    /// <summary>
    /// 更新游戏行为集合内的所有行为。
    /// </summary>
    public void GameUpdate()
    {
        // 1. 此方法遍历行为列表，对每个行为调用GameUpdate方法。
        for (int i = 0; i < m_behaviors.Count; i++)
        {
            // 2. 2. 如果某个行为的GameUpdate返回false，该行为将被移除出列表并跳过本次循环的剩余行为。
            if (!m_behaviors[i].GameUpdate())
            {
                int lastIndex = m_behaviors.Count - 1;
                m_behaviors[i] = m_behaviors[lastIndex];
                m_behaviors.RemoveAt(lastIndex);
                i -= 1;
            }
        }
    }

    /// <summary>
    /// 向集合中添加一个新的行为。
    /// </summary>
    /// <param name="behavior">要添加到集合中的行为实例。</param>
    public void Add(GameBehavior behavior)
    {
        m_behaviors.Add(behavior);
    }

    #endregion

    #region 字段

    /// <summary>
    /// 行为集合类，用于管理并执行一系列行为的更新。
    /// </summary>
    private List<GameBehavior> m_behaviors = new List<GameBehavior>();

    #endregion
}