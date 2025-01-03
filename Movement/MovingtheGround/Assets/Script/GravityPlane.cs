using UnityEngine;

namespace Script
{
    public class GravityPlane : GravitySource
    {
        #region Unity 生命周期

        private void OnDrawGizmos()
        {
            /* // 1. 默认情况下，小工具以世界空间绘制。
            // 为了正确定位和旋转正方形，我们需要使用我们平面的变换矩阵，通过将它的 localToWorldMatrix 赋值给 Gizmos.matrix 。
            // 这也允许我们缩放平面对象，使正方形更容易看到，即使这不会影响平面的重力。
            // Gizmos.matrix = transform.localToWorldMatrix;*/

            // 1. 使用范围作为偏移量，不受游戏对象的缩放比例影响。可以通过 Matrix4x4.TRS 方法自己构造一个矩阵来实现这一点，将位置、旋转和缩放比例传递给它。
            var scale = transform.localScale;
            scale.y = m_range;
            Gizmos.matrix =
                Matrix4x4.TRS(transform.position, transform.rotation, scale);

            // 2. 绘制正方形指示重力范围
            var size = new Vector3(1f, 0f, 1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, size);
            if (m_range > 0f)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(Vector3.up, size);
            }
        }

        #endregion

        #region 方法

        public override Vector3 GetGravity(Vector3 position)
        {
            var up = transform.up;

            float distance = Vector3.Dot(up, position - transform.position);
            if (distance > m_range)
            {
                return Vector3.zero;
            }

            float g = -m_gravity;
            if (distance > 0f)
            {
                g *= 1f - distance / m_range;
            }

            return g * up;
        }

        #endregion

        #region 字段

        /// <summary>
        ///  重力范围
        /// </summary>

        [SerializeField]
        [Min(0f)]
        private float m_range = 1f;

        #endregion
    }
}