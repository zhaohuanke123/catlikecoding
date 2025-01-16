using UnityEditor;

/// <summary>
/// ListTester 自定义Inspector 面板绘制。
/// </summary>
[CustomEditor(typeof(ListTester))]
[CanEditMultipleObjects]
public class ListTesterInspector : Editor
{
    #region Unity 生命周期

    public override void OnInspectorGUI()
    {
        // 1. 更新序列化对象
        serializedObject.Update();

        // 2. 绘制属性
        EditorList.Show(serializedObject.FindProperty("m_integers"), EditorListOption.ListSize);
        EditorList.Show(serializedObject.FindProperty("m_vectors"));
        EditorList.Show(serializedObject.FindProperty("m_colorPoints"), EditorListOption.Buttons);
        EditorList.Show(serializedObject.FindProperty("m_objects"),
            EditorListOption.ListLabel | EditorListOption.Buttons);
        EditorList.Show(serializedObject.FindProperty("m_notAList"));

        // 3. 应用修改
        serializedObject.ApplyModifiedProperties();
    }

    #endregion
}