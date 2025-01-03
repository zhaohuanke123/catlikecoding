using UnityEngine;

namespace Script
{
    public class GravitySphere : GravitySource
    {
        #region Unity 生命周期

        private void Awake()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            m_innerFalloffRadius = Mathf.Max(m_innerFalloffRadius, 0f);
            m_innerRadius = Mathf.Max(m_innerRadius, m_innerFalloffRadius);
            m_outerRadius = Mathf.Max(m_outerRadius, m_innerRadius);
            m_outerFalloffRadius = Mathf.Max(m_outerFalloffRadius, m_outerRadius);

            m_innerFalloffFactor = 1f / (m_innerRadius - m_innerFalloffRadius);
            m_outerFalloffFactor = 1f / (m_outerFalloffRadius - m_outerRadius);
        }

        private void OnDrawGizmos()
        {
            var p = transform.position;

            // 1. 绘制内衰减半径
            if (m_innerFalloffRadius > 0f && m_innerFalloffRadius < m_innerRadius)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(p, m_innerFalloffRadius);
            }

            // 2. 绘制内半径
            Gizmos.color = Color.yellow;
            if (m_innerRadius > 0f && m_innerRadius < m_outerRadius)
            {
                Gizmos.DrawWireSphere(p, m_innerRadius);
            }

            // 3. 绘制外半径
            Gizmos.DrawWireSphere(p, m_outerRadius);
            if (m_outerFalloffRadius > m_outerRadius)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(p, m_outerFalloffRadius);
            }
        }

        #endregion

        #region 方法

        public override Vector3 GetGravity(Vector3 position)
        {
            // 1. 计算重力源和位置之间的 方向。
            var vector = transform.position - position;

            // 2. 超过范围则返回 0。
            float distance = vector.magnitude;
            if (distance > m_outerFalloffRadius || distance < m_innerFalloffRadius)
            {
                return Vector3.zero;
            }

            // 3. 使用已计算的距离计算归一化
            float g = m_gravity / distance;

            // 4. 计算衰减
            // 在外半径和外衰减半径之间线性降低重力，其减小方式与平面相同。
            if (distance > m_outerRadius)
            {
                g *= 1f - (distance - m_outerRadius) * m_outerFalloffFactor;
            }
            // 在内半径和内衰减半径之间线性调整速度
            else if (distance < m_innerRadius)
            {
                g *= 1f - (m_innerRadius - distance) * m_innerFalloffFactor;
            }

            return g * vector;
        }

        #endregion

        #region 字段

        /// <summary>
        /// 外半径 
        /// </summary>
        [SerializeField]
        [Min(0f)]
        private float m_outerRadius = 10f;

        /// <summary>
        /// 外衰减半径 
        /// </summary>
        [SerializeField]
        [Min(0f)]
        private float m_outerFalloffRadius = 15f;

        /// <summary>
        /// 外衰减因子
        /// </summary>
        private float m_outerFalloffFactor;

        /// <summary>
        /// 内衰减因子
        /// </summary>
        private float m_innerFalloffFactor;

        /// <summary>
        /// 内衰减半径
        /// </summary>
        [SerializeField]
        [Min(0f)]
        private float m_innerFalloffRadius = 1f;

        /// <summary>
        /// 内半径
        /// </summary>
        [SerializeField]
        [Min(0f)]
        private float m_innerRadius = 5f;

        #endregion
    }
}