using System.IO;
using UnityEngine;

/// <summary>
///  用于保存游戏状态的类，可以保存和加载游戏对象的状态。
/// </summary>
public class PersistentStorage : MonoBehaviour
{
    #region Unity 生命周期

    private void Awake()
    {
        m_savePath = Path.Combine(Application.persistentDataPath, "saveFile");
    }

    #endregion

    #region 方法

    /// <summary>
    /// 保存游戏状态
    /// </summary>
    /// <param name="o">要保存的对象</param>
    /// <param name="version">  保存的数据版本。</param>
    public void Save(PersistableObject o, int version)
    {
        using (var writer = new BinaryWriter(File.Open(m_savePath, FileMode.Create)))
        {
            writer.Write(-version);
            o.Save(new GameDataWriter(writer));
        }
    }

    /// <summary>
    ///  加载对象状态
    ///  </summary>
    ///  <param name="o">要加载的对象</param>
    public void Load(PersistableObject o)
    {
        // 因为 Game的Load 变成异步的，所以 reader 不能使用 using 语句
        // using (var reader = new BinaryReader(File.Open(m_savePath, FileMode.Open)))
        // {
        //     o.Load(new GameDataReader(reader, -reader.ReadInt32()));
        // }

        byte[] data = File.ReadAllBytes(m_savePath);
        var reader = new BinaryReader(new MemoryStream(data));
        o.Load(new GameDataReader(reader, -reader.ReadInt32()));
    }

    #endregion

    #region 字段

    /// <summary>
    ///  保存游戏状态的路径
    /// </summary>
    private string m_savePath;

    #endregion
}