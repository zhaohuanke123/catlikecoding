using UnityEngine;


/// <summary>
/// 卫星shape行为类，用于模拟卫星围绕焦点的轨道运动。
/// </summary>
public class SatelliteShapeBehavior : ShapeBehavior
{
    #region 方法

    public override bool GameUpdate(Shape shape)
    {
        if (m_focalShape.IsValid)
        {
            //  计算卫星的位置
            float t = 2f * Mathf.PI * m_frequency * shape.Age;
            m_previousPosition = shape.transform.localPosition;
            shape.transform.localPosition =
                m_focalShape.Shape.transform.localPosition + m_cosOffset * Mathf.Cos(t) + m_sinOffset * Mathf.Sin(t);

            return true;
        }

        //  如果焦点Shape无效，则将卫星shape 添加移动行为, 并返回false让卫星行为回收
        shape.AddBehavior<MovementShapeBehavior>().Velocity =
            (shape.transform.localPosition - m_previousPosition) / Time.deltaTime;
        return false;
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(m_focalShape);
        writer.Write(m_frequency);
        writer.Write(m_cosOffset);
        writer.Write(m_sinOffset);
        writer.Write(m_previousPosition);
    }

    public override void Load(GameDataReader reader)
    {
        m_focalShape = reader.ReadShapeInstance();
        m_frequency = reader.ReadFloat();
        m_cosOffset = reader.ReadVector3();
        m_sinOffset = reader.ReadVector3();
        m_previousPosition = reader.ReadVector3();
    }

    public override void Recycle()
    {
        ShapeBehaviorPool<SatelliteShapeBehavior>.Reclaim(this);
    }

    public override void ResolveShapeInstances()
    {
        m_focalShape.Resolve();
    }

    /// <summary>
    /// 初始化卫星行为。
    /// 设置卫星围绕焦点shape旋转的轨道参数和初始位置。
    /// </summary>
    /// <param name="shape">卫星shape实例。</param>
    /// <param name="focalShape">焦点shape实例。</param>
    /// <param name="radius">卫星轨道半径。</param>
    public void Initialize(Shape shape, Shape focalShape, float radius, float frequency)
    {
        // 1. 保存卫星的相关信息 
        m_focalShape = focalShape;
        m_frequency = frequency;

        // 2. 生成一个随机的轨道轴
        Vector3 orbitAxis = Random.onUnitSphere;
        // 当向量最终太短而无法标准化时， Vector3.normalized 将返回零向量。
        // 我们可以通过检查偏移向量的平方大小是否小于 1 来检测到这一点。
        // 但由于数值精度，我们应该检查更小的值，所以让我们使用 0.1。它将非常接近 1 或正好为零。
        do
        {
            m_cosOffset = Vector3.Cross(orbitAxis, Random.onUnitSphere).normalized;
        } while (m_cosOffset.sqrMagnitude < 0.1f);

        m_sinOffset = Vector3.Cross(m_cosOffset, orbitAxis);
        m_cosOffset *= radius;
        m_sinOffset *= radius;

        // 3. 为卫星添加旋转行为
        // 它们的旋转与轨道匹配，因此它们总是以同一面朝向其焦点shape。
        // 使用 InverseTransformDirection 将轨道轴转换为卫星的本地空间。
        shape.AddBehavior<RotationShapeBehavior>().AngularVelocity =
            -360f * frequency * shape.transform.InverseTransformDirection(orbitAxis);

        // 3. 在 Initialize 结束时调用一次 GameUpdate 。这是必要的，因为在shape生成的同一帧中不会调用 GameUpdate 。
        GameUpdate(shape);

        // 4. 先前的位置向量是任意的，对于新的行为可能是零，或者仍然包含已回收卫星行为的值。此时卫星尚未移动，因此最初将先前位置设置为其当前位置，在 Initialize 的末尾。
        m_previousPosition = shape.transform.localPosition;
    }

    #endregion

    #region 属性

    public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Satellite;

    #endregion

    #region 字段

    /// <summary>
    /// 焦点shape实例，用于定义轨道中心。
    /// </summary>
    private ShapeInstance m_focalShape;

    /// <summary>
    /// 轨道旋转频率（每秒转动次数）。
    /// </summary>
    private float m_frequency;

    /// <summary>
    /// 用于计算轨道偏移的余弦方向向量。
    /// </summary>
    private Vector3 m_cosOffset;

    /// <summary>
    /// 用于计算轨道偏移的正弦方向向量。
    /// </summary>
    private Vector3 m_sinOffset;

    /// <summary>
    /// 上一帧的位置，用于计算速度。
    /// </summary>
    private Vector3 m_previousPosition;

    #endregion
}