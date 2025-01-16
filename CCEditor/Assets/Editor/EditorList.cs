using UnityEditor;
using UnityEngine;

/// <summary>
/// 绘制列表属性的帮助类
/// </summary>
public static class EditorList
{
    #region 静态工具方法

    /// <summary>
    /// 绘制列表属性
    /// </summary>
    /// <param name="list">列表可序列化属性</param>
    /// <param name="options">绘制显示的选项</param>
    public static void Show(SerializedProperty list, EditorListOption options = EditorListOption.Default)
    {
        // 1. 检查列表是否为数组或列表
        if (!list.isArray)
        {
            EditorGUILayout.HelpBox(list.name + " is neither an array nor a list!", MessageType.Error);
            return;
        }

        bool showListLabel = (options & EditorListOption.ListLabel) != 0;
        bool showListSize = (options & EditorListOption.ListSize) != 0;

        // 2. 绘制标签
        if (showListLabel)
        {
            EditorGUILayout.PrefixLabel(list.displayName);
            EditorGUI.indentLevel += 1;
        }

        // 3. 绘制列表
        if (!showListLabel || list.isExpanded)
        {
            SerializedProperty size = list.FindPropertyRelative("Array.size");
            if (showListSize)
            {
                EditorGUILayout.PropertyField(size);
            }

            if (size.hasMultipleDifferentValues)
            {
                EditorGUILayout.HelpBox("Not showing lists with different sizes.", MessageType.Info);
            }
            else
            {
                ShowElements(list, options);
            }
        }

        if (showListLabel)
        {
            EditorGUI.indentLevel -= 1;
        }
    }

    /// <summary>
    /// 绘制列表的每个元素
    /// </summary>
    /// <param name="list">列表可序列化属性</param>
    /// <param name="options">绘制显示的选项</param>
    private static void ShowElements(SerializedProperty list, EditorListOption options)
    {
        bool showElementLabels = (options & EditorListOption.ElementLabels) != 0;
        bool showButtons = (options & EditorListOption.Buttons) != 0;

        // 1. 循环绘制
        for (int i = 0; i < list.arraySize; i++)
        {
            // 1. 需要显示按钮时，把元素和按钮放在同一行
            if (showButtons)
            {
                EditorGUILayout.BeginHorizontal();
            }

            // 2. 显示元素
            if (showElementLabels)
            {
                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
            }
            else
            {
                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), GUIContent.none);
            }

            if (showButtons)
            {
                ShowButtons(list, i);
                EditorGUILayout.EndHorizontal();
            }
        }

        // 2. 没有元素时，显示单行的添加按钮
        if (showButtons && list.arraySize == 0 && GUILayout.Button(s_addButtonContent, EditorStyles.miniButton))
        {
            list.arraySize += 1;
        }
    }


    /// <summary>
    /// 显示单个元素后面的按钮
    /// </summary>
    /// <param name="list">列表可序列化属性</param>
    /// <param name="index">元素所在的索引</param>
    private static void ShowButtons(SerializedProperty list, int index)
    {
        // 1. 显示下移按钮                                                          
        if (GUILayout.Button(s_moveButtonContent, EditorStyles.miniButtonLeft, s_miniButtonWidth))
        {
            list.MoveArrayElement(index, index + 1);
        }

        // 2. 显示复制按钮
        if (GUILayout.Button(s_duplicateButtonContent, EditorStyles.miniButtonMid, s_miniButtonWidth))
        {
            list.InsertArrayElementAtIndex(index);
        }

        // 3. 显示删除按钮
        if (GUILayout.Button(s_deleteButtonContent, EditorStyles.miniButtonRight, s_miniButtonWidth))
        {
            // 检查删除元素后列表的大小是否保持不变来强制执行元素始终被移除
            int oldSize = list.arraySize;
            list.DeleteArrayElementAtIndex(index);
            if (list.arraySize == oldSize)
            {
                list.DeleteArrayElementAtIndex(index);
            }
        }
    }

    #endregion

    #region 字段

    /// <summary>
    /// 下移按钮显示内容 
    /// </summary>
    private static GUIContent s_moveButtonContent = new GUIContent("\u21b4", "move down");

    /// <summary>
    ///  复制按钮显示内容
    /// </summary>
    private static GUIContent s_duplicateButtonContent = new GUIContent("+", "duplicate");

    /// <summary>
    ///  删除按钮显示内容
    /// </summary>
    private static GUIContent s_deleteButtonContent = new GUIContent("-", "delete");

    /// <summary>
    ///  添加按钮显示内容
    /// </summary>
    private static GUIContent s_addButtonContent = new GUIContent("+", "add element");

    /// <summary>
    ///  按钮宽度
    /// </summary>
    private static GUILayoutOption s_miniButtonWidth = GUILayout.Width(20f);

    #endregion
}