using UnityEngine;

public class OscillationShapeBehavior : ShapeBehavior
{
    #region 方法

    public override bool GameUpdate(Shape shape)
    {
        // 1. 计算振动
        float oscillation = Mathf.Sin(2f * Mathf.PI * Frequency * shape.Age);
        // 2. 更新位置 使用+= 而不是直接赋值, 以便于多个行为组件的叠加
        // 每次更新都将振荡偏移添加到位置中，那么我们最终会累积偏移量，而不是每次更新都使用新的偏移量。
        // 为了补偿之前的振荡，我们必须记住它并在确定最终偏移量之前将其减去，并在循环利用时将其设置为零
        shape.transform.localPosition += (oscillation - m_previousOscillation) * Offset;
        // 3. 保存上一次的振动值
        m_previousOscillation = oscillation;
        return true;
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(Offset);
        writer.Write(Frequency);
        writer.Write(m_previousOscillation);
    }

    public override void Load(GameDataReader reader)
    {
        Offset = reader.ReadVector3();
        Frequency = reader.ReadFloat();
        m_previousOscillation = reader.ReadFloat();
    }

    public override void Recycle()
    {
        m_previousOscillation = 0f;
        ShapeBehaviorPool<OscillationShapeBehavior>.Reclaim(this);
    }

    #endregion

    #region 属性

    /// <summary>
    ///  振动的偏移量
    /// </summary>
    public Vector3 Offset { get; set; }

    /// <summary>
    ///  振动的频率
    /// </summary>
    public float Frequency { get; set; }

    public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Oscillation;

    #endregion

    #region 字段

    /// <summary>
    ///  上一次的振动值
    /// </summary>
    private float m_previousOscillation;

    #endregion
}