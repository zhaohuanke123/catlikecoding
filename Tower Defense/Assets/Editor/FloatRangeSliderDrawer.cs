using UnityEditor;
using UnityEngine;

/// <summary>
///  FloatRange 自定义属性绘制器, 用在ColorRangeHSV.cs中 
/// </summary>
[CustomPropertyDrawer(typeof(FloatRangeSliderAttribute))]
public class FloatRangeSliderDrawer : PropertyDrawer
{
    #region Unity 生命周期

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 1. 开始绘制该属性，并传入矩形区域、标签和属性
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        
        // 2. 获取 min 和 max 属性
        SerializedProperty minProperty = property.FindPropertyRelative("m_min");
        SerializedProperty maxProperty = property.FindPropertyRelative("m_max");

        // 3. 保存和回复原始值，确保撤销和重做支持
        float minValue = minProperty.floatValue;
        float maxValue = maxProperty.floatValue;

        // 4. 计算字段宽度和滑块宽度
        float fieldWidth = position.width / 4f - 4f;
        float sliderWidth = position.width / 2f;

        // 5. 设置 min 值的绘制区域并绘制 FloatField
        position.width = fieldWidth;
        minValue = EditorGUI.FloatField(position, minValue);

        // 6. 移动到滑块的绘制区域
        position.x += fieldWidth + 4f;
        position.width = sliderWidth;

        // 7. 绘制 MinMaxSlider
        FloatRangeSliderAttribute limit = attribute as FloatRangeSliderAttribute;
        // EditorGUI.MinMaxSlider(position, label, ref minValue, ref maxValue, limit.Min, limit.Max); // 如果需要标签
        EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, limit.Min, limit.Max);

        // 8. 绘制 max 值的 FloatField
        position.x += sliderWidth + 4f;
        position.width = fieldWidth;
        maxValue = EditorGUI.FloatField(position, maxValue);

        // 9. 限制 minValue 和 maxValue 在合理的范围内
        if (minValue < limit.Min)
        {
            minValue = limit.Min;
        }
        else if (minValue > limit.Max)
        {
            minValue = limit.Max;
        }

        if (maxValue < minValue)
        {
            maxValue = minValue;
        }
        else if (maxValue > limit.Max)
        {
            maxValue = limit.Max;
        }

        minProperty.floatValue = minValue;
        maxProperty.floatValue = maxValue;

        EditorGUI.EndProperty();
    }

    #endregion
}