using System;
using UnityEngine;

public class GameLevel : PersistableObject
{
    #region Unity 生命周期

    private void OnEnable()
    {
        Current = this;
        m_persistentObjects ??= Array.Empty<PersistableObject>();
    }

    #endregion

    #region 方法

    public override void Save(GameDataWriter writer)
    {
        writer.Write(m_persistentObjects.Length);
        for (int i = 0; i < m_persistentObjects.Length; i++)
        {
            m_persistentObjects[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        int savedCount = reader.ReadInt();
        for (int i = 0; i < savedCount; i++)
        {
            m_persistentObjects[i].Load(reader);
        }
    }

    /// <summary>
    /// 生成一个形状, 使用SpawnZone生成 
    /// </summary>
    public Shape SpawnShape()
    {
        return m_spawnZone.SpawnShape();
    }

    #endregion

    #region 属性

    /// <summary>
    ///   当前关卡
    /// </summary>
    public static GameLevel Current { get; private set; }

    #endregion

    #region 字段

    /// <summary>
    ///  生成区域
    /// </summary>
    [SerializeField]
    private SpawnZone m_spawnZone;

    /// <summary>
    ///  用于保存关卡时哪些对象应该被持久化
    /// </summary>
    [SerializeField]
    private PersistableObject[] m_persistentObjects;

    #endregion
}