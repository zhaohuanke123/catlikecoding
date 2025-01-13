using UnityEngine;

/// <summary>
/// 目标点类，用于标记enemy（Enemy）受击点位置。
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class TargetPoint : MonoBehaviour
{
    #region Unity 生命周期

    private void Awake()
    {
        Enemy = transform.root.GetComponent<Enemy>();

        Debug.Assert(Enemy != null, "Target point without Enemy root!", this);
        // Debug.Assert(GetComponent<SphereCollider>() != null, "Target point without sphere collider!", this);
        Debug.Assert(gameObject.layer == 9, "Target point on wrong layer!", this);
    }

    #endregion

    #region 方法

    #endregion


    #region 属性

    public Enemy Enemy { get; private set; }

    public Vector3 Position => transform.position;

    #endregion

    #region 字段

    #endregion
}