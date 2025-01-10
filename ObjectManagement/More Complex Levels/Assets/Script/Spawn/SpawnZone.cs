using UnityEngine;

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
///  生成区域 抽象类
/// </summary>
public abstract class SpawnZone : PersistableObject
{
    #region 方法

    /// <summary>
    /// 生成新的shape, 带卫星
    /// </summary>
    public virtual void SpawnShapes()
    {
        int factoryIndex = Random.Range(0, m_spawnConfig.m_factories.Length);
        Shape shape = m_spawnConfig.m_factories[factoryIndex].GetRandom();

        // 1 配置物体的位置旋转缩放 颜色
        Transform t = shape.transform;
        t.localPosition = SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * m_spawnConfig.m_scale.RandomValueInRange;

        SetupColor(shape);

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

        Vector3 lifecycleDurations = m_spawnConfig.m_lifecycle.RandomDurations;


        // 4. 生成卫星
        int satelliteCount = m_spawnConfig.m_satellite.m_amount.RandomValueInRange;
        for (int i = 0; i < satelliteCount; i++)
        {
            CreateSatelliteFor(
                shape,
                m_spawnConfig.m_satellite.m_uniformLifecycles
                    ? lifecycleDurations
                    : m_spawnConfig.m_lifecycle.RandomDurations
            );
        }

        // 5. 添加生命周期行为
        SetupLifecycle(shape, lifecycleDurations);
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
    /// <param name="shape">目标shape对象 (Shape)，用于添加震动行为。</param>
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
    }

    /// <summary>
    /// 为指定的焦点Shape创建一个卫星Shape，该卫星将在焦点Shape周围运行。
    /// </summary>
    /// <param name="focalShape">焦点Shape，卫星将围绕这个Shape运行。</param>
    /// <param name="lifecycleDurations">卫星的生命周期时长范围向量。包含开始和结束时间信息。</param>
    private void CreateSatelliteFor(Shape focalShape, Vector3 lifecycleDurations)
    {
        // 1. 生成卫星
        int factoryIndex = Random.Range(0, m_spawnConfig.m_factories.Length);
        Shape shape = m_spawnConfig.m_factories[factoryIndex].GetRandom();
        Transform t = shape.transform;

        // 2. 配置卫星的位置旋转缩放 颜色
        t.localRotation = Random.rotation;
        t.localScale = focalShape.transform.localScale * m_spawnConfig.m_satellite.m_relativeScale.RandomValueInRange;
        SetupColor(shape);

        // 3. 添加卫星行为并初始化
        shape.AddBehavior<SatelliteShapeBehavior>().Initialize(shape, focalShape,
            m_spawnConfig.m_satellite.m_orbitRadius.RandomValueInRange,
            m_spawnConfig.m_satellite.m_orbitFrequency.RandomValueInRange
        );

        // 4. 添加生命周期行为
        SetupLifecycle(shape, lifecycleDurations);
    }

    /// <summary>
    ///  配置颜色
    /// </summary>
    /// <param name="shape"> 需要配置颜色的Shape </param>
    private void SetupColor(Shape shape)
    {
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
    }

    /// <summary>
    /// 设置shape的生命周期行为，根据给定的持续时间使shape生长或消亡。
    /// </summary>
    /// <param name="shape">需要配置生命周期行为的shape实例。</param>
    /// <param name="durations">一个包含两个元素的向量，分别表示shape变大和变小的持续时间（秒）。</param>
    private void SetupLifecycle(Shape shape, Vector3 durations)
    {
        if (durations.x > 0f)
        {
            if (durations.y > 0f || durations.z > 0f)
            {
                shape.AddBehavior<LifecycleShapeBehavior>().Initialize(shape, durations.x, durations.y, durations.z);
            }
            else
            {
                shape.AddBehavior<GrowingShapeBehavior>().Initialize(shape, durations.x);
            }
        }
        else if (durations.y > 0f)
        {
            shape.AddBehavior<LifecycleShapeBehavior>().Initialize(shape, durations.x, durations.y, durations.z);
        }
        else if (durations.z > 0f)
        {
            shape.AddBehavior<DyingShapeBehavior>().Initialize(shape, durations.z);
        }
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