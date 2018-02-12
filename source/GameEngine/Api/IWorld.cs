using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameEngine.Api
{
    public delegate void EntityEventHandler(IEntity e);

    public interface IWorld
    {

        event EntityEventHandler NewEntity;
        event EntityEventHandler EntityVanished;

        IDictionary<int, IEntity> Entities { get; }
        Random Rng { get; }
        Vector2 Dimensions { get; }
        float Height { get; }
        float Width { get; }
        float DistanceScale { get; }
        int TurnCounter { get; }

        void NextFrame();
        void Initialize();
    }
}
