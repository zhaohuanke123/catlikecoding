using UnityEditor;
using UnityEngine;

/// <summary>
///  FloatRange 自定义属性绘制器
/// </summary>
[CustomPropertyDrawer(typeof(FloatRange))]
[CustomPropertyDrawer(typeof(IntRange))]
public class FloatRangeDrawer : PropertyDrawer
{
    #region Unity 生命周期

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 1. 无关紧要的保存和恢复, Unity 的默认编辑器会为我们恢复这些值，但我们通常不能依赖它
        int originalIndentLevel = EditorGUI.indentLevel;
        float originalLabelWidth = EditorGUIUtility.labelWidth;

        // 2. Begin End 确保编辑器能够处理预制件和预制件覆盖
        EditorGUI.BeginProperty(position, label, property);

        // 3. 绘制FloatRange字段名字标签  GUIUtility.GetControlID(FocusType.Passive) 不接受选中 
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // 4. 设置缩进、标签宽度和缩进级别
        position.width /= 2f;
        EditorGUIUtility.labelWidth = position.width / 2f;
        EditorGUI.indentLevel = 1;

        //  5. 绘制 min 和 max 字段
        EditorGUI.PropertyField(position, property.FindPropertyRelative("m_min"));
        position.x += position.width;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("m_max"));

        EditorGUI.EndProperty();
        EditorGUI.indentLevel = originalIndentLevel;
        EditorGUIUtility.labelWidth = originalLabelWidth;
    }

    #endregion
}