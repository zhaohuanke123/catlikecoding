using UnityEngine;

/// <summary>
///  代表一个可持久化的游戏对象，它能够保存和加载自己的变换（位置、旋转和缩放）, 以及颜色信息，材质信息。
/// </summary>
public class Shape : PersistableObject
{
    #region Unity 生命周期

    private void Awake()
    {
        m_colors = new Color[m_meshRenderers.Length];
    }


    public void GameUpdate()
    {
        transform.Rotate(AngularVelocity * Time.deltaTime);
        transform.localPosition += Velocity * Time.deltaTime;
    }

    #endregion

    #region 方法

    /// <summary>
    /// 设置物体的材质及其材质ID。
    /// </summary>
    /// <param name="material">需要设置的材质。</param>
    /// <param name="materialId">该材质的ID。</param>
    public void SetMaterial(Material material, int materialId)
    {
        for (int i = 0; i < m_meshRenderers.Length; i++)
        {
            m_meshRenderers[i].material = material;
        }

        MaterialId = materialId;
    }

    /// <summary>
    /// 设置是有物体的颜色。使用`MaterialPropertyBlock`来避免每次修改材质时都创建新的材质实例。
    /// </summary>
    /// <param name="color">需要设置的颜色。</param>
    public void SetColor(Color color)
    {
        // m_color = color;
        // 设置材质颜色的缺点是这会导致创建新的材质，该材质对于形状是唯一的。每次设置其颜色时都会发生这种情况。
        // meshRenderer.material.color = color;

        s_sharedPropertyBlock ??= new MaterialPropertyBlock();

        s_sharedPropertyBlock.SetColor(ColorPropertyId, color);
        for (int i = 0; i < m_meshRenderers.Length; i++)
        {
            m_colors[i] = color;
            m_meshRenderers[i].SetPropertyBlock(s_sharedPropertyBlock);
        }
    }

    /// <summary>
    /// 设置子物体的颜色。
    /// </summary>
    /// <param name="color">需要设置的颜色。</param>
    /// <param name="index"> 子物体的索引 </param>
    public void SetColor(Color color, int index)
    {
        s_sharedPropertyBlock ??= new MaterialPropertyBlock();

        s_sharedPropertyBlock.SetColor(ColorPropertyId, color);
        m_colors[index] = color;
        m_meshRenderers[index].SetPropertyBlock(s_sharedPropertyBlock);
    }

    /// <summary>
    /// 保存Shape对象的状态，包括颜色信息。
    /// </summary>
    /// <param name="writer">用于写入数据的GameDataWriter。</param>
    public override void Save(GameDataWriter writer)
    {
        base.Save(writer);
        writer.Write(m_colors.Length);
        for (int i = 0; i < m_colors.Length; i++)
        {
            writer.Write(m_colors[i]);
        }

        writer.Write(AngularVelocity);
        writer.Write(Velocity);
    }

    /// <summary>
    /// 加载Shape对象的状态，并根据版本判断是否加载颜色数据。
    /// </summary>
    /// <param name="reader">用于读取数据的GameDataReader。</param>
    public override void Load(GameDataReader reader)
    {
        base.Load(reader);
        if (reader.Version >= 5)
        {
            LoadColors(reader);
        }
        else
        {
            SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
        }

        AngularVelocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
        Velocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
    }

    /// <summary>
    ///  加载颜色信息, 考虑版本差异 (代码版本和资源版本)
    /// </summary>
    /// <param name="reader"></param>
    private void LoadColors(GameDataReader reader)
    {
        // 1. 首先读取已保存的数量，这可能与我们当前预期颜色的数量不匹配。
        int count = reader.ReadInt();
        // 2. 我们可以安全读取和设置的颜色的最大数量等于已加载或当前计数中的较小值。
        int max = count <= m_colors.Length ? count : m_colors.Length;

        // 3. 读取颜色并将其设置为形状的颜色。
        int i = 0;
        for (; i < max; i++)
        {
            SetColor(reader.ReadColor(), i);
        }

        // 4. 如果已保存的颜色数量大于我们当前的颜色数量，则继续读取颜色，但不设置它们。
        if (count > m_colors.Length)
        {
            for (; i < count; i++)
            {
                reader.ReadColor();
            }
        }
        else if (count < m_colors.Length)
        {
            for (; i < m_colors.Length; i++)
            {
                SetColor(Color.white, i);
            }
        }
    }

    public void Recycle()
    {
        OriginFactory.Reclaim(this);
    }

    #endregion

    #region 属性

    /// <summary>
    /// 获取或设置形状的ID。形状的ID一旦设置为有效值后，不能再更改。
    /// </summary>
    public int ShapeId
    {
        get => m_shapeId;
        set
        {
            if (m_shapeId == int.MinValue && value != int.MinValue)
            {
                m_shapeId = value;
            }
            else
            {
                Debug.LogError("Not allowed to change shapeId.");
            }
        }
    }

    /// <summary>
    /// 获取物体的材质ID
    /// </summary>
    public int MaterialId { get; private set; }

    /// <summary>
    ///  物体的旋转速度 大小和方向
    /// </summary>
    public Vector3 AngularVelocity { get; set; }

    /// <summary>
    ///  物体的速度
    /// </summary>
    public Vector3 Velocity { get; set; }

    /// <summary>
    ///  shape的颜色数量
    /// </summary>
    public int ColorCount => m_colors.Length;

    /// <summary>
    /// 获取和设置原始工厂实例。
    /// </summary>
    public ShapeFactory OriginFactory
    {
        get => m_originFactory;
        set
        {
            if (m_originFactory == null)
            {
                m_originFactory = value;
            }
            else
            {
                Debug.LogError("Not allowed to change origin factory.");
            }
        }
    }

    #endregion

    #region 字段

    /// <summary>
    /// 形状的ID, 标识形状的类型
    /// </summary>
    private int m_shapeId = int.MinValue;

    /// <summary>
    ///  形状的颜色
    /// </summary>
    private Color m_color;

    /// <summary>
    /// 存储MeshRenderer组件的引用，用于修改形状的材质和颜色。
    /// </summary>
    private MeshRenderer m_meshRenderer;

    /// <summary>
    /// 可配置的Renderer数组，用于复合Shape
    /// </summary>
    
    [SerializeField]
    private MeshRenderer[] m_meshRenderers;

    /// <summary>
    ///  用于存储每个Renderer的颜色
    /// </summary>
    private Color[] m_colors;

    /// <summary>
    /// 用于优化材质属性设置的共享MaterialPropertyBlock实例。
    /// </summary>
    private static MaterialPropertyBlock s_sharedPropertyBlock;

    /// <summary>
    /// 用于设置材质的颜色属性的ID（缓存优化）。
    /// </summary>
    private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

    /// <summary>
    ///  用于存储Shape的原始工厂实例。实现Shape必须由创建它们的工厂回收
    /// </summary>
    private ShapeFactory m_originFactory;

    #endregion
}