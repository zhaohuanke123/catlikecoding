using UnityEngine;

/// <summary>
///  生成区域 抽象类
/// </summary>
public abstract class SpawnZone : PersistableObject
{
    #region 属性

    /// <summary>
    /// 用于获取生成点
    /// </summary>
    public abstract Vector3 SpawnPoint { get; }

    #endregion
}