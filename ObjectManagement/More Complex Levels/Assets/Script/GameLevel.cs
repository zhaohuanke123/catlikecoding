using System;
using UnityEngine;

public partial class GameLevel : PersistableObject
{
    #region Unity 生命周期

    private void OnEnable()
    {
        Current = this;
        levelObjects ??= Array.Empty<GameLevelObject>();
    }

    #endregion

    #region 方法

    public void GameUpdate()
    {
        for (int i = 0; i < levelObjects.Length; i++)
        {
            levelObjects[i].GameUpdate();
        }
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(levelObjects.Length);
        for (int i = 0; i < levelObjects.Length; i++)
        {
            levelObjects[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        int savedCount = reader.ReadInt();
        for (int i = 0; i < savedCount; i++)
        {
            levelObjects[i].Load(reader);
        }
    }

    /// <summary>
    /// 生成一个shape, 使用SpawnZone生成 
    /// </summary>
    public void SpawnShapes()
    {
        m_spawnZone.SpawnShapes();
    }

    #endregion

    #region 属性

    /// <summary>
    ///   当前关卡
    /// </summary>
    public static GameLevel Current { get; private set; }

    /// <summary>
    ///   获取shape 数量限制
    /// </summary>
    public int PopulationLimit => m_populationLimit;

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
    private GameLevelObject[] levelObjects;

    /// <summary>
    ///  生成的数量限制
    /// </summary>
    [SerializeField]
    private int m_populationLimit;

    #endregion
}