using System.IO;
using UnityEngine;

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
    public void Save(PersistableObject o)
    {
        using (var writer = new BinaryWriter(File.Open(m_savePath, FileMode.Create)))
        {
            o.Save(new GameDataWriter(writer));
        }
    }

    public void Load(PersistableObject o)
    {
        using (var reader = new BinaryReader(File.Open(m_savePath, FileMode.Open)))
        {
            o.Load(new GameDataReader(reader));
        }
    }

    #endregion

    #region 字段

    /// <summary>
    ///  保存游戏状态的路径
    /// </summary>
    private string m_savePath;

    #endregion
}