using UnityEngine;

/// <summary>
///  代表一个可持久化的游戏对象，它能够保存和加载自己的变换（位置、旋转和缩放）, 以及颜色信息，材质信息。
/// </summary>
public class Shape : PersistableObject
{
    #region Unity 生命周期

    private void Awake()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();
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
        m_meshRenderer.material = material;
        MaterialId = materialId;
    }

    /// <summary>
    /// 设置物体的颜色。使用`MaterialPropertyBlock`来避免每次修改材质时都创建新的材质实例。
    /// </summary>
    /// <param name="color">需要设置的颜色。</param>
    public void SetColor(Color color)
    {
        m_color = color;
        // 设置材质颜色的缺点是这会导致创建新的材质，该材质对于形状是唯一的。每次设置其颜色时都会发生这种情况。
        // meshRenderer.material.color = color;

        s_sharedPropertyBlock ??= new MaterialPropertyBlock();

        s_sharedPropertyBlock.SetColor(ColorPropertyId, color);
        m_meshRenderer.SetPropertyBlock(s_sharedPropertyBlock);
    }

    /// <summary>
    /// 保存Shape对象的状态，包括颜色信息。
    /// </summary>
    /// <param name="writer">用于写入数据的GameDataWriter。</param>
    public override void Save(GameDataWriter writer)
    {
        base.Save(writer);
        writer.Write(m_color);
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
        SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
        AngularVelocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
        Velocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
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
    /// 用于优化材质属性设置的共享MaterialPropertyBlock实例。
    /// </summary>
    private static MaterialPropertyBlock s_sharedPropertyBlock;

    /// <summary>
    /// 用于设置材质的颜色属性的ID（缓存优化）。
    /// </summary>
    private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

    #endregion
}