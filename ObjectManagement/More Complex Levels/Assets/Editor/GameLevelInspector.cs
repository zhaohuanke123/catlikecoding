using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameLevel))]
public class GameLevelInspector : Editor
{
    #region Unity 生命周期

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // 1. 获取目标对象
        var gameLevel = (GameLevel)target;
        // 2. 有缺失的Level对象弹出警告
        if (gameLevel.HasMissingLevelObjects)
        {
            EditorGUILayout.HelpBox("Missing level objects!", MessageType.Error);
        }

        // 3. 移除缺失的Level对象按钮
        if (GUILayout.Button("Remove Missing Elements"))
        {
            gameLevel.RemoveMissingLevelObjects();
        }
    }

    #endregion
}