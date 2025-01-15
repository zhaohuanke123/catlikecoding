using UnityEngine;

/// <summary>
///  War实体类，塔防游戏实体基类
/// </summary>
public class WarEntity : GameBehavior
{
    #region 方法

    /// <summary>
    ///  回收War实体
    /// </summary>
    public void Recycle()
    {
        m_originFactory.Reclaim(this);
    }

    #endregion

    #region 属性

    /// <summary>
    ///  表示创建该War实体实例的工厂引用。
    /// </summary>
    public WarFactory OriginFactory
    {
        get => m_originFactory;
        set
        {
            Debug.Assert(m_originFactory == null, "Redefined origin factory!");
            m_originFactory = value;
        }
    }

    #endregion

    #region 字段

    /// <summary>
    ///  War实体的原始工厂实例，用于跟踪创建该War实体的工厂。
    /// </summary>
    private WarFactory m_originFactory;

    #endregion
}