using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
///  代表游戏的主要类，负责生成物体、保存和加载游戏状态。
/// </summary>
public class Game : PersistableObject
{
    #region Unity 生命周期

    private void Start()
    {
        // 1. 做初始化 
        m_mainRandomState = Random.state;
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

        BeginNewGame();
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
            StartCoroutine(LoadLevel(m_loadedLevelBuildIndex));
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
                    StartCoroutine(LoadLevel(i));
                    return;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        // 1. for循环中驱动shape的Update
        for (int i = 0; i < m_shapes.Count; i++)
        {
            m_shapes[i].GameUpdate();
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
        // 1. 获取一个随机shape物体
        Shape instance = m_shapeFactory.GetRandom();
        
        // 2. 配置物体的状态
        GameLevel.Current.ConfigureSpawn(instance);

        // 3. 将物体添加到物体列表中
        m_shapes.Add(instance);
    }

    /// <summary>
    ///  开始新游戏， 重置游戏状态
    /// </summary>
    private void BeginNewGame()
    {
        // 1. 重置随机数状态
        Random.state = m_mainRandomState;
        int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
        m_mainRandomState = Random.state;
        Random.InitState(seed);

        // 2. 重置游戏状态 和更新UI
        m_creationSpeedSlider.value = CreationSpeed = 0;
        m_destructionSpeedSlider.value = DestructionSpeed = 0;

        // 3. 销毁所有已存在的物体
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

        // 写入物体数量和每个物体的状态, 保存随机数状态，保存当前关卡
        writer.Write(m_shapes.Count);
        writer.Write(Random.state);
        writer.Write(CreationSpeed);
        writer.Write(m_creationProgress);
        writer.Write(DestructionSpeed);
        writer.Write(m_destructionProgress);
        writer.Write(m_loadedLevelBuildIndex);
        GameLevel.Current.Save(writer);
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
        int version = reader.Version;
        if (version > SaveVersion)
        {
            Debug.LogError("Unsupported future save version " + version);
            return;
        }

        StartCoroutine(LoadGame(reader));
    }

    private IEnumerator LoadGame(GameDataReader reader)
    {
        //  1. 读取存档版本
        int version = reader.Version;
        // 2. 读取物体数量, 切换保存的场景, 随机数状态 (考虑版本差异)
        int count = version <= 0 ? -version : reader.ReadInt();
        if (version >= 3)
        {
            // Random.state = reader.ReadRandomState();
            Random.State state = reader.ReadRandomState();
            if (!m_reseedOnLoad)
            {
                Random.state = state;
            }

            m_creationSpeedSlider.value = CreationSpeed = reader.ReadFloat();
            m_creationProgress = reader.ReadFloat();
            m_destructionSpeedSlider.value = DestructionSpeed = reader.ReadFloat();
            m_destructionProgress = reader.ReadFloat();
        }

        // 3. 加载时，我们必须在读取关卡构建索引后读取关卡数据。
        // 但是，只有在关卡场景加载后才能这样做，否则我们将把它应用到即将卸载的关卡场景中。
        // 因此，我们必须推迟读取保存文件的其余部分，直到 LoadLevel 协程完成。
        yield return LoadLevel(version < 2 ? DefaultLevelIndex : reader.ReadInt());
        if (version >= 3)
        {
            GameLevel.Current.Load(reader);
        }

        // 4. 读取每个物体的状态
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
        // 1. 防止场景加载中调用Update
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
    [SerializeField]
    private ShapeFactory m_shapeFactory;

    /// <summary>
    /// 存储当前游戏的物体列表。
    /// </summary>
    private List<Shape> m_shapes;

    /// <summary>
    /// 用于保存和加载随机数状态的Random.State实例。
    /// </summary>
    private Random.State m_mainRandomState;

    /// <summary>
    ///  是否在加载时重新生成随机数种子
    /// </summary>
    [SerializeField]
    private bool m_reseedOnLoad;

    /// <summary>
    /// 游戏存储实例，负责游戏数据的保存和加载。
    /// </summary>
    public PersistentStorage m_storage;

    /// <summary>
    ///  游戏数据文件版本
    /// </summary>
    private const int SaveVersion = 4;

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

    /// <summary>
    ///  创建速度滑块UI
    /// </summary>
    [SerializeField]
    private Slider m_creationSpeedSlider;

    /// <summary>
    ///  销毁速度滑块UI
    /// </summary>
    [SerializeField]
    private Slider m_destructionSpeedSlider;

    #endregion
}