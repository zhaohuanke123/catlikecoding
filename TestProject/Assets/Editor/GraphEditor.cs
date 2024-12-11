namespace Editor
{
    using UnityEditor;

    [CustomEditor(typeof(Graph))]
    public class GraphEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // 根据FunctionLibrary的funcName的List，创建下拉列表，选择调整Graph 选择的函数索引
            var graph = target as Graph;
            graph.m_functionIndex =
                EditorGUILayout.Popup("Function", graph.m_functionIndex, FunctionLibrary.GetFunctionNames());
        }
    }
}