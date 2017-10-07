using System;

namespace Scripts.GameModel.AbstractGame.AiDefinitions
{    
    public interface IWorld
    {
        Vector Dimensions { get; }
    }

    public struct Vector
    {
        public float X;
        public float Y;
        public float SquaredMagnitude => X * X + Y * Y;
        public float Magnitude => (float)Math.Sqrt(SquaredMagnitude);
    }
}