using UnityEngine;

public class GameLevel : MonoBehaviour
{
    #region Unity 生命周期

    private void Start()
    {
        // 将引用传递给 Game 单例 
        Game.Instance.SpawnZoneOfLevel = m_spawnZone;
    }

    #endregion

    #region 字段

    /// <summary>
    ///  生成区域
    /// </summary>
    [SerializeField]
    private SpawnZone m_spawnZone;

    #endregion
}