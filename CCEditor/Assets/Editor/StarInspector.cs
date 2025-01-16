using UnityEditor;
using UnityEngine;

/// <summary>
/// Star Inspector自定义绘制
/// </summary>
[CustomEditor(typeof(Star))]
[CanEditMultipleObjects]
public class StarInspector : Editor
{
    #region Unity 生命周期

    public override void OnInspectorGUI()
    {
        // 1. 获取属性
        SerializedProperty points = serializedObject.FindProperty("m_points");
        SerializedProperty frequency = serializedObject.FindProperty("m_frequency");

        serializedObject.Update();

        // 2. 绘制属性
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_center"));
        EditorList.Show(points, EditorListOption.Buttons | EditorListOption.ListLabel);
        EditorGUILayout.IntSlider(frequency, 1, 20);

        // 3. 提示信息
        if (!serializedObject.isEditingMultipleObjects)
        {
            int totalPoints = frequency.intValue * points.arraySize;
            if (totalPoints < 3)
            {
                EditorGUILayout.HelpBox("At least three points are needed.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox(totalPoints + " points in total.", MessageType.Info);
            }
        }

        // 4. 修改时或者Undo/Redo时更新Mesh
        if (serializedObject.ApplyModifiedProperties() ||
            (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed")
           )
        {
            foreach (var o in targets)
            {
                {
                    if (o is Star s)
                    {
                        if (PrefabUtility.GetPrefabType(s) != PrefabType.Prefab)
                        {
                            s.UpdateMesh();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Scene 可编辑
    /// </summary>
    private void OnSceneGUI()
    {
        // 1. 获取编辑对象
        Star star = target as Star;
        Transform starTransform = star.transform;

        // 2. 绘制可编辑移动点
        float angle = -360f / (star.m_frequency * star.m_points.Length);
        for (int i = 0; i < star.m_points.Length; i++)
        {
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle * i);
            Vector3 oldPoint = starTransform.TransformPoint(rotation * star.m_points[i].m_position);
            Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, 0.02f, s_pointSnap, Handles.DotHandleCap);

            if (oldPoint != newPoint)
            {
                Undo.RecordObject(star, "Move");
                star.m_points[i].m_position =
                    Quaternion.Inverse(rotation) * starTransform.InverseTransformPoint(newPoint);
                star.UpdateMesh();
            }
        }
    }

    #endregion

    #region 字段

    /// <summary>
    /// 点的吸附距离
    /// </summary>
    private static Vector3 s_pointSnap = Vector3.one * 0.1f;

    #endregion
}