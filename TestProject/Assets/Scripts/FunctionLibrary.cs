using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityEngine.Mathf;

/// <summary>
/// 图像函数库工具类
/// </summary>
public static class FunctionLibrary
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    private sealed class FunLibAttribute : Attribute
    {
    }

    /// <summary>
    ///  图形函数委托
    /// </summary>
    private delegate Vector3 Function(float u, float v, float t);

    /// <summary>
    ///  图形函数列表
    /// </summary>
    // static List<Function> m_functions;
    private static List<Function> m_functions;

    /// <summary>
    ///  图形函数名称列表
    /// </summary>
    private static string[] m_functionNames;

    static FunctionLibrary()
    {
        var type = typeof(FunctionLibrary);

        // 获取打有特性的方法
        var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
        m_functions = new List<Function>();
        List<string> methodNames = new List<string>();
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

    // public static float Wave(float x, float z, float t)
    // {
    //     return Sin(PI * (x + z + t));
    // }
    //
    // public static float MultiWave(float x, float z, float t)
    // {
    //     float y = Sin(PI * (x + 0.5f * t));
    //     y += 0.5f * Sin(2f * PI * (z + t));
    //     // return y * (2f / 3f);
    //     y += Sin(PI * (x + z + 0.25f * t));
    //     return y * (1f / 2.5f);
    // }
    //
    // public static float Ripple(float x, float z, float t)
    // {
    //     // float d = Abs(x);
    //     // float y = Sin(4f * PI * d - t);
    //     // return y / (1f + 10f * d);
    //     float d = Sqrt(x * x + z * z);
    //     float y = Sin(PI * (4f * d - t));
    //     return y / (1f + 10f * d);
    // }

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

    /// <summary>
    ///  圆锥
    ///  </summary>
    [FunLib]
    private static Vector3 CylinderWithCollapsingRadius(float u, float v, float t)
    {
        float r = Cos(0.5f * PI * v);
        Vector3 p;
        p.x = r * Sin(PI * u);
        p.y = v;
        p.z = r * Cos(PI * u);
        return p;
    }

    /// <summary>
    ///  球体
    /// </summary>
    [FunLib]
    private static Vector3 Sphere(float u, float v, float t)
    {
        float r = Cos(0.5f * PI * v);
        Vector3 p;
        p.x = r * Sin(PI * u);
        p.y = Sin(PI * 0.5f * v);
        p.z = r * Cos(PI * u);
        return p;
    }

    /// <summary>
    ///  球体扰动
    ///  </summary>
    [FunLib]
    private static Vector3 SpherePerturbing(float u, float v, float t)
    {
        float r = 0.5f + 0.5f * Sin(PI * t);
        float s = r * Cos(0.5f * PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin(0.5f * PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }

    /// <summary>
    ///  球体垂直带
    ///  </summary>
    [FunLib]
    private static Vector3 SphereWithVerticalBands(float u, float v, float t)
    {
        float r = 0.9f + 0.1f * Sin(8f * PI * u);
        float s = r * Cos(0.5f * PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin(0.5f * PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }

    /// <summary>
    ///  球体水平带 
    ///  </summary>
    [FunLib]
    private static Vector3 SphereWithHorizontalBands(float u, float v, float t)
    {
        float r = 0.9f + 0.1f * Sin(8f * PI * v);
        float s = r * Cos(0.5f * PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin(0.5f * PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }

    /// <summary>
    ///  球体旋转扭曲
    ///  </summary>
    [FunLib]
    private static Vector3 SphereWithRotatingTwisted(float u, float v, float t)
    {
        float r = 0.9f + 0.1f * Sin(PI * (6f * u + 4f * v + t));
        float s = r * Cos(0.5f * PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin(0.5f * PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }

    /// <summary>
    /// 拉开的球体 
    /// </summary>
    [FunLib]
    private static Vector3 SpherePulledApart(float u, float v, float t)
    {
        float r = 1f;
        float s = 0.5f + r * Cos(0.5f * PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin(0.5f * PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }

    /// <summary>
    /// 自交纺锤环面
    /// </summary>
    [FunLib]
    private static Vector3 SelfIntersectingSpindleTorus(float u, float v, float t)
    {
        float r = 1f;
        float s = 0.5f + r * Cos(PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin(PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }

    /// <summary>
    /// 圆环环面
    /// </summary>
    [FunLib]
    private static Vector3 RingTorus
        (float u, float v, float t)
    {
        float r1 = 0.75f;
        float r2 = 0.25f;
        float s = r1 + r2 * Cos(PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r2 * Sin(PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }

    /// <summary>
    /// 扭曲环面
    /// </summary>
    [FunLib]
    private static Vector3 TwistingTorus(float u, float v, float t)
    {
        float r1 = 0.7f + 0.1f * Sin(PI * (6f * u + 0.5f * t));
        float r2 = 0.15f + 0.05f * Sin(PI * (8f * u + 4f * v + 2f * t));
        float s = r1 + r2 * Cos(PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r2 * Sin(PI * v);
        p.z = s * Cos(PI * u);

        return p;
    }

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

    /// <summary>
    ///  获取函数名称列表
    /// </summary>
    public static string[] GetFunctionNames()
    {
        return m_functionNames;
    }
}