﻿using UnityEngine;

/// <summary>
/// 标识符编号, 用于标识shape的行为
/// </summary>
public enum ShapeBehaviorType
{
    Movement,
    Rotation,
    Oscillation,
    Satellite,
    Growing,
    Dying,
    Lifecycle
}

/// <summary>
///  ShapeBehaviorType 的扩展方法
/// </summary>
public static class ShapeBehaviorTypeMethods
{
    /// <summary>
    /// 根据ShapeBehaviorType获取对应的ShapeBehavior实例
    /// </summary>
    /// <param name="type">Shape行为的类型标识</param>
    /// <returns>与指定类型对应的ShapeBehavior实例，若未支持该类型则返回null并记录错误日志</returns>
    public static ShapeBehavior GetInstance(this ShapeBehaviorType type)
    {
        switch (type)
        {
            case ShapeBehaviorType.Movement:
                return ShapeBehaviorPool<MovementShapeBehavior>.Get();
            case ShapeBehaviorType.Rotation:
                return ShapeBehaviorPool<RotationShapeBehavior>.Get();
            case ShapeBehaviorType.Oscillation:
                return ShapeBehaviorPool<OscillationShapeBehavior>.Get();
            case ShapeBehaviorType.Satellite:
                return ShapeBehaviorPool<SatelliteShapeBehavior>.Get();
            case ShapeBehaviorType.Growing:
                return ShapeBehaviorPool<GrowingShapeBehavior>.Get();
            case ShapeBehaviorType.Dying:
                return ShapeBehaviorPool<DyingShapeBehavior>.Get();
            case ShapeBehaviorType.Lifecycle:
                return ShapeBehaviorPool<LifecycleShapeBehavior>.Get();
        }

        Debug.LogError("Forgot to support " + type);
        return null;
    }
}

/// <summary>
///  代表一个shape的行为 抽象基类
/// </summary>
public abstract class ShapeBehavior
#if UNITY_EDITOR
    : ScriptableObject
#endif
{
    #region Unity 生命周期

#if UNITY_EDITOR

    private void OnEnable()
    {
        // 检查它是否已被回收。如果是，则使其自身进行回收。
        // 此方法在通过 ScriptableObject.CreateInstance 创建资源时以及每次热重载后都会被调用，因此池将被重新生成。
        if (IsReclaimed)
        {
            Recycle();
        }
    }
#endif

    #endregion

    #region 方法

    /// <summary>
    ///  每个shape的行为都有自己的更新方法, 用于更新shape的状态
    /// </summary>
    /// <param name="shape"> 行为组件所属的Shape </param>
    public abstract bool GameUpdate(Shape shape);

    /// <summary>
    /// 每个shape的行为可能都有配置和状态, 我们需要保存它们
    /// </summary>
    /// <param name="writer">  保存数据的对象 </param>
    public abstract void Save(GameDataWriter writer);

    /// <summary>
    /// 每个shape的行为可能都有配置和状态, 我们需要加载它们
    /// </summary>
    /// <param name="reader"> 读取数据的对象 </param>
    public abstract void Load(GameDataReader reader);

    /// <summary>
    ///  回收行为组件
    /// </summary>
    public abstract void Recycle();

    /// <summary>
    /// 发出信号，表明现在是时候解析任何shape实例
    /// </summary>
    public virtual void ResolveShapeInstances()
    {
    }

    #endregion

    #region 属性

    /// <summary>
    ///  行为类型, 用于标识行为的类型，用于序列化和反序列化
    /// </summary>
    public abstract ShapeBehaviorType BehaviorType { get; }

#if UNITY_EDITOR
    /// <summary>
    ///  是否已被回收, 用于在编辑器中检查是否已被回收
    /// </summary>
    public bool IsReclaimed { get; set; }
#endif

    #endregion
}