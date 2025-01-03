using UnityEngine;

/// <summary>
/// 重力源类，提供通用的重力计算接口
/// </summary>
public class GravitySource : MonoBehaviour
{
    #region Unity 生命周期

    private void OnEnable()
    {
        CustomGravity.Register(this);
    }

    private void OnDisable()
    {
        CustomGravity.Unregister(this);
    }

    #endregion

    #region 方法

    /// <summary>
    ///  获取重力方向
    /// </summary>
    /// <param name="position">物体的位置</param>
    /// <returns>返回该位置的重力方向向量</returns>
    public virtual Vector3 GetGravity(Vector3 position)
    {
        return Physics.gravity;
    }

    #endregion

    #region 字段

    /// <summary>
    ///  重力大小
    /// </summary>

    [SerializeField]
    protected float m_gravity = 9.81f;

    #endregion
}