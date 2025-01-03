using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
///  代表游戏的主要类，负责生成物体、保存和加载游戏状态。
/// </summary>
public class Game : PersistableObject
{
    #region Unity 生命周期

    private void Awake()
    {
        m_shapes = new List<Shape>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(m_createKey))
        {
            CreateShape();
        }
        else if (Input.GetKeyDown(m_newGameKey))
        {
            BeginNewGame();
        }
        else if (Input.GetKeyDown(m_saveKey))
        {
            m_storage.Save(this, SaveVersion);
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
    private void CreateShape()
    {
        // 1. 获取一个随机形状
        Shape instance = m_shapeFactory.GetRandom();

        // 2. 随机设置物体的位置、旋转和缩放
        Transform t = instance.transform;
        t.localPosition = Random.insideUnitSphere * 5f;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);

        // 3. 随机设置物体的颜色
        instance.SetColor(Random.ColorHSV(
            hueMin: 0f, hueMax: 1f,
            saturationMin: 0.5f, saturationMax: 1f,
            valueMin: 0.25f, valueMax: 1f,
            alphaMin: 1f, alphaMax: 1f
        ));

        // 4. 将物体添加到物体列表中
        m_shapes.Add(instance);
    }

    /// <summary>
    ///  开始新游戏，销毁当前所有物体并清空物体列表。
    /// </summary>
    private void BeginNewGame()
    {
        // 销毁所有已存在的物体
        for (int i = 0; i < m_shapes.Count; i++)
        {
            Destroy(m_shapes[i].gameObject);
        }

        m_shapes.Clear();
    }

    /// <summary>
    /// 将所有物体的状态保存到GameDataWriter中。
    /// </summary>
    /// <param name="writer">用于写入数据的GameDataWriter实例。</param>
    public override void Save(GameDataWriter writer)
    {
        // 不需要写入存档版本，因为PersistentStorage会在写入数据时写入版本号。
        // // 通过在写入存档版本时不直接写入版本号来实现区分存档版本和对象计数。
        // writer.Write(-saveVersion);

        // 写入物体数量和每个物体的状态
        writer.Write(m_shapes.Count);
        for (int i = 0; i < m_shapes.Count; i++)
        {
            writer.Write(m_shapes[i].ShapeId);
            writer.Write(m_shapes[i].MaterialId);
            m_shapes[i].Save(writer);
        }
    }

    /// <summary>
    /// 从GameDataReader中加载物体的状态。
    /// </summary>
    /// <param name="reader">用于读取数据的GameDataReader实例。</param>
    public override void Load(GameDataReader reader)
    {
        //  1. 读取存档版本
        int version = reader.Version;
        if (version > SaveVersion)
        {
            Debug.LogError("Unsupported future save version " + version);
            return;
        }

        // 2. 读取物体数量 (考虑版本差异)
        int count = version <= 0 ? -version : reader.ReadInt();

        // 3. 读取每个物体的状态
        for (int i = 0; i < count; i++)
        {
            // 实例化新物体并加载其状态
            int shapeId = version > 0 ? reader.ReadInt() : 0;
            int materialId = version > 0 ? reader.ReadInt() : 0;
            Shape instance = m_shapeFactory.Get(shapeId, materialId);
            instance.Load(reader);
            m_shapes.Add(instance);
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

    public ShapeFactory m_shapeFactory;

    /// <summary>
    /// 存储当前游戏的物体列表。
    /// </summary>
    private List<Shape> m_shapes;

    /// <summary>
    /// 游戏存储实例，负责游戏数据的保存和加载。
    /// </summary>
    public PersistentStorage m_storage;

    /// <summary>
    ///  游戏数据文件版本
    /// </summary>
    private const int SaveVersion = 1;

    #endregion
}