using System.IO;
using UnityEngine;

/// <summary>
///  用于包装并读取游戏数据的BinaryReader装饰器类。该类提供多种读取方法，可以从流中读取不同的数据类型。
/// </summary>
public class GameDataReader
{
    #region 构造器

    /// <summary>
    /// 初始化GameDataReader并保存传入的BinaryReader实例。
    /// </summary>
    /// <param name="reader">需要包装的BinaryReader实例。</param>
    /// <param name="version"> 读取的数据版本。 </param>
    public GameDataReader(BinaryReader reader, int version)
    {
        this.m_reader = reader;
        this.Version = version;
    }

    #endregion


    #region 方法

    /// <summary>
    /// 从BinaryReader中读取一个float值。
    /// </summary>
    /// <returns>读取的float值。</returns>
    public float ReadFloat()
    {
        return m_reader.ReadSingle();
    }

    /// <summary>
    /// 从BinaryReader中读取一个int值。
    /// </summary>
    /// <returns>读取的int值。</returns>
    public int ReadInt()
    {
        return m_reader.ReadInt32();
    }

    /// <summary>
    /// 从BinaryReader中读取一个Quaternion值。Quaternion由四个float值组成。
    /// </summary>
    /// <returns>读取的Quaternion值。</returns>
    public Quaternion ReadQuaternion()
    {
        Quaternion value;
        value.x = m_reader.ReadSingle();
        value.y = m_reader.ReadSingle();
        value.z = m_reader.ReadSingle();
        value.w = m_reader.ReadSingle();
        return value;
    }

    /// <summary>
    /// 从BinaryReader中读取一个Vector3值。Vector3由三个float值组成。
    /// </summary>
    /// <returns>读取的Vector3值。</returns>
    public Vector3 ReadVector3()
    {
        Vector3 value;
        value.x = m_reader.ReadSingle();
        value.y = m_reader.ReadSingle();
        value.z = m_reader.ReadSingle();
        return value;
    }

    /// <summary>
    /// 从BinaryReader中读取一个Color值。Color由四个float值组成。
    /// </summary>
    /// <returns>读取的Color值。</returns>
    public Color ReadColor()
    {
        Color value;
        value.r = m_reader.ReadSingle();
        value.g = m_reader.ReadSingle();
        value.b = m_reader.ReadSingle();
        value.a = m_reader.ReadSingle();
        return value;
    }

    public Random.State ReadRandomState()
    {
        return JsonUtility.FromJson<Random.State>(m_reader.ReadString());
    }

    #endregion

    #region 属性

    /// <summary>
    ///  读取的数据版本。
    /// </summary>
    public int Version { get; }

    #endregion

    #region 字段

    /// <summary>
    /// 用于从流中读取数据的BinaryReader实例。
    /// </summary>
    private BinaryReader m_reader;

    #endregion
}