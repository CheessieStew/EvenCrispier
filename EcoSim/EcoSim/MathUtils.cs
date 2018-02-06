
using System;
using System.Numerics;

internal static class MathUtils
{
    public static double ToDegrees(this float a) => a * 180 / Math.PI;

    public static float VectorAngle(this Vector2 v)
    {
        var res = (Math.Atan2(v.Y, v.X));
        return (float)(res < 0 ? res + Math.PI * 2 : res);
    }

}