using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class Calculator : EditorWindow
{
    #region 方法

    [MenuItem("UElement例子/计算器")]
    public static void OpenCalculator()
    {
        Calculator wnd = GetWindow<Calculator>();
        wnd.titleContent = new GUIContent("Calculator");
    }

    public void CreateGUI()
    {
        // 1. 加载UXML文件
        VisualElement root = rootVisualElement;
        m_visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Calculator.uxml");
        VisualElement labelFromUxml = m_visualTreeAsset.Instantiate();
        root.Add(labelFromUxml);

        var container = root.Q<VisualElement>("container");
        var label = container.Q<Label>("output");

        // 2. 为按钮添加点击事件
        container.Query<Button>().ForEach((button) =>
        {
            button.clicked += () =>
            {
                HandleButtonClick(button.text);
                label.text = m_showText.ToString();
            };
        });
    }

    /// <summary>
    /// 处理各个按钮的点击事件
    /// </summary>
    /// <param name="buttonText">按钮文本内容</param>
    private void HandleButtonClick(string buttonText)
    {
        switch (buttonText)
        {
            // 1. 数字按钮
            case "1":
                m_showText.Append("1");
                m_operator1 = m_operator1 * 10 + 1;
                break;
            case "2":
                m_showText.Append("2");
                m_operator1 = m_operator1 * 10 + 2;
                break;
            case "3":
                m_showText.Append("3");
                m_operator1 = m_operator1 * 10 + 3;
                break;
            case "4":
                m_showText.Append("4");
                m_operator1 = m_operator1 * 10 + 4;
                break;
            case "5":
                m_showText.Append("5");
                m_operator1 = m_operator1 * 10 + 5;
                break;
            case "6":
                m_showText.Append("6");
                m_operator1 = m_operator1 * 10 + 6;
                break;
            case "7":
                m_showText.Append("7");
                m_operator1 = m_operator1 * 10 + 7;
                break;
            case "8":
                m_showText.Append("8");
                m_operator1 = m_operator1 * 10 + 8;
                break;
            case "9":
                m_showText.Append("9");
                m_operator1 = m_operator1 * 10 + 9;
                break;
            case "0":
                m_showText.Append("0");
                m_operator1 *= 10;
                break;
            // 2. 运算符按钮
            case "+":
                if (m_op != OpCode.None)
                {
                    return;
                }

                m_showText.Append("+");
                m_op = OpCode.Addition;
                m_operator2 = m_operator1;
                m_operator1 = 0;
                break;
            case "-":
                if (m_op != OpCode.None)
                {
                    return;
                }

                m_showText.Append("-");
                m_op = OpCode.Subtract;
                m_operator2 = m_operator1;
                m_operator1 = 0;
                break;
            case "*":
                if (m_op != OpCode.None)
                {
                    return;
                }

                m_showText.Append("*");
                m_op = OpCode.Multiply;
                m_operator2 = m_operator1;
                m_operator1 = 0;
                break;
            case "/":
                if (m_op != OpCode.None)
                {
                    return;
                }

                m_showText.Append("/");
                m_op = OpCode.Divide;
                m_operator2 = m_operator1;
                m_operator1 = 0;
                break;
            // 3. 等号按钮
            case "=":
                switch (m_op)
                {
                    case OpCode.Addition:
                        m_operator1 = m_operator2 + m_operator1;
                        break;
                    case OpCode.Subtract:
                        m_operator1 = m_operator2 - m_operator1;
                        break;
                    case OpCode.Multiply:
                        m_operator1 = m_operator2 * m_operator1;
                        break;
                    case OpCode.Divide:
                        if (m_operator1 == 0)
                        {
                            m_showText.Clear();
                            m_showText.Append(m_operator2);
                            return;
                        }

                        m_operator1 = m_operator2 / m_operator1;
                        m_operator1 = Math.Round(m_operator1, 16);
                        break;
                    case OpCode.None:
                        return;
                }

                m_showText.Clear();
                m_showText.Append(m_operator1);
                m_operator2 = 0;
                m_op = OpCode.None;
                break;
            // 4. 清空按钮
            case "C":
                m_showText.Clear();
                m_operator1 = 0;
                m_operator2 = 0;
                m_op = OpCode.None;
                break;
        }
    }

    #endregion

    #region 字段

    /// <summary>
    /// 操作数1
    /// </summary>
    private decimal m_operator1 = 0;

    /// <summary>
    /// 操作数2
    /// </summary>
    private decimal m_operator2 = 0;

    /// <summary>
    /// 操作符
    /// </summary>
    private OpCode m_op = OpCode.None;

    // 显示内容缓存 
    private StringBuilder m_showText = new StringBuilder();

    /// <summary>
    /// xml资源
    /// </summary>
    [SerializeField]
    private VisualTreeAsset m_visualTreeAsset = default;

    #endregion
}