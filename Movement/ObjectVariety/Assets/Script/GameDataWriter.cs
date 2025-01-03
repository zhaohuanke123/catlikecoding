using System.IO;
using UnityEngine;


/// <summary>
/// 用于包装并写入游戏数据的BinaryWriter装饰器类。该类提供多种写入方法，可以将不同的数据类型写入到流中。
/// </summary>
public class GameDataWriter
{
    #region 构造器

    /// <summary>
    ///初始化GameDataWriter并保存传入的BinaryWriter实例。
    /// </summary>
    /// <param name="writer">需要包装的BinaryWriter实例。</param>
    public GameDataWriter(BinaryWriter writer)
    {
        this.m_writer = writer;
    }

    #endregion

    #region 方法

    /// <summary>
    /// 将一个float类型的值写入到BinaryWriter中。
    /// </summary>
    /// <param name="value">需要写入的float值。</param>
    public void Write(float value)
    {
        m_writer.Write(value);
    }


    /// <summary>
    /// 将一个int类型的值写入到BinaryWriter中。
    /// </summary>
    /// <param name="value">需要写入的int值。</param>
    public void Write(int value)
    {
        m_writer.Write(value);
    }

    /// <summary>
    /// 将一个Quaternion类型的值写入到BinaryWriter中。Quaternion由四个float值组成。
    /// </summary>
    /// <param name="value">需要写入的Quaternion值。</param>
    public void Write(Quaternion value)
    {
        m_writer.Write(value.x);
        m_writer.Write(value.y);
        m_writer.Write(value.z);
        m_writer.Write(value.w);
    }

    /// <summary>
    /// 将一个Vector3类型的值写入到BinaryWriter中。Vector3由三个float值组成。
    /// </summary>
    /// <param name="value">需要写入的Vector3值。</param>
    public void Write(Vector3 value)
    {
        m_writer.Write(value.x);
        m_writer.Write(value.y);
        m_writer.Write(value.z);
    }

    /// <summary>
    /// 将一个Color类型的值写入到BinaryWriter中。Color由四个float值组成。
    /// </summary>
    /// <param name="value">需要写入的Color值。</param>
    public void Write(Color value)
    {
        m_writer.Write(value.r);
        m_writer.Write(value.g);
        m_writer.Write(value.b);
        m_writer.Write(value.a);
    }

    #endregion

    #region 字段

    /// <summary>
    /// 用于将数据写入到流中的BinaryWriter实例。
    /// </summary>
    private BinaryWriter m_writer;

    #endregion
}