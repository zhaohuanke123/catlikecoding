using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏对象工厂基类，用于创建和管理游戏对象实例。
/// </summary>
public class GameObjectFactory : ScriptableObject
{
    #region 方法

    /// <summary>
    /// 将指定的游戏对象移动到工厂场景中。
    /// 此方法用于管理对象池，将不再当前场景中使用的 GameTileContent
    /// 实例移动到一个专门的场景中，以便后续重用，减少资源消耗。
    /// </summary>
    /// <param name="prefab">需要移动到工厂场景的游戏对象。</param>
    protected T CreateGameObjectInstance<T>(T prefab) where T : MonoBehaviour
    {
        // 1. 创建专门的场景
        if (!m_scene.isLoaded)
        {
            if (Application.isEditor)
            {
                m_scene = SceneManager.GetSceneByName(name);
                if (!m_scene.isLoaded)
                {
                    m_scene = SceneManager.CreateScene(name);
                }
            }
            else
            {
                m_scene = SceneManager.CreateScene(name);
            }
        }

        // 2. 实例化游戏对象
        T instance = Instantiate(prefab);
        // 3. 移动到工厂场景
        SceneManager.MoveGameObjectToScene(instance.gameObject, m_scene);
        return instance;
    }

    #endregion

    #region 字段

    /// <summary>
    /// 当前工厂使用的场景，用于存储和管理游戏对象实例，以实现对象池的功能。
    /// 该场景通常在首次调用时创建或加载，用于隔离和复用那些不直接展示在当前活跃场景中的游戏对象。
    /// </summary>
    private Scene m_scene;

    #endregion
}