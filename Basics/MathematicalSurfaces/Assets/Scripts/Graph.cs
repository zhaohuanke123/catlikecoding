using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Graph : MonoBehaviour
{
    [SerializeField] Transform m_pointPrefab;

    [SerializeField, Range(10, 1000)] private int m_resolution = 10;

    [SerializeField] FunctionLibrary.FunctionName m_function;

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
        FunctionLibrary.Function f = FunctionLibrary.GetFunction(m_function);
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
            m_points[i].localPosition = f(u, v, time);
        }
    }
}