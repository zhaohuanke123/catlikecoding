using UnityEngine;

public class LifeZone : MonoBehaviour
{
    #region Unity 生命周期

    private void OnTriggerExit(Collider other)
    {
        var shape = other.GetComponent<Shape>();
        if (shape)
        {
            if (m_dyingDuration <= 0f)
            {
                shape.Die();
            }
            else if (!shape.IsMarkedAsDying)
            {
                shape.AddBehavior<DyingShapeBehavior>().Initialize(shape, m_dyingDuration);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        var c = GetComponent<Collider>();

        // 2. 根据碰撞体类型绘制不同的Gizmos
        var b = c as BoxCollider;
        if (b != null)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawWireCube(b.center, b.size);
            return;
        }

        var s = c as SphereCollider;
        if (s != null)
        {
            Vector3 scale = transform.lossyScale;
            scale = Vector3.one * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, scale);
            Gizmos.DrawWireSphere(s.center, s.radius);
            return;
        }
    }

    #endregion

    #region 字段

    [SerializeField]
    private float m_dyingDuration;

    #endregion
}