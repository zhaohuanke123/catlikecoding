using UnityEngine;

/// <summary>
///  用于旋转物体
/// </summary>
public class RotatingObject : PersistableObject
{
    #region Unity 生命周期

    private void FixedUpdate()
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