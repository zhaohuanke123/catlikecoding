using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 显示绘制一个Star
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Star : MonoBehaviour
{
    #region unity 生命周期

    private void Start()
    {
        UpdateMesh();
    }

    public void UpdateMesh()
    {
        if (m_mesh == null)
        {
            // 1. 设置mesh和名字
            GetComponent<MeshFilter>().mesh = m_mesh = new Mesh();
            m_mesh.name = "Star Mesh";
            m_mesh.hideFlags = HideFlags.HideAndDontSave;
        }

        if (m_frequency < 1)
        {
            m_frequency = 1;
        }

        m_points ??= Array.Empty<ColorPoint>();
        int numberOfPoints = m_frequency * m_points.Length;

        if (m_vertices == null || m_vertices.Length != numberOfPoints + 1)
        {
            // 初始化网格数据
            m_vertices = new Vector3[numberOfPoints + 1];
            m_triangles = new int[numberOfPoints * 3];
            m_colors = new Color[numberOfPoints + 1];
            m_mesh.Clear();
        }

        if (numberOfPoints >= 3)
        {
            m_vertices[0] = m_center.m_position;
            m_colors[0] = m_center.m_color;

            // 2. 计算网格数据
            float angle = -360f / numberOfPoints;
            for (int repetitions = 0, v = 1, t = 1; repetitions < m_frequency; repetitions++)
            {
                for (int p = 0; p < m_points.Length; p += 1, v += 1, t += 3)
                {
                    m_vertices[v] = Quaternion.Euler(0f, 0f, angle * (v - 1)) * m_points[p].m_position;
                    m_colors[v] = m_points[p].m_color;
                    m_triangles[t] = v;
                    m_triangles[t + 1] = v + 1;
                }
            }
        }

        m_triangles[m_triangles.Length - 1] = 1;

        m_mesh.vertices = m_vertices;
        m_mesh.colors = m_colors;
        m_mesh.triangles = m_triangles;
    }

    private void OnEnable()
    {
        UpdateMesh();
    }

    private void Reset()
    {
        UpdateMesh();
    }

    #endregion

    #region 字段

    /// <summary>
    /// star使用的mesh，用于存储星形的网格数据
    /// </summary>
    private Mesh m_mesh;

    /// <summary>
    /// 星形的中心点，包含位置和颜色信息
    /// </summary>
    public ColorPoint m_center;

    /// <summary>
    /// 星形的单次循环的所有点集合，每个点包含位置和颜色信息
    /// </summary>
    public ColorPoint[] m_points;

    /// <summary>
    /// 存储网格的颜色数据，每个顶点的颜色
    /// </summary>
    private Color[] m_colors;

    /// <summary>
    /// 控制频率，决定每次旋转的重复次数
    /// </summary>
    public int m_frequency = 1;

    /// <summary>
    /// 存储网格的顶点数据，每个顶点的位置
    /// </summary>
    private Vector3[] m_vertices;

    /// <summary>
    /// 存储网格的三角形数据，定义了网格的面
    /// </summary>
    private int[] m_triangles;

    #endregion
}