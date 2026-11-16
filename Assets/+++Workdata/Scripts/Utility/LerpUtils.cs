using UnityEngine;

public static class LerpUtils
{
    public static float Lerp(float a, float b, float t) => a + (b - a) * Mathf.Clamp01(t);
    /*
    public static float Lerp(float a, float b, float t)
    {
        t = Mathf.Clamp01(t);
        float diff = b - a;
        return a + diff * t;
    }*/
    
    
}
