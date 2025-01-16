using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Calculator : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("UI Toolkit/Calculator")]
    public static void ShowExample()
    {
        Calculator wnd = GetWindow<Calculator>();
        wnd.titleContent = new GUIContent("Calculator");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
    }
}
