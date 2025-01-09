using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 移动方向枚举
/// </summary>
public enum MovementDirection
{
    Forward,
    Upward,
    Outward,
    Random
}

/// <summary>
/// 生成点配置
/// </summary>
[System.Serializable]
public struct SpawnConfiguration
{
    /// <summary>
    ///  生成物体的工厂数组
    /// </summary>
    public ShapeFactory[] m_factories;

    /// <summary>
    ///  移动方向
    /// </summary>
    public MovementDirection m_movementDirection;

    /// <summary>
    ///  速度范围
    /// </summary>
    public FloatRange m_speed;

    /// <summary>
    ///  角速度范围
    /// </summary>
    public FloatRange m_angularSpeed;

    /// <summary>
    ///  缩放范围
    /// </summary>
    public FloatRange m_scale;

    /// <summary>
    ///  颜色范围
    /// </summary>
    public ColorRangeHSV m_color;

    /// <summary>
    ///  组合物体各个部分是否使用相同的颜色
    /// </summary>
    public bool m_uniformColor;

    /// <summary>
    ///  震动方向
    /// </summary>
    public MovementDirection m_oscillationDirection;

    /// <summary>
    ///  震动幅度
    /// </summary>
    public FloatRange m_oscillationAmplitude;

    /// <summary>
    ///  震动频率
    /// </summary>
    public FloatRange m_oscillationFrequency;
}


/// <summary>
///  生成区域 抽象类
/// </summary>
public abstract class SpawnZone : PersistableObject
{
    #region 方法

    /// <summary>
    /// 生成一个新的shape
    /// </summary>
    public virtual Shape SpawnShape()
    {
        int factoryIndex = Random.Range(0, m_spawnConfig.m_factories.Length);
        Shape shape = m_spawnConfig.m_factories[factoryIndex].GetRandom();

        // 1 配置物体的位置旋转缩放 颜色
        Transform t = shape.transform;
        t.localPosition = SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * m_spawnConfig.m_scale.RandomValueInRange;

        if (m_spawnConfig.m_uniformColor)
        {
            shape.SetColor(m_spawnConfig.m_color.RandomInRange);
        }
        else
        {
            for (int i = 0; i < shape.ColorCount; i++)
            {
                shape.SetColor(m_spawnConfig.m_color.RandomInRange, i);
            }
        }

        // 2.  添加旋转和移动组件 配置物体的速度和旋转速度
        float angularSpeed = m_spawnConfig.m_angularSpeed.RandomValueInRange;
        if (angularSpeed != 0f)
        {
            var rotation = shape.AddBehavior<RotationShapeBehavior>();
            rotation.AngularVelocity = Random.onUnitSphere * m_spawnConfig.m_angularSpeed.RandomValueInRange;
        }

        float speed = m_spawnConfig.m_speed.RandomValueInRange;
        if (speed != 0f)
        {
            var movement = shape.AddBehavior<MovementShapeBehavior>();
            movement.Velocity = GetDirectionVector(m_spawnConfig.m_movementDirection, t) * speed;
        }

        // 3. 配置震动组件
        SetupOscillation(shape);
        return shape;
    }

    /// <summary>
    /// 根据配置的枚举获取方向向量。
    /// </summary>
    /// <param name="direction">移动方向的枚举类型，定义了方向选项。</param>
    /// <param name="t">参考对象的变换 (Transform)，用于计算相对方向。</param>
    /// <returns>返回计算出的方向向量。</returns>
    private Vector3 GetDirectionVector(MovementDirection direction, Transform t)
    {
        switch (direction)
        {
            case MovementDirection.Upward:
                return transform.up;
            case MovementDirection.Outward:
                return (t.localPosition - transform.position).normalized;
            case MovementDirection.Random:
                return Random.onUnitSphere;
            default:
                return transform.forward;
        }
    }

    /// <summary>
    /// 配置震动组件的数据。
    /// </summary>
    /// <param name="shape">目标形状对象 (Shape)，用于添加震动行为。</param>
    private void SetupOscillation(Shape shape)
    {
        float amplitude = m_spawnConfig.m_oscillationAmplitude.RandomValueInRange;
        float frequency = m_spawnConfig.m_oscillationFrequency.RandomValueInRange;
        if (amplitude == 0f || frequency == 0f)
        {
            return;
        }

        var oscillation = shape.AddBehavior<OscillationShapeBehavior>();
        oscillation.Offset = GetDirectionVector(m_spawnConfig.m_oscillationDirection, shape.transform) * amplitude;
        oscillation.Frequency = frequency;
        Debug.Log(amplitude + " " + frequency);
    }

    #endregion

    #region 属性

    /// <summary>
    /// 用于获取生成点
    /// </summary>
    public abstract Vector3 SpawnPoint { get; }

    #endregion

    #region 字段

    /// <summary>
    ///  生成区域的配置
    /// </summary>
    [SerializeField]
    private SpawnConfiguration m_spawnConfig;

    #endregion
}