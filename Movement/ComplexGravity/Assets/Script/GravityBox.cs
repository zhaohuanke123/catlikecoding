using System.Threading.Tasks;
using UnityEngine;

public class GravityBox : GravitySource
{
    #region unity 生命周期

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        // 1. 确保边界距离不会小于零
        m_boundaryDistance = Vector3.Max(m_boundaryDistance, Vector3.zero);

        // 2. 内部距离不能超过边界距离的最小值
        float maxInner = Mathf.Min(Mathf.Min(m_boundaryDistance.x, m_boundaryDistance.y), m_boundaryDistance.z);
        m_innerDistance = Mathf.Min(m_innerDistance, maxInner);

        // 3. 内部衰减距离需要大于等于内部距离并且小于等于最小边界值
        m_innerFalloffDistance = Mathf.Max(Mathf.Min(m_innerFalloffDistance, maxInner), m_innerDistance);

        // 4. 外部衰减距离应大于外部距离
        m_outerFalloffDistance = Mathf.Max(m_outerFalloffDistance, m_outerDistance);

        // 5. 计算内部衰减因子和外部衰减因子
        m_innerFalloffFactor = 1f / (m_innerFalloffDistance - m_innerDistance);
        m_outerFalloffFactor = 1f / (m_outerFalloffDistance - m_outerDistance);
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        // 1. 绘制内部衰减区域
        Vector3 size;
        if (m_innerFalloffDistance > m_innerDistance)
        {
            Gizmos.color = Color.cyan;
            size.x = 2f * (m_boundaryDistance.x - m_innerFalloffDistance);
            size.y = 2f * (m_boundaryDistance.y - m_innerFalloffDistance);
            size.z = 2f * (m_boundaryDistance.z - m_innerFalloffDistance);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }

        // 2. 绘制内部区域
        if (m_innerDistance > 0f)
        {
            Gizmos.color = Color.yellow;
            size.x = 2f * (m_boundaryDistance.x - m_innerDistance);
            size.y = 2f * (m_boundaryDistance.y - m_innerDistance);
            size.z = 2f * (m_boundaryDistance.z - m_innerDistance);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }

        // 3. 绘制外部区域
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, 2f * m_boundaryDistance);

        if (m_outerDistance > 0f)
        {
            Gizmos.color = Color.yellow;
            DrawGizmosOuterCube(m_outerDistance);
        }

        if (m_outerFalloffDistance > m_outerDistance)
        {
            Gizmos.color = Color.cyan;
            DrawGizmosOuterCube(m_outerFalloffDistance);
        }
    }

    #endregion

    #region 方法

    /// <summary>
    /// 绘制由四个点构成的矩形的边框
    /// </summary>
    /// <param name="a">矩形的第一个顶点</param>
    /// <param name="b">矩形的第二个顶点</param>
    /// <param name="c">矩形的第三个顶点</param>
    /// <param name="d">矩形的第四个顶点</param>
    private void DrawGizmosRect(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, d);
        Gizmos.DrawLine(d, a);
    }

    /// <summary>
    /// 绘制外部重力盒的 Gizmos，显示重力盒外部的衰减区域
    /// </summary>
    /// <param name="distance">外部距离，用于计算重力盒的外部边界</param>
    private void DrawGizmosOuterCube(float distance)
    {
        // 1. 计算并绘制边界距离加上给定距离后的外部方块
        Vector3 a, b, c, d;
        a.y = b.y = m_boundaryDistance.y;
        d.y = c.y = -m_boundaryDistance.y;
        b.z = c.z = m_boundaryDistance.z;
        d.z = a.z = -m_boundaryDistance.z;
        a.x = b.x = c.x = d.x = m_boundaryDistance.x + distance;
        DrawGizmosRect(a, b, c, d);
        a.x = b.x = c.x = d.x = -a.x;
        DrawGizmosRect(a, b, c, d);

        a.x = d.x = m_boundaryDistance.x;
        b.x = c.x = -m_boundaryDistance.x;
        a.z = b.z = m_boundaryDistance.z;
        c.z = d.z = -m_boundaryDistance.z;
        a.y = b.y = c.y = d.y = m_boundaryDistance.y + distance;
        DrawGizmosRect(a, b, c, d);
        a.y = b.y = c.y = d.y = -a.y;
        DrawGizmosRect(a, b, c, d);

        a.x = d.x = m_boundaryDistance.x;
        b.x = c.x = -m_boundaryDistance.x;
        a.y = b.y = m_boundaryDistance.y;
        c.y = d.y = -m_boundaryDistance.y;
        a.z = b.z = c.z = d.z = m_boundaryDistance.z + distance;
        DrawGizmosRect(a, b, c, d);
        a.z = b.z = c.z = d.z = -a.z;
        DrawGizmosRect(a, b, c, d);

        // 2. 绘制距离外部边界的线框
        distance *= 0.5773502692f;
        var size = m_boundaryDistance;
        size.x = 2f * (size.x + distance);
        size.y = 2f * (size.y + distance);
        size.z = 2f * (size.z + distance);
        Gizmos.DrawWireCube(Vector3.zero, size);
    }

    /// <summary>
    /// 获取指定位置的重力方向
    /// </summary>
    /// <param name="position">指定位置的世界坐标</param>
    /// <returns>返回该位置的重力方向</returns>
    public override Vector3 GetGravity(Vector3 position)
    {
        // 1. 将位置转换为本地坐标系
        position = transform.InverseTransformDirection(position - transform.position);
        var vector = Vector3.zero;

        // 2. 检查物体是否位于重力盒的外部
        int outside = 0;
        if (position.x > m_boundaryDistance.x)
        {
            vector.x = m_boundaryDistance.x - position.x;
            outside = 1;
        }
        else if (position.x < -m_boundaryDistance.x)
        {
            vector.x = -m_boundaryDistance.x - position.x;
            outside = 1;
        }

        if (position.y > m_boundaryDistance.y)
        {
            vector.y = m_boundaryDistance.y - position.y;
            outside += 1;
        }
        else if (position.y < -m_boundaryDistance.y)
        {
            vector.y = -m_boundaryDistance.y - position.y;
            outside += 1;
        }

        if (position.z > m_boundaryDistance.z)
        {
            vector.z = m_boundaryDistance.z - position.z;
            outside += 1;
        }
        else if (position.z < -m_boundaryDistance.z)
        {
            vector.z = -m_boundaryDistance.z - position.z;
            outside += 1;
        }

        // 3. 如果物体位于外部区域，计算距离并返回相应的重力方向
        if (outside > 0)
        {
            float distance = outside == 1 ? Mathf.Abs(vector.x + vector.y + vector.z) : vector.magnitude;
            if (distance > m_outerFalloffDistance)
            {
                return Vector3.zero;
            }

            // 计算重力衰减
            float g = m_gravity / distance;
            if (distance > m_outerDistance)
            {
                g *= 1f - (distance - m_outerDistance) * m_outerFalloffFactor;
            }

            // 返回本地坐标系转换后的重力向量
            return transform.TransformDirection(g * vector);
        }

        // 4. 计算内部区域的重力
        Vector3 distances;
        distances.x = m_boundaryDistance.x - Mathf.Abs(position.x);
        distances.y = m_boundaryDistance.y - Mathf.Abs(position.y);
        distances.z = m_boundaryDistance.z - Mathf.Abs(position.z);

        if (distances.x < distances.y)
        {
            if (distances.x < distances.z)
            {
                vector.x = GetGravityComponent(position.x, distances.x);
            }
            else
            {
                vector.z = GetGravityComponent(position.z, distances.z);
            }
        }
        else if (distances.y < distances.z)
        {
            vector.y = GetGravityComponent(position.y, distances.y);
        }
        else
        {
            vector.z = GetGravityComponent(position.z, distances.z);
        }

        return transform.TransformDirection(vector);
    }

    /// <summary>
    /// 获取某一轴向的重力分量
    /// </summary>
    /// <param name="coordinate">物体在当前轴向的坐标</param>
    /// <param name="distance">物体到重力盒该轴的距离</param>
    /// <returns>返回该轴向的重力分量</returns>
    private float GetGravityComponent(float coordinate, float distance)
    {
        // 1. 如果距离大于内部衰减距离，返回零
        if (distance > m_innerFalloffDistance)
        {
            return 0f;
        }

        // 2. 计算内部衰减后的重力值
        float g = m_gravity;
        if (distance > m_innerDistance)
        {
            g *= 1f - (distance - m_innerDistance) * m_innerFalloffFactor;
        }

        // 3. 返回基于坐标方向的重力分量
        return coordinate > 0f ? -g : g;
    }

    #endregion

    #region 字段

    /// <summary>
    /// 重力盒的边界距离
    /// </summary>
    [SerializeField]
    private Vector3 m_boundaryDistance = Vector3.one;

    /// <summary>
    /// 重力盒的内部距离
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float m_innerDistance = 0f;

    /// <summary>
    /// 重力盒的内部衰减距离
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float m_innerFalloffDistance = 0f;

    /// <summary>
    /// 重力盒的内部衰减因子
    /// </summary>
    private float m_innerFalloffFactor, m_outerFalloffFactor;

    /// <summary>
    ///    重力盒的外部距离
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float m_outerDistance = 0f;

    /// <summary>
    /// 重力盒的外部衰减距离
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float m_outerFalloffDistance = 0f;

    #endregion
}