using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Game : PersistableObject
{
    #region Unity 生命周期

    private void Awake()
    {
        m_objects = new List<PersistableObject>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(m_createKey))
        {
            CreateObject();
        }
        else if (Input.GetKeyDown(m_newGameKey))
        {
            BeginNewGame();
        }
        else if (Input.GetKeyDown(m_saveKey))
        {
            m_storage.Save(this);
        }
        else if (Input.GetKeyDown(m_loadKey))
        {
            BeginNewGame();
            m_storage.Load(this);
        }
    }

    #endregion

    #region 方法

    /// <summary>
    /// 创建一个物体
    /// </summary>
    private void CreateObject()
    {
        PersistableObject o = Instantiate(m_prefab);
        Transform t = o.transform;
        t.localPosition = Random.insideUnitSphere * 5f;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);

        m_objects.Add(o);
    }

    /// <summary>
    ///  开始新游戏，销毁当前所有物体并清空物体列表。
    /// </summary>
    private void BeginNewGame()
    {
        // 销毁所有已存在的物体
        for (int i = 0; i < m_objects.Count; i++)
        {
            Destroy(m_objects[i].gameObject);
        }

        m_objects.Clear();
    }

    /// <summary>
    /// 将所有物体的状态保存到GameDataWriter中。
    /// </summary>
    /// <param name="writer">用于写入数据的GameDataWriter实例。</param>
    public override void Save(GameDataWriter writer)
    {
        // 写入物体数量和每个物体的状态
        writer.Write(m_objects.Count);
        for (int i = 0; i < m_objects.Count; i++)
        {
            m_objects[i].Save(writer);
        }
    }

    /// <summary>
    /// 从GameDataReader中加载物体的状态。
    /// </summary>
    /// <param name="reader">用于读取数据的GameDataReader实例。</param>
    public override void Load(GameDataReader reader)
    {
        // 1. 读取物体数量
        int count = reader.ReadInt();
        for (int i = 0; i < count; i++)
        {
            // 2. 实例化新物体并加载其状态
            PersistableObject o = Instantiate(m_prefab);
            o.Load(reader);
            m_objects.Add(o);
        }
    }

    #endregion

    #region 字段

    #region KeyCode

    /// <summary>
    /// 用于生成物体的按键（默认为C）。
    /// </summary>
    public KeyCode m_createKey = KeyCode.C;

    /// <summary>
    /// 用于开始新游戏的按键（默认为N）。
    /// </summary>
    public KeyCode m_newGameKey = KeyCode.N;

    /// <summary>
    /// 用于保存游戏状态的按键（默认为S）。
    /// </summary>
    public KeyCode m_saveKey = KeyCode.S;

    /// <summary>
    /// 用于加载保存游戏状态的按键（默认为L）。
    /// </summary>
    public KeyCode m_loadKey = KeyCode.L;

    #endregion

    /// <summary>
    /// 物体的预制件，用于创建新的PersistableObject实例。
    /// </summary>
    public PersistableObject m_prefab;

    /// <summary>
    /// 存储当前游戏的物体列表。
    /// </summary>
    private List<PersistableObject> m_objects;

    /// <summary>
    /// 游戏存储实例，负责游戏数据的保存和加载。
    /// </summary>
    public PersistentStorage m_storage;

    #endregion
}