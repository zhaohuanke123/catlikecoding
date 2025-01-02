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
        return position.normalized * Physics.gravity.y;
    }

    /// <summary>
    /// 获取向上方向
    /// 根据物体的位置，计算出重力方向相反的向上方向。
    /// </summary>
    /// <param name="position">物体的位置</param>
    /// <returns>根据位置计算出的向上方向</returns>
    public static Vector3 GetUpAxis(Vector3 position)
    {
        var up = position.normalized;
        return Physics.gravity.y < 0f ? up : -up;
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
        var up = position.normalized;
        upAxis = Physics.gravity.y < 0f ? up : -up;
        return up * Physics.gravity.y;
    }

    #endregion
}