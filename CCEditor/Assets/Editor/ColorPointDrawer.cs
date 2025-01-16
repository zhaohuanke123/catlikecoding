using UnityEditor;
using UnityEngine;

/// <summary>
/// ColorPoint 自定义属性绘制。
/// </summary>
[CustomPropertyDrawer(typeof(ColorPoint))]
public class ColorPointDrawer : PropertyDrawer
{
    #region Unity 生命周期

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        int oldIndentLevel = EditorGUI.indentLevel;
        // 1. 使用Begin End 创建一个Warrper，使得支持一个属性块的Copy和Paste
        label = EditorGUI.BeginProperty(position, label, property);

        // 2. 用于绘制标签
        Rect contentPosition = EditorGUI.PrefixLabel(position, label);

        // 3. 页面太窄时，显示到下一行
        if (position.height > 16f)
        {
            position.height = 16f;
            EditorGUI.indentLevel += 1;
            contentPosition = EditorGUI.IndentedRect(position);
            contentPosition.y += 18f;
        }

        // 4. 绘制属性
        contentPosition.width *= 0.75f;
        EditorGUI.indentLevel = 0;
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("m_position"), GUIContent.none);

        contentPosition.x += contentPosition.width;
        contentPosition.width /= 3f;
        EditorGUIUtility.labelWidth = 14f;
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("m_color"), new GUIContent("C"));
        EditorGUI.EndProperty();
        EditorGUI.indentLevel = oldIndentLevel;
    }

    #endregion

    #region 方法

    /// <summary>
    /// 计算并返回属性抽屉的高度。
    /// 此方法根据屏幕宽度动态调整高度，以优化不同屏幕尺寸下的显示效果。
    /// </summary>
    /// <param name="property">属性抽屉中正在绘制的序列化属性对象。</param>
    /// <param name="label">属性的GUI内容标签。</param>
    /// <returns>计算得到的属性抽屉高度，单位为像素。</returns>
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return label != GUIContent.none && Screen.width < 333 ? (16f + 18f) : 16f;
    }

    #endregion
}