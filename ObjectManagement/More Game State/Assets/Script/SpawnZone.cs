using UnityEngine;

public abstract class SpawnZone : MonoBehaviour
{
    #region 属性

    /// <summary>
    /// 生成点
    /// </summary>
    public abstract Vector3 SpawnPoint { get; }

    #endregion
}