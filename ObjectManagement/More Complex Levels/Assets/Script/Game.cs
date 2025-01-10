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
        m_killList = new List<ShapeInstance>();
        m_markAsDyingList = new List<ShapeInstance>();

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


    private void OnEnable()
    {
        Instance = this;
        if (m_shapeFactories[0].FactoryId != 0)
        {
            for (int i = 0; i < m_shapeFactories.Length; i++)
            {
                m_shapeFactories[i].FactoryId = i;
            }
        }
    }

    private void Update()
    {
        // 1. 检查按键输入
        if (Input.GetKeyDown(m_createKey))
        {
            GameLevel.Current.SpawnShapes();
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
        m_inGameUpdateLoop = true;
        // 1. for循环中驱动shape的Update
        for (int i = 0; i < m_shapes.Count; i++)
        {
            m_shapes[i].GameUpdate();
        }

        GameLevel.Current.GameUpdate();
        m_inGameUpdateLoop = false;

        // 2. 更新生成和销毁物体的进度
        m_creationProgress += Time.deltaTime * CreationSpeed;
        while (m_creationProgress >= 1f)
        {
            m_creationProgress -= 1f;
            GameLevel.Current.SpawnShapes();
        }

        m_destructionProgress += Time.deltaTime * DestructionSpeed;
        while (m_destructionProgress >= 1f)
        {
            m_destructionProgress -= 1f;
            DestroyShape();
        }

        // 3. 超出数量限制时，销毁多余的物体
        int limit = GameLevel.Current.PopulationLimit;
        if (limit > 0)
        {
            while (m_shapes.Count - m_dyingShapeCount > limit)
            {
                DestroyShape();
            }
        }

        // 4. 处理待销毁列表
        if (m_killList.Count > 0)
        {
            for (int i = 0; i < m_killList.Count; i++)
            {
                if (m_killList[i].IsValid)
                {
                    KillImmediately(m_killList[i].Shape);
                }
            }

            m_killList.Clear();
        }

        if (m_markAsDyingList.Count > 0)
        {
            for (int i = 0; i < m_markAsDyingList.Count; i++)
            {
                if (m_markAsDyingList[i].IsValid)
                {
                    MarkAsDyingImmediately(m_markAsDyingList[i].Shape);
                }
            }

            m_markAsDyingList.Clear();
        }
    }

    #endregion

    #region 方法

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
            m_shapes[i].Recycle();
        }

        m_shapes.Clear();
        m_dyingShapeCount = 0;
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
            writer.Write(m_shapes[i].OriginFactory.FactoryId);
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

    /// <summary>
    ///  从GameDataReader中加载物体的状态. 协程方式加载
    /// </summary>
    /// <param name="reader">用于读取数据的GameDataReader实例。</param>
    /// <returns> </returns>
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
            int factoryId = version >= 5 ? reader.ReadInt() : 0;
            int shapeId = version > 0 ? reader.ReadInt() : 0;
            int materialId = version > 0 ? reader.ReadInt() : 0;
            Shape instance = m_shapeFactories[factoryId].Get(shapeId, materialId);
            instance.Load(reader);
        }

        // 5. 读取完毕后，我们需要解决所有shape实例的引用。
        for (int i = 0; i < m_shapes.Count; i++)
        {
            m_shapes[i].ResolveShapeInstances();
        }
    }

    /// <summary>
    /// 销毁场景中的一个shape对象。
    /// 选择待销毁的shape时会排除正在消亡的shape。若无符合条件的shape，则不执行任何操作。
    /// 若销毁持续时间（destroyDuration）非正，则立即销毁shape；否则，将添加一个渐消失行为（DyingShapeBehavior）到shape上。
    /// </summary>
    private void DestroyShape()
    {
        if (m_shapes.Count - m_dyingShapeCount > 0)
        {
            Shape shape = m_shapes[Random.Range(m_dyingShapeCount, m_shapes.Count)];
            if (m_destroyDuration <= 0)
            {
                KillImmediately(shape);
            }
            else
            {
                shape.AddBehavior<DyingShapeBehavior>().Initialize(shape, m_destroyDuration);
            }
        }
    }

    /// <summary>
    ///  加载关卡, 协程方式加载
    /// </summary>
    /// <param name="levelBuildIndex"> 加载的关卡索引  </param>
    /// <returns></returns>
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

    /// <summary>
    ///  添加一个物体到物体列表中
    /// </summary>
    /// <param name="shape">  要添加的物体 </param>
    public void AddShape(Shape shape)
    {
        shape.SaveIndex = m_shapes.Count;
        m_shapes.Add(shape);
    }

    /// <summary>
    ///  从物体列表中获取一个物体
    /// </summary>
    /// <param name="index"> 物体索引 </param>
    /// <returns> 物体实例 </returns>
    public Shape GetShape(int index)
    {
        return m_shapes[index];
    }

    /// <summary>
    /// 移除指定shape对象。根据当前游戏状态决定立即销毁或加入待销毁列表。
    /// </summary>
    /// <param name="shape">待移除的shape实例。</param>
    public void Kill(Shape shape)
    {
        if (m_inGameUpdateLoop)
        {
            m_killList.Add(shape);
        }
        else
        {
            KillImmediately(shape);
        }
    }

    /// <summary>
    /// 立即销毁shape对象，不经过常规的游戏循环处理。
    /// 此方法直接从游戏世界中移除shape，并更新shape列表，以保持索引的连续性。
    /// </summary>
    /// <param name="shape">待立即销毁的shape实例。</param>
    private void KillImmediately(Shape shape)
    {
        // 1. 获取shape的索引
        int index = shape.SaveIndex;
        shape.Recycle();

        // 2. 如果shape不是待销魂部分中的最后一个，将其移动到那部分的末尾
        if (index < m_dyingShapeCount && index < --m_dyingShapeCount)
        {
            m_shapes[m_dyingShapeCount].SaveIndex = index;
            m_shapes[index] = m_shapes[m_dyingShapeCount];
            index = m_dyingShapeCount;
        }

        // 3. 将其移动到list的末尾
        int lastIndex = m_shapes.Count - 1;
        m_shapes[lastIndex].SaveIndex = index;
        m_shapes[index] = m_shapes[lastIndex];
        m_shapes.RemoveAt(lastIndex);
    }

    /// <summary>
    /// 立即将shape标记为即将销毁状态，调整shape在列表中的位置以准备回收。
    /// </summary>
    /// <param name="shape">待标记的shape实例。</param>
    private void MarkAsDyingImmediately(Shape shape)
    {
        // 1. 如果shape已经在即将销毁的部分中，直接返回
        int index = shape.SaveIndex;
        if (index < m_dyingShapeCount)
        {
            return;
        }

        // 2. 将shape移动到即将销毁的部分中
        m_shapes[m_dyingShapeCount].SaveIndex = index;
        m_shapes[index] = m_shapes[m_dyingShapeCount];
        shape.SaveIndex = m_dyingShapeCount;
        m_shapes[m_dyingShapeCount++] = shape;
    }

    /// <summary>
    /// 将shape标记为即将销毁。如果当前处于游戏更新循环中，
    /// 则将shape添加到待销毁列表以稍后处理；否则立即调用 <see cref="MarkAsDyingImmediately(Shape)"/> 处理。
    /// </summary>
    /// <param name="shape">需要被标记为即将销毁的shape实例。</param>
    public void MarkAsDying(Shape shape)
    {
        if (m_inGameUpdateLoop)
        {
            m_markAsDyingList.Add(shape);
        }
        else
        {
            MarkAsDyingImmediately(shape);
        }
    }

    /// <summary>
    /// 判断shape是否已被标记为即将消亡。
    /// </summary>
    /// <param name="shape">待检查的shape实例。</param>
    /// <returns>如果shape已被标记为消亡，则返回true；否则返回false。</returns>
    public bool IsMarkedAsDying(Shape shape)
    {
        return shape.SaveIndex < m_dyingShapeCount;
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

    /// <summary>
    ///  获取Game单例
    /// </summary>
    public static Game Instance { get; private set; }

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
    /// 存储当前游戏的物体列表。
    /// </summary>
    private List<Shape> m_shapes;

    /// <summary>
    /// 存储即将或已被销毁的游戏shape对象的列表。
    /// </summary>
    private List<ShapeInstance> m_killList;

    /// <summary>
    /// 用于标记即将销毁的shape实例列表。
    /// 当shape被标记为待销毁时，它们将被添加到此列表中，并在合适的时机进行处理。
    /// </summary>
    private List<ShapeInstance> m_markAsDyingList;

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
    private const int SaveVersion = 7;

    /// <summary>
    /// 生成物体的进度, 当该值达到 1 时，应该创建一个新的shape
    /// </summary>
    private float m_creationProgress;

    /// <summary>
    ///  销毁物体的进度, 当该值达到 1 时，应该销毁一个shape
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

    /// <summary>
    /// shape工厂数组
    /// </summary>
    [SerializeField]
    private ShapeFactory[] m_shapeFactories;

    /// <summary>
    /// 表示游戏进行中的更新循环标志。
    /// </summary>
    private bool m_inGameUpdateLoop;

    /// <summary>
    /// 当前正在消亡的shape数量。 m_dyingShapeCount 的值所在的下标为非消亡shape部分的第一个。
    /// </summary>
    private int m_dyingShapeCount;

    /// <summary>
    /// 物体销毁持续时间
    /// </summary>
    [SerializeField]
    private float m_destroyDuration;

    #endregion
}