using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 提供shape行为对象的复用池管理。
/// 此类允许高效地获取和回收特定类型的`ShapeBehavior`实例，以减少内存分配和垃圾回收的压力。
/// 使用泛型约束确保所有对象都是`ShapeBehavior`的子类，并且可以被实例化。 </summary>
/// <typeparam name="T">行为类型，必须继承自`ShapeBehavior`并且具有默认构造函数。</typeparam>
public static class ShapeBehaviorPool<T> where T : ShapeBehavior, new()
{
    #region 方法

    /// <summary>
    /// 从池中获取一个复用的shape行为实例。
    /// 尝试从对象池中弹出一个已有的实例，如果池为空，则根据泛型参数T创建一个新的实例。
    /// 注意：在Unity编辑器环境下，将使用ScriptableObject.CreateInstance创建实例。
    /// </summary>
    /// <returns>一个shape行为T的实例，已准备好被使用或重置后重新使用。</returns>
    public static T Get()
    {
        if (s_stack.Count > 0)
        {
            T behavior = s_stack.Pop();
#if UNITY_EDITOR
            behavior.IsReclaimed = false;
#endif

            return behavior;
        }
#if UNITY_EDITOR
        return ScriptableObject.CreateInstance<T>();
#else
		return new T();
#endif
    }

    /// <summary>
    /// 将使用完毕的shape行为实例放回池中以便复用。
    /// 此方法应由shape行为实例调用以表明其不再被使用，可以安全地返回池中等待下一次复用。
    /// 实际操作中，会将该行为实例压入栈中，若在Unity编辑器环境中，还会标记该实例为已回收。
    /// </summary>
    /// <param name="behavior">要回收的shape行为实例。必须是之前从池中获取的实例，且类型T与池匹配。</param>
    public static void Reclaim(T behavior)
    {
#if UNITY_EDITOR
        behavior.IsReclaimed = true;
#endif
        s_stack.Push(behavior);
    }

    #endregion

    #region 字段

    /// <summary>
    /// 静态私有字段，用于存储shape行为实例的复用池。
    /// 类型为Stack，确保遵循先进后出（LIFO）原则，优化频繁回收和获取场景中shape行为实例的性能。
    /// 此栈实例由`ShapeBehaviorPool`管理，负责在对象不再需要时收集它们，以及当需要时提供新的或复用的实例。
    /// </summary>
    private static Stack<T> s_stack = new Stack<T>();

    #endregion
}