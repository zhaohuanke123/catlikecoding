using System.Collections.Generic;
using UnityEngine;

public static class ShapeBehaviorPool<T> where T : ShapeBehavior, new()
{
    #region 方法

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

    public static void Reclaim(T behavior)
    {
#if UNITY_EDITOR
        behavior.IsReclaimed = true;
#endif
        s_stack.Push(behavior);
    }

    #endregion

    #region 字段

    private static Stack<T> s_stack = new Stack<T>();

    #endregion
}