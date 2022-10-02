using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelper
{
    public static Vector3 Damping(Vector3 src, Vector3 dst, float dt, float factor)
    {
        return ((src * factor) + (dst * dt)) / (factor + dt);
    }

    public static float Damping(float src, float dst, float dt, float factor)
    {
        return ((src * factor) + (dst * dt)) / (factor + dt);
    }
}
