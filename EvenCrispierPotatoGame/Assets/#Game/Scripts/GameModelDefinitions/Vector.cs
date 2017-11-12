using System;
using NUnit.Framework.Constraints;

namespace Scripts.GameModelDefinitions
{
    public struct Vector
    {
        public float X;
        public float Y;
        public float SquaredMagnitude => X * X + Y * Y;
        public float Magnitude => (float)Math.Sqrt(SquaredMagnitude);
        public Vector Normalized => new Vector(X/Magnitude, Y/Magnitude);
        
        public Vector(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            return new Vector(v1.X + v2.X, v1.Y + v2.Y);
        }
        
        public static Vector operator -(Vector v1, Vector v2)
        {
            return new Vector(v1.X - v2.X, v1.Y - v2.Y);
        }

        public override string ToString() => $"({X},{Y})";
    }
}