using System;
using UnityEngine;

public class Graph : MonoBehaviour
{
    /// <summary>
    /// 函数采样点的预制体
    /// </summary>
    [SerializeField] Transform pointPrefab;

    /// <summary>
    ///  函数采样点的数量
    /// </summary>
    [SerializeField, Range(10, 1000)] private int resolution = 10;

    /// <summary>
    ///  采样点数组
    /// </summary>
    private Transform[] points;

    private void Awake()
    {
        // for (int i = 0; i < 10; i++)
        // {
        //     Transform point = Instantiate(pointPrefab);
        //     point.localPosition = Vector3.right * i / 5;
        //     point.localPosition = Vector3.right * i;
        // }

        // for (int i = 0; i < 10; i++)
        // {
        //     Transform point = Instantiate(pointPrefab);
        //     point.localPosition = Vector3.right * i;
        //     // point.localScale = Vector3.one / 5f;
        //     // point.localPosition = Vector3.right * (i / 5f - 1f);
        //     point.localPosition = Vector3.right * ((i + 0.5f) / 5f - 1f);
        // }

        // Vector3 position = new Vector3();
        // var scale = Vector3.one / 5f;
        // for (int i = 0; i < resolution; i++)
        // {
        //     Transform point = Instantiate(pointPrefab);
        //     // point.localPosition = Vector3.right * ((i + 0.5f) / 5f - 1f);
        //     // point.localScale = scale;
        //     position.x = (i + 0.5f) / 5f - 1f;
        //     position.y = position.x * position.x;
        //     point.localPosition = position;
        //     point.localScale = scale;
        // }
        points = new Transform[resolution];
        float step = 2f / resolution;
        var position = Vector3.zero;
        var scale = Vector3.one * step;
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = Instantiate(pointPrefab, transform, false);
            points[i] = point;
            position.x = (i + 0.5f) * step - 1f;
            // position.y = position.x * position.x * position.x;
            point.localPosition = position;
            point.localScale = scale;
        }
    }

    private void Update()
    {
        var time = Time.time;
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i];
            Vector3 position = point.localPosition;
            position.y = Mathf.Sin(Mathf.PI * (position.x + time));
            point.localPosition = position;
        }
    }
}