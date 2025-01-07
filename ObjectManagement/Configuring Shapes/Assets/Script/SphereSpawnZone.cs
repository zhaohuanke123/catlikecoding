﻿using UnityEngine;

/// <summary>
///  用于生成球体内部或者表面的点
/// </summary>
public class SphereSpawnZone : SpawnZone
{
    #region Unity 生命周期

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, 1f);
    }

    #endregion

    #region 属性

    public override Vector3 SpawnPoint =>
        // 生成在球体表面或者内部的点
        transform.TransformPoint(
            m_surfaceOnly ? Random.onUnitSphere : Random.insideUnitSphere
        );

    #endregion

    #region 字段

    /// <summary>
    ///  是否只生成在表面
    /// </summary>
    [SerializeField]
    protected bool m_surfaceOnly;

    #endregion
}