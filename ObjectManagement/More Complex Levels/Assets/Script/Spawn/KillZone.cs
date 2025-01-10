using UnityEngine;

/// <summary>
/// 此类代表一个杀伤区域，当指定的对象进入该区域时，将触发特定的事件或行为，通常用于游戏中的危险区域、边界等。
/// </summary>
public class KillZone : MonoBehaviour
{
    #region Unity 生命周期

    private void OnTriggerEnter(Collider other)
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
        Gizmos.color = Color.magenta;
        var c = GetComponent<Collider>();
        
        // 1. 根据碰撞体类型绘制不同的Gizmos
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

    #region 方法

    #endregion

    #region 事件

    #endregion

    #region 属性

    #endregion

    #region 字段

    [SerializeField]
    private float m_dyingDuration;

    #endregion
}