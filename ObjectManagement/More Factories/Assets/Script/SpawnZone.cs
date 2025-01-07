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
/// 生成点配置
/// </summary>
[System.Serializable]
public struct SpawnConfiguration
{
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
}


/// <summary>
///  生成区域 抽象类
/// </summary>
public abstract class SpawnZone : PersistableObject
{
    #region 方法

    /// <summary>
    /// 配置每个新的shape 的状态
    /// </summary>
    /// <param name="shape"> 配置的物体A </param>
    public virtual void ConfigureSpawn(Shape shape)
    {
        // 1 配置物体的位置旋转缩放 颜色
        Transform t = shape.transform;
        t.localPosition = SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * m_spawnConfig.m_scale.RandomValueInRange;

        shape.SetColor(m_spawnConfig.m_color.RandomInRange);

        // 2.  配置物体的速度和旋转速度
        shape.AngularVelocity = Random.onUnitSphere * m_spawnConfig.m_angularSpeed.RandomValueInRange;

        Vector3 direction;
        switch (m_spawnConfig.m_movementDirection)
        {
            case MovementDirection.Upward:
                direction = transform.up;
                break;
            case MovementDirection.Outward:
                direction = (t.localPosition - transform.position).normalized;
                break;
            case MovementDirection.Random:
                direction = Random.onUnitSphere;
                break;
            default:
                direction = transform.forward;
                break;
        }

        shape.Velocity = direction * m_spawnConfig.m_speed.RandomValueInRange;
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