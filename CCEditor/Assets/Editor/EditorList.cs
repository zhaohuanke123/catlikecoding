using UnityEditor;
using UnityEngine;

public static class EditorList
{
    #region 方法

    public static void Show(SerializedProperty list, EditorListOption options = EditorListOption.Default)
    {
        if (!list.isArray)
        {
            EditorGUILayout.HelpBox(list.name + " is neither an array nor a list!", MessageType.Error);
            return;
        }

        bool showListLabel = (options & EditorListOption.ListLabel) != 0,
            showListSize = (options & EditorListOption.ListSize) != 0;
        if (showListLabel)
        {
            EditorGUILayout.PropertyField(list);
            EditorGUI.indentLevel += 1;
        }

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

    private static void ShowElements(SerializedProperty list, EditorListOption options)
    {
        bool showElementLabels = (options & EditorListOption.ElementLabels) != 0,
            showButtons = (options & EditorListOption.Buttons) != 0;
        ;
        for (int i = 0; i < list.arraySize; i++)
        {
            if (showButtons)
            {
                EditorGUILayout.BeginHorizontal();
            }

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

        if (showButtons && list.arraySize == 0 && GUILayout.Button(s_addButtonContent, EditorStyles.miniButton))
        {
            list.arraySize += 1;
        }
    }


    private static void ShowButtons(SerializedProperty list, int index)
    {
        if (GUILayout.Button(s_moveButtonContent, EditorStyles.miniButtonLeft, s_miniButtonWidth))
        {
            list.MoveArrayElement(index, index + 1);
        }

        if (GUILayout.Button(s_duplicateButtonContent, EditorStyles.miniButtonMid, s_miniButtonWidth))
        {
            list.InsertArrayElementAtIndex(index);
        }

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

    #region 事件

    #endregion

    #region 属性

    #endregion

    #region 字段

    /// <summary>
    /// 
    /// </summary>
    private static GUIContent s_moveButtonContent = new GUIContent("\u21b4", "move down");

    /// <summary>
    /// 
    /// </summary>
    private static GUIContent s_duplicateButtonContent = new GUIContent("+", "duplicate");

    /// <summary>
    /// 
    /// </summary>
    private static GUIContent s_deleteButtonContent = new GUIContent("-", "delete");

    /// <summary>
    /// 
    /// </summary>
    private static GUIContent s_addButtonContent = new GUIContent("+", "add element");

    /// <summary>
    /// 
    /// </summary>
    private static GUILayoutOption s_miniButtonWidth = GUILayout.Width(20f);

    #endregion
}