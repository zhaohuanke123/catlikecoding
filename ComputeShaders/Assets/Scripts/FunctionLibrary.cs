using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityEngine.Mathf;
using Random = UnityEngine.Random;


/// <summary>
///  图形函数委托
/// </summary>
public delegate Vector3 Function(float u, float v, float t);

/// <summary>
/// 图像函数库工具类
/// </summary>
public static class FunctionLibrary
{
    #region 嵌套类型

    /// <summary>
    ///  图形函数特性, 标志表示该方法是图形函数 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    private sealed class FunLibAttribute : Attribute
    {
    }

    #endregion

    #region 方法

    /// <summary>
    ///  平滑过渡
    /// </summary>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <param name="t"> 进度 </param>
    /// <param name="fromIndex"> 起始函数索引 </param>
    /// <param name="toIndex"> 目标函数索引 </param>
    /// <param name="progress">  进度 </param>
    /// <returns> 返回过渡后的函数值 </returns>
    public static Vector3 Morph(
        float u, float v, float t, int fromIndex, int toIndex, float progress
    )
    {
        return Vector3.LerpUnclamped(GetFunction(fromIndex)(u, v, t), GetFunction(toIndex)(u, v, t),
            SmoothStep(0f, 1f, progress));
    }

    /// <summary>
    ///  随机获取函数索引, 保证不与当前索引相同
    /// </summary>
    ///  <param name="index">当前索引</param>
    /// <returns>函数索引</returns>
    public static int GetRandomFunctionIndex(int index)
    {
        var choice = Random.Range(1, m_functionNames.Length);
        return choice == index ? 0 : choice;
    }

    /// <summary>
    ///  根据索引值获取下一个函数索引
    /// </summary>
    /// <param name="index">索引值</param>
    /// <returns></returns>
    public static int GetNextFunctionIndex(int index)
    {
        if (index >= m_functionNames.Length - 1)
        {
            return 0;
        }

        return index + 1;
    }

    /// <summary>
    ///  获取函数名称列表
    /// </summary>
    public static string[] GetFunctionNames()
    {
        return m_functionNames;
    }

    static FunctionLibrary()
    {
        var type = typeof(FunctionLibrary);

        // 1. 获取所有的方法
        var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
        m_functions = new List<Function>();
        List<string> methodNames = new List<string>();

        // 2. 遍历方法，获取打有特性的方法
        foreach (var method in methods)
        {
            if (method.IsDefined(typeof(FunLibAttribute)))
            {
                methodNames.Add(method.Name);
                m_functions.Add((Function)Delegate.CreateDelegate(typeof(Function), method));
            }
        }

        m_functionNames = methodNames.ToArray();
    }

    #region 各类函数图形

    /// <summary>
    ///  对角波
    /// </summary>
    [FunLib]
    private static Vector3 Wave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + v + t));
        p.z = v;
        return p;
    }

    /// <summary>
    ///  x z 组合波
    ///  </summary>
    [FunLib]
    private static Vector3 MultiWave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + 0.5f * t));
        p.y += 0.5f * Sin(2f * PI * (v + t));
        p.y += Sin(PI * (u + v + 0.25f * t));
        p.y *= 1f / 2.5f;
        p.z = v;
        return p;
    }

    /// <summary>
    /// 对角线中心波 
    /// </summary>
    [FunLib]
    private static Vector3 Ripple(float u, float v, float t)
    {
        float d = Sqrt(u * u + v * v);
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (4f * d - t));
        p.y /= 1f + 10f * d;
        p.z = v;
        return p;
    }

    /// <summary>
    ///  圆形
    /// </summary>
    [FunLib]
    private static Vector3 Circle(float u, float v, float t)
    {
        Vector3 p;
        p.x = Sin(PI * u);
        p.y = 0f;
        p.z = Cos(PI * u);
        return p;
    }

    /// <summary>
    ///  圆柱
    /// </summary>
    [FunLib]
    private static Vector3 Cylinder(float u, float v, float t)
    {
        Vector3 p;
        p.x = Sin(PI * u);
        p.y = v;
        p.z = Cos(PI * u);
        return p;
    }

    #endregion

    /// <summary>
    ///  根据索引获取函数
    /// </summary>
    /// <param name="funcIndex"></param>
    /// <returns>  函数委托 </returns>
    private static Function GetFunction(int funcIndex)
    {
        return m_functions[funcIndex];
    }

    /// <summary>
    ///  获取函数值
    /// </summary>
    /// <returns> 函数值 </returns>
    public static Vector3 GetFunctionValue(int funcIndex, float u, float v, float t)
    {
        return GetFunction(funcIndex)(u, v, t);
    }

    #endregion

    #region 属性

    public static int FunctionCount => m_functionNames.Length;

    #endregion

    #region 内部字段

    /// <summary>
    ///  函数列表, 通过反射填充
    /// </summary>
    // static List<Function> m_functions;
    private static List<Function> m_functions;

    /// <summary>
    ///  函数名称列表, 通过反射填充名字，给编辑器使用
    /// </summary>
    private static string[] m_functionNames;

    #endregion
}