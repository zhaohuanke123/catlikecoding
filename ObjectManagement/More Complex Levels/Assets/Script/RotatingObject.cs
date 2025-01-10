using UnityEngine;

/// <summary>
/// 旋转对象类，用于在游戏中实现物体的持续旋转功能。
/// </summary>
public class RotatingObject : GameLevelObject
{
    #region 方法

    public override void GameUpdate()
    {
        transform.Rotate(m_angularVelocity * Time.deltaTime);
    }

    #endregion

    #region 字段

    /// <summary>
    ///  旋转速度
    /// </summary>
    [SerializeField]
    private Vector3 m_angularVelocity;

    #endregion
}