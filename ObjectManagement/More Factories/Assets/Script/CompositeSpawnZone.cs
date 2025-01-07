using UnityEngine;

/// <summary>
///  用于生成组合多个区域的点
/// </summary>
public class CompositeSpawnZone : SpawnZone
{
    #region 方法

    public override void Save(GameDataWriter writer)
    {
        writer.Write(m_nextSequentialIndex);
    }

    public override void Load(GameDataReader reader)
    {
        m_nextSequentialIndex = reader.ReadInt();
    }

    #endregion

    #region 属性

    public override Vector3 SpawnPoint
    {
        get
        {
            // 1. 顺序生成或随机生成
            int index;
            if (m_sequential)
            {
                index = m_nextSequentialIndex++;
                if (m_nextSequentialIndex >= m_spawnZones.Length)
                {
                    m_nextSequentialIndex = 0;
                }
            }
            else
            {
                index = Random.Range(0, m_spawnZones.Length);
            }

            // 2. 返回子区域的生成点
            return m_spawnZones[index].SpawnPoint;
        }
    }

    public override Shape SpawnShape()
    {
        if (m_overrideConfig)
        {
            return base.SpawnShape();
        }
        else
        {
            int index;
            if (m_sequential)
            {
                index = m_nextSequentialIndex++;
                if (m_nextSequentialIndex >= m_spawnZones.Length)
                {
                    m_nextSequentialIndex = 0;
                }
            }
            else
            {
                index = Random.Range(0, m_spawnZones.Length);
            }

            return m_spawnZones[index].SpawnShape();
        }
    }

    #endregion

    #region 字段

    /// <summary>
    ///  生成区域数组
    /// </summary>
    [SerializeField]
    private SpawnZone[] m_spawnZones;

    /// <summary>
    ///  下一个顺序生成的索引
    /// </summary>
    private int m_nextSequentialIndex;

    /// <summary>
    ///  是否在几个区域间顺序生成
    /// </summary>
    [SerializeField]
    private bool m_sequential;

    /// <summary>
    ///  是否覆盖子区域的生成配置
    /// </summary>
    [SerializeField]
    private bool m_overrideConfig;

    #endregion
}