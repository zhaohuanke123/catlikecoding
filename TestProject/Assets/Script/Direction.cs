using UnityEngine;

/// <summary>
/// 方向枚举，用于表示四个基本的方位。
/// </summary>
public enum Direction
{
    North,
    East,
    South,
    West
}

/// <summary>
/// 方向变更枚举，描述从一个方向转向另一个方向时的动作。
/// </summary>
public enum DirectionChange
{
    None,
    TurnRight,
    TurnLeft,
    TurnAround
}

/// <summary>
/// 提供对<see cref="Direction"/>枚举类型扩展方法的静态类
/// </summary>
public static class DirectionExtensions
{
    /// <summary>
    /// 获取指定方向的旋转Quaternion。
    /// </summary>
    /// <param name="direction">要获取旋转的方向。</param>
    /// <returns>表示该方向的旋转Quaternion。</returns>
    public static Quaternion GetRotation(this Direction direction)
    {
        return s_rotations[(int)direction];
    }

    /// <summary>
    /// 获取从当前方向到目标方向所需的方向变更。
    /// </summary>
    /// <param name="current">当前方向。</param>
    /// <param name="next">目标方向。</param>
    /// <returns>表示所需方向变更的<see cref="DirectionChange"/>枚举值。</returns>
    public static DirectionChange GetDirectionChangeTo(this Direction current, Direction next)
    {
        if (current == next)
        {
            return DirectionChange.None;
        }
        else if (current + 1 == next || current - 3 == next)
        {
            return DirectionChange.TurnRight;
        }
        else if (current - 1 == next || current + 3 == next)
        {
            return DirectionChange.TurnLeft;
        }

        return DirectionChange.TurnAround;
    }

    /// <summary>
    /// 获取指定方向的角度值（以度为单位）。
    /// </summary>
    /// <param name="direction">要获取角度的方向。</param>
    /// <returns>表示该方向的角度值，范围为0至360度。</returns>
    public static float GetAngle(this Direction direction)
    {
        return (float)direction * 90f;
    }

    /// <summary>
    /// 获取指定方向的半向量。
    /// </summary>
    /// <param name="direction">要获取半向量的方向。</param>
    /// <returns>表示该方向的半向量。</returns>
    public static Vector3 GetHalfVector(this Direction direction)
    {
        return s_halfVectors[(int)direction];
    }

    #region 字段

    /// <summary>
    /// 静态数组，存储了对应<see cref="Direction"/>枚举中每个方向的旋转Quaternion。
    /// 这些旋转Quaternion分别代表北(North)、东(East)、南(South)、西(West)四个方向。
    /// </summary>
    private static Quaternion[] s_rotations =
    {
        Quaternion.identity,
        Quaternion.Euler(0f, 90f, 0f),
        Quaternion.Euler(0f, 180f, 0f),
        Quaternion.Euler(0f, 270f, 0f)
    };

    /// <summary>
    /// 存储与<see cref="Direction"/>枚举每个方向对应的半向量数组。
    /// 每个半向量代表了朝着该方向移动半个单位的距离。
    /// </summary>
    private static Vector3[] s_halfVectors =
    {
        Vector3.forward * 0.5f,
        Vector3.right * 0.5f,
        Vector3.back * 0.5f,
        Vector3.left * 0.5f
    };

    #endregion
}