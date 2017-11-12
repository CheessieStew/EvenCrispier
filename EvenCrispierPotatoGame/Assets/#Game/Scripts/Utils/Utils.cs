using Scripts.GameModelDefinitions;
using UnityEngine;

namespace Scripts.Utils
{
    public static class Utils
    {
        public static Vector3 ToVector3(this Vector v) => new Vector3(v.X, v.Y);
    }
}