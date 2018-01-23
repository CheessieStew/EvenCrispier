using GameEngine.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Simple
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

        public int Health { get; protected set; }
        public int MaxHealth { get; protected set; }

        
        private Entity()
        {
            AddDescription("Id", () => Id);
            AddDescription("Health", () => Health);
            AddDescription("MaxHealth", () => MaxHealth);
            AddDescription("XPos", () => XPos);
            AddDescription("YPos", () => YPos);
        }

        private void AddDescription(string name, EntityVariable.DescriptionAccessor accessor)
        {
            _descriptions[name] = new EntityVariable(name, accessor);
        }



        protected virtual void Die()
        {
            Alive = false;
        }

        public abstract void Update();


        private EntityDescription GetDescription() => new EntityDescription()
        {
            BodyType = this.IntBodyType,
            Species = (this is Animal a) ? a.Species.SpeciesId : 0,
            Id = this.Id,
            State = Alive ? 1 : 0,
            Health = Math.Max(0, ((float)Health) / MaxHealth),
            PositionX = XPos,
            PositionY = YPos,
        };


        public class EntityDescription
        {
            public int BodyType;
            public int Species;
            public int Id;
            public int State;
            public float Health;
            public float PositionX;
            public float PositionY;
        }

        protected abstract int GetEaten(int biteSize, Entity e);

        private bool TakeDamage(int dmg, Entity e)
        {
            Health -= dmg;
            return true;
        }

        public class SimpleEntityFactory : EntityFactory
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
                        if (!(args[2] is AnimalSpecies species))
                            throw new InvalidOperationException();
                        if (!(args[3] is Vector2 position))
                            throw new InvalidOperationException();
                        return new Animal()
                        {
                            _id = id,
                            _world = w,
                            Vanish = vanish,
                            Species = species,
                            _position = position
                        };
                    case "Plant":
                        if (!(args[2] is int regrowthRate))
                            throw new InvalidOperationException();
                        if (!(args[3] is int mass))
                            throw new InvalidOperationException();
                        return new Plant()
                        {
                            MaxHealth = 100,
                            Health = 100,
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
