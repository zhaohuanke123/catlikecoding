using UnityEngine;

/// <summary>
///  游戏行为基类
/// </summary>
public class GameBehavior : MonoBehaviour
{
    #region 方法

    /// <summary>
    /// 在派生类中实现时，提供游戏内对象的更新逻辑。
    /// 返回布尔值表示对象是否应继续存在和更新，若返回false则可能表示游戏对象已达到其生命周期的终点或应被移除。
    /// </summary>
    /// <returns>如果对象仍有效并应继续参与游戏循环，则返回true；否则返回false。</returns>
    public virtual bool GameUpdate() => true;

    #endregion
}