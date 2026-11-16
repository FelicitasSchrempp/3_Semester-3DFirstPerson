using UnityEngine;

public class Lerper
{
    public static float LerpFromTo(float from, float to, float t)
    {
        float value = Mathf.Lerp(from, to, t);
        return value;
    }
}
