using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 提供了一个泛型对象池，用于管理并重用<typeparamref name="T"/>类型的<see cref="ShapeBehavior"/>实例，
/// 其中<typeparamref name="T"/>必须是继承自<see cref="ShapeBehavior"/>的新实例可被创建的类型。
/// 此类通过静态方法<see cref="Get"/>和<see cref="Reclaim"/>来分配和回收行为实例，
/// 旨在优化内存使用和提高性能，减少频繁的实例化与销毁操作。
/// </summary>
/// <typeparam name="T">行为类型，需继承自<see cref="ShapeBehavior"/>。</typeparam>
public static class ShapeBehaviorPool<T> where T : ShapeBehavior, new()
{
    #region 方法

    /// <summary>
    /// 从对象池中获取一个<typeparamref name="T"/>类型的<see cref="ShapeBehavior"/>实例。
    /// 如果对象池中有可用的实例，则直接返回；否则，将创建一个新的实例。
    /// </summary>
    /// <returns>一个可用于使用的<typeparamref name="T"/>类型的<see cref="ShapeBehavior"/>实例。</returns>
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
    /// 将使用的<typeparamref name="T"/>实例归还给对象池。
    /// 被归还的行为实例会被标记为可重用，并压入栈中以供后续获取操作使用。
    /// </summary>
    /// <param name="behavior">要归还到池中的<typeparamref name="T"/>类型的实例。</param>
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
    /// 静态私有字段，用于存储
    /// </summary>
    ///  <typeparam name="T">行为类型，需继承自<see cref="ShapeBehavior"/>。</typeparam>
    private static Stack<T> s_stack = new Stack<T>();

    #endregion
}