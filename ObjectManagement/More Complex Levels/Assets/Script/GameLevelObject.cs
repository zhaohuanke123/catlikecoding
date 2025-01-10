/// <summary>
/// 游戏关卡对象基类
/// 子类可通过重写GameUpdate方法实现特定的游戏逻辑更新行为。
/// </summary>
public class GameLevelObject : PersistableObject
{
    #region 方法

    public virtual void GameUpdate()
    {
    }

    #endregion
}