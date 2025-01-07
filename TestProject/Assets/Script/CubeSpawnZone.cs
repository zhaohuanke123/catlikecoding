using UnityEngine;

public class CubeSpawnZone : SpawnZone
{
    #region Unity 生命周期

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    #endregion

    #region 属性

    public override Vector3 SpawnPoint
    {
        get
        {
            Vector3 p;
            // 1. 随机生成一个正方体内的点
            p.x = Random.Range(-0.5f, 0.5f);
            p.y = Random.Range(-0.5f, 0.5f);
            p.z = Random.Range(-0.5f, 0.5f);

            // 2. 如果surfaceOnly为true，则只生成在正方体表面的点
            if (m_surfaceOnly)
            {
                int axis = Random.Range(0, 3);
                p[axis] = p[axis] < 0f ? -0.5f : 0.5f;
            }

            // 3. 将生成的点转换到世界坐标系中, 使用自己的transform的localToWorldMatrix
            return transform.TransformPoint(p);
        }
    }

    #endregion

    #region 字段

    /// <summary>
    ///  是否只生成在表面
    /// </summary>
    [SerializeField]
    protected bool m_surfaceOnly;

    #endregion
}