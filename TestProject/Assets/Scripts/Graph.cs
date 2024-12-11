using UnityEngine;

public class Graph : MonoBehaviour
{
    /// <summary>
    ///  函数点预制体
    /// </summary>
    [SerializeField] Transform m_pointPrefab;

    /// <summary>
    /// 函数分辨率
    /// </summary>
    [SerializeField, Range(10, 1000)] private int m_resolution = 10;

    /// <summary>
    ///  函数索引
    /// </summary>
    [HideInInspector] public int m_functionIndex;

    /// <summary>
    ///  函数点数组
    /// </summary>
    private Transform[] m_points;

    private void Awake()
    {
        m_points = new Transform[m_resolution * m_resolution];
        float step = 2f / m_resolution;
        // var position = Vector3.zero;
        var scale = Vector3.one * step;
        // for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        for (int i = 0; i < m_points.Length; i++)
        {
            // if (x == resolution)
            // {
            //     x = 0;
            //     z += 1;
            // }

            Transform point = Instantiate(m_pointPrefab, transform, false);
            m_points[i] = point;
            // position.x = (x + 0.5f) * step - 1f;
            // position.z = (z + 0.5f) * step - 1f;
            // point.localPosition = position;
            point.localScale = scale;
        }
    }

    private void Update()
    {
        var time = Time.time;
        // for (int i = 0; i < points.Length; i++)
        // {
        //     Transform point = points[i];
        //     Vector3 position = point.localPosition;
        //
        //     position.y = f(position.x, position.z, time);
        //
        //     point.localPosition = position;
        // }

        float step = 2f / m_resolution;
        float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < m_points.Length; i++, x++)
        {
            if (x == m_resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }

            float u = (x + 0.5f) * step - 1f;
            // float v = (z + 0.5f) * step - 1f;
            m_points[i].localPosition = FunctionLibrary.GetFunctionValue(m_functionIndex, u, v, time);
        }
    }
}