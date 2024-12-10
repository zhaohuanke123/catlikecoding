using UnityEngine;
using static UnityEngine.Mathf;

public static class FunctionLibrary
{
    public delegate Vector3 Function(float u, float v, float t);

    static Function[] functions = { Wave, MultiWave, Ripple, Sphere };


    public enum FunctionName
    {
        Wave,
        MultiWave,
        Ripple,
        Sphere
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
    public static Vector3 Wave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + v + t));
        p.z = v;
        return p;
    }

    public static Vector3 MultiWave(float u, float v, float t)
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

    public static Vector3 Ripple(float u, float v, float t)
    {
        float d = Sqrt(u * u + v * v);
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (4f * d - t));
        p.y /= 1f + 10f * d;
        p.z = v;
        return p;
    }

    private static Vector3 Sphere(float u, float v, float t)
    {
        Vector3 p;
        p.x = Sin(PI * u);
        p.y = 0f;
        p.z = Cos(PI * u);
        return p;
    }

    public static Function GetFunction(FunctionName name)
    {
        return functions[(int)name];
    }
}