using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  自定义重力工具类
/// </summary>
public static class CustomGravity
{
    #region 静态工具方法

    /// <summary>
    /// 获取重力方向
    /// 根据给定的位置计算该位置的重力方向。
    /// </summary>
    /// <param name="position">物体的位置</param>
    /// <returns>返回该位置的重力方向向量</returns>
    public static Vector3 GetGravity(Vector3 position)
    {
        // 累加所有重力源的重力方向
        Vector3 g = Vector3.zero;
        for (int i = 0; i < sources.Count; i++)
        {
            g += sources[i].GetGravity(position);
        }

        return g;
    }

    /// <summary>
    /// 获取重力方向和向上方向
    /// 计算该位置的重力方向并返回与之对应的向上方向。
    /// </summary>
    /// <param name="position">物体的位置</param>
    /// <param name="upAxis">输出参数，计算出的向上方向</param>
    /// <returns>返回该位置的重力方向向量</returns>
    public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
    {
        Vector3 g = Vector3.zero;
        for (int i = 0; i < sources.Count; i++)
        {
            g += sources[i].GetGravity(position);
        }

        upAxis = -g.normalized;
        return g;
    }

    /// <summary>
    /// 获取向上方向
    /// 根据物体的位置，计算出重力方向相反的向上方向。
    /// </summary>
    /// <param name="position">物体的位置</param>
    /// <returns>根据位置计算出的向上方向</returns>
    public static Vector3 GetUpAxis(Vector3 position)
    {
        Vector3 g = Vector3.zero;
        for (int i = 0; i < sources.Count; i++)
        {
            g += sources[i].GetGravity(position);
        }

        return -g.normalized;
    }

    public static void Register(GravitySource source)
    {
        Debug.Assert(
            !sources.Contains(source),
            "Duplicate registration of gravity source!", source
        );
        sources.Add(source);
    }

    public static void Unregister(GravitySource source)
    {
        Debug.Assert(
            sources.Contains(source),
            "Attempting to unregister a source that was not registered!", source
        );
        sources.Remove(source);
    }

    #endregion

    #region 内部字段

    /// <summary>
    /// 重力源列表, 用于存储所有的重力源
    /// </summary>
    static List<GravitySource> sources = new List<GravitySource>();

    #endregion
}