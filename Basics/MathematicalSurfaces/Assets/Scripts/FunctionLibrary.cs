using static UnityEngine.Mathf;

public static class FunctionLibrary
{
    public delegate float Function(float x, float z, float t);

    static Function[] functions = { Wave, MultiWave, Ripple };

    public enum FunctionName
    {
        Wave,
        MultiWave,
        Ripple
    }

    public static float Wave(float x, float z, float t)
    {
        return Sin(PI * (x + z + t));
    }

    public static float MultiWave(float x, float z, float t)
    {
        float y = Sin(PI * (x + 0.5f * t));
        y += 0.5f * Sin(2f * PI * (z + t));
        // return y * (2f / 3f);
        y += Sin(PI * (x + z + 0.25f * t));
        return y * (1f / 2.5f);
    }

    public static float Ripple(float x, float z, float t)
    {
        // float d = Abs(x);
        // float y = Sin(4f * PI * d - t);
        // return y / (1f + 10f * d);
        float d = Sqrt(x * x + z * z);
        float y = Sin(PI * (4f * d - t));
        return y / (1f + 10f * d);
    }

    public static Function GetFunction(FunctionName name)
    {
        return functions[(int)name];
    }
}