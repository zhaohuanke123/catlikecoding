using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

/// <summary>
///  代表游戏的主要类，负责生成物体、保存和加载游戏状态。
/// </summary>
public class Game : PersistableObject
{
    #region Unity 生命周期

    // 在 Awake 中尝试太早了，但如果我们稍作延迟并改用 Start 方法，它就能工作。
    // private void Awake()
    private void Start()
    {
        // 1. 化用于存储当前场景中的所有 Shape 实例。
        m_shapes = new List<Shape>();

        // 2. 在编辑器模式下进行特殊场景检查
        if (Application.isEditor)
        {
            // 1. 遍历当前加载的所有场景，寻找名称包含 "Level " 的场景
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if (loadedScene.name.Contains(LevelPrefix))
                {
                    SceneManager.SetActiveScene(loadedScene);
                    m_loadedLevelBuildIndex = loadedScene.buildIndex;
                    return;
                }
            }
        }

        // 3. 如果不在编辑器模式或没有找到合适的场景，异步加载默认关卡 1
        StartCoroutine(LoadLevel(DefaultLevelIndex));
    }

    private void Update()
    {
        // 1. 检查按键输入
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
        else if (Input.GetKeyDown(m_destroyKey))
        {
            DestroyShape();
        }
        else if (Input.GetKeyDown(m_loadKey))
        {
            BeginNewGame();
            m_storage.Load(this);
        }
        else
        {
            // 检测数字键，加载对应关卡
            for (int i = 1; i <= m_levelCount; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    BeginNewGame();
                    StartCoroutine(LoadLevel(DefaultLevelIndex));
                    return;
                }
            }
        }

        // 2. 更新生成和销毁物体的进度
        m_creationProgress += Time.deltaTime * CreationSpeed;
        while (m_creationProgress >= 1f)
        {
            m_creationProgress -= 1f;
            CreateShape();
        }

        m_destructionProgress += Time.deltaTime * DestructionSpeed;
        while (m_destructionProgress >= 1f)
        {
            m_destructionProgress -= 1f;
            DestroyShape();
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
            m_shapeFactory.Reclaim(m_shapes[i]);
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
        writer.Write(m_loadedLevelBuildIndex);
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

        // 2. 读取物体数量和切换保存的场景 (考虑版本差异)
        int count = version <= 0 ? -version : reader.ReadInt();
        StartCoroutine(LoadLevel(version < 2 ? 1 : reader.ReadInt()));

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

    /// <summary>
    ///  如果物体列表中有物体，则销毁一个物体。
    /// </summary>
    private void DestroyShape()
    {
        if (m_shapes.Count > 0)
        {
            int index = Random.Range(0, m_shapes.Count);
            m_shapeFactory.Reclaim(m_shapes[index]);
            int lastIndex = m_shapes.Count - 1;
            m_shapes[index] = m_shapes[lastIndex];
            m_shapes.RemoveAt(lastIndex);
        }
    }

    private IEnumerator LoadLevel(int levelBuildIndex)
    {
        // 1. 防止场景加载中调用Uodate
        enabled = false;

        // 2. 卸载当前场景
        if (m_loadedLevelBuildIndex > 0)
        {
            yield return SceneManager.UnloadSceneAsync(m_loadedLevelBuildIndex);
        }

        // 3. 加载新场景
        yield return SceneManager.LoadSceneAsync(levelBuildIndex, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(levelBuildIndex));
        m_loadedLevelBuildIndex = levelBuildIndex;

        enabled = true;
    }

    #endregion

    #region 属性

    /// <summary>
    /// 创建速度
    /// </summary>
    public float CreationSpeed { get; set; } = 5;

    /// <summary>
    ///  销毁速度
    /// </summary>
    public float DestructionSpeed { get; set; } = 2;

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

    /// <summary>
    ///  用于销毁物体的按键（默认为X）。
    /// </summary>
    public KeyCode m_destroyKey = KeyCode.X;

    #endregion

    /// <summary>
    ///  形状工厂实例，用于生成形状。
    /// </summary>
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
    private const int SaveVersion = 2;

    /// <summary>
    /// 生成物体的进度, 当该值达到 1 时，应该创建一个新的形状
    /// </summary>
    private float m_creationProgress;

    /// <summary>
    ///  销毁物体的进度, 当该值达到 1 时，应该销毁一个形状
    /// </summary>
    private float m_destructionProgress;

    #region 关卡相关

    /// <summary>
    ///  关卡名称前缀常量
    /// </summary>
    private const string LevelPrefix = "Level ";

    /// <summary>
    ///  默认加载的关卡索引
    /// </summary>
    private const int DefaultLevelIndex = 1;

    /// <summary>
    ///  支持的关卡数量
    /// </summary>
    public int m_levelCount;

    /// <summary>
    ///  当前加载的关卡索引
    /// </summary>
    private int m_loadedLevelBuildIndex;

    #endregion

    #endregion
}