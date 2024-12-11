using TMPro;
using UnityEngine;
using UnityEditor;
using UnityEngine;

public class ClockTicksGenerator : EditorWindow
{
    private GameObject m_tickPrefab;
    private GameObject m_clockSurface;

    /// <summary>
    /// 偏移位置
    /// </summary>
    private int m_offset = 1;

    /// <summary>
    /// 是否启动旋转
    /// </summary>
    private bool m_enableRotation = false;

    [MenuItem("Tools/生成时钟刻度")]
    public static void ShowWindow()
    {
        GetWindow<ClockTicksGenerator>("生成时钟刻度");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Clock Ticks", EditorStyles.boldLabel);

        m_tickPrefab = (GameObject)EditorGUILayout.ObjectField("Tick Prefab", m_tickPrefab, typeof(GameObject), false);
        m_clockSurface =
            (GameObject)EditorGUILayout.ObjectField("Clock Surface", m_clockSurface, typeof(GameObject), true);
        m_offset = EditorGUILayout.IntField("Offset", m_offset);
        m_enableRotation = EditorGUILayout.Toggle("Enable Rotation", m_enableRotation);

        if (GUILayout.Button("Generate Ticks"))
        {
            if (m_tickPrefab != null && m_clockSurface != null)
            {
                GenerateTicks();
            }
            else
            {
                Debug.LogError("Tick Prefab and Clock Surface must be assigned!");
            }
        }
    }

    private void GenerateTicks()
    {
        Vector3 clockCenter = m_clockSurface.transform.position;
        float clockRadius = m_clockSurface.transform.localScale.x / 2 - m_offset; // 假设x-scale定义了半径
        // float tickOffset = 0.25f; // z-offset for tick positioning

        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30f * Mathf.Deg2Rad; // 角度转弧度
            float x = clockCenter.x + clockRadius * Mathf.Sin(angle);
            float y = clockCenter.y + clockRadius * Mathf.Cos(angle);
            float z = -0.25f; // 顺时针方向

            Vector3 tickPosition = new Vector3(x, y, z); // y调整为表面下方

            GameObject tick =
                (GameObject)PrefabUtility.InstantiatePrefab(m_tickPrefab, m_clockSurface.transform.parent);
            tick.transform.position = tickPosition;
            tick.name = "Tick " + i;

            Undo.RegisterCreatedObjectUndo(tick, "Create Clock Tick");

            if (tick.transform.TryGetComponent<TextMeshPro>(out var text))
            {
                text.text = (i == 0 ? 12 : i).ToString();
                tick.name = "TimeNumber" + text.text;
            }

            if (m_enableRotation)
            {
                // ，考虑旋转, 0 1 2 3 点钟方向
                tick.transform.Rotate(Vector3.forward, -i * 30f);
            }
        }

        Debug.Log("Clock ticks generated successfully!");
    }
}