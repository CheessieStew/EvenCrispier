using GameEngine.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Grazing
{
    public abstract partial class Entity : IEntity
    {

        public abstract int IntBodyType { get; }
        public Action<IEntity> Vanish;
        public bool Alive = true;
        public abstract string BodyType { get; }
        private int _id;
        public int Id => _id;

        private World _world;

        private Vector2 _position;
        public Vector2 Position => _position;

        public float XPos => _position.X;
        public float YPos => _position.Y;

        private Dictionary<string, EntityVariable> _descriptions = new Dictionary<string, EntityVariable>();

        public event Action UpdatePosition;

        public IList<EntityVariable> Variables => _descriptions.Values.ToList();

        public virtual int Mass { get; protected set; }
        public virtual int MaxMass { get; protected set; }
        public abstract string Name { get; }

        private Entity()
        {
            AddDescription("Id", () => Id);
            AddDescription("MaxMass", () => MaxMass);
            AddDescription("XPos", () => XPos);
            AddDescription("YPos", () => YPos);
            AddDescription("MassLeft", () => Mass);
        }

        private void AddDescription(string name, EntityVariable.DescriptionAccessor accessor)
        {
            _descriptions[name] = new EntityVariable(name, accessor);
        }



        protected virtual void Die()
        {
            Alive = false;
            Vanish?.Invoke(this);
        }

        public abstract void Update();


        private EntityDescription GetDescription() => new EntityDescription()
        {
            BodyType = this.IntBodyType,
            Id = this.Id,
            AvailableMass = this.Mass,
            Position = _position,
            PositionX = XPos,
            PositionY = YPos,
        };


        public class EntityDescription
        {
            public int BodyType;
            public int Id;
            public int AvailableMass;
            public Vector2 Position;
            public float PositionX;
            public float PositionY;
        }

        internal abstract void Kill();


        protected abstract int GetEaten(int biteSize, Entity e);

        public class GrazingEntityFactory : EntityFactory
        {
            protected override IEntity MakeEntity(int id, IWorld world, object[] args)
            {
                if (args.Length < 2)
                    throw new ArgumentException();

                if(!(world is World w))
                    throw new InvalidOperationException();
                if (!(args[0] is string type))
                    throw new InvalidOperationException();
                if (!(args[1] is Action<IEntity> vanish))
                    throw new InvalidOperationException();
                switch (type)
                {
                    case "Animal":
                        if (!(args[2] is Vector2 position))
                            throw new InvalidOperationException();
                        if (!(args[3] is IBrain brain))
                            throw new InvalidOperationException();
                        return new Animal()
                        {
                            
                            Mass = w.Settings.StomachCapacity / 2,
                            _id = id,
                            _world = w,
                            Vanish = vanish,
                            _position = position,
                            Brain = brain
                        };
                    case "Plant":
                        if (!(args[2] is int mass))
                            throw new InvalidOperationException();
                        if (!(args[3] is int regrowthRate))
                            throw new InvalidOperationException();
                        return new Plant()
                        {
                            MaxMass = mass,
                            Mass = mass,
                            RegrowthRate = regrowthRate,
                            Vanish = vanish,
                            _id = id,
                            _world = w,
                            _position = new Vector2((float)(1 + world.Rng.NextDouble() * (world.Dimensions.X - 2)), (float)(1 + world.Rng.NextDouble() * (world.Dimensions.Y - 2))),
                        };
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
        }
    
}
}
