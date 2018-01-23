using GameEngine.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Mock
{
    public abstract class MockEntity : IEntity
    {
        
        public abstract string BodyType { get; }
        private int _id;
        public int Id => _id;

        private IWorld _world;

        private Vector2 _position;
        public Vector2 Position => _position;

        public float XPos => _position.X;
        public float YPos => _position.Y;

        private Dictionary<string, EntityVariable> _descriptions = new Dictionary<string, EntityVariable>();
        public IList<EntityVariable> Variables => _descriptions.Values.ToList();

        private Vector2 _direction;

        public event Action UpdatePosition;

        public virtual void Update()
        {
        }

        private bool CloseToEdge(Vector2 pos) => pos.X < 1 || pos.Y < 1 || pos.X > _world.Dimensions.X - 1 || pos.Y > _world.Dimensions.Y - 1;

        private MockEntity()
        {
            AddDescription("Id", () => Id);
            AddDescription("Health", () => 100);
            AddDescription("XPos", () => XPos);
            AddDescription("YPos", () => YPos);
        }

        private void AddDescription(string name, EntityVariable.DescriptionAccessor accessor)
        {
            _descriptions[name] = new EntityVariable(name, accessor);
        }

        private class Animal : MockEntity
        {
            public override string BodyType => "Animal";

            public override void Update()
            {
                UpdatePosition?.Invoke();
                bool edge;
                var nextPos = _position;
                do
                {
                    nextPos = _position + _direction * _world.DistanceScale;
                    edge = CloseToEdge(nextPos);
                    if (edge)
                        _direction = Vector2.Transform(Vector2.UnitX, Matrix3x2.CreateRotation((float)(_world.Rng.NextDouble() * Math.PI * 2)));
                }
                while (edge);
                _position = nextPos;
            }
        }

        private class Plant : MockEntity
        {
            public override string BodyType => "Plant";
        }

        public class MockEntityFactory : EntityFactory
        {
            static MockEntityFactory()
            {

            }

            protected override IEntity MakeEntity(int id, IWorld world, object[] args)
            {
                if (args.Length == 0)
                    throw new ArgumentException();
                switch(args[0] as string)
                {
                    case "Animal":
                        return new Animal()
                        {
                            _id = id,
                            _world = world,
                            _position = new Vector2((float)(1 + world.Rng.NextDouble() * (world.Dimensions.X - 2)), (float)(1 + world.Rng.NextDouble() * (world.Dimensions.Y - 2))),
                            _direction = Vector2.Transform(Vector2.UnitX, Matrix3x2.CreateRotation((float)(world.Rng.NextDouble() * Math.PI * 2)))
                        };
                    case "Plant":
                        return new Plant()
                        {
                            _id = id,
                            _world = world,
                            _position = new Vector2((float)(1 + world.Rng.NextDouble() * (world.Dimensions.X - 2)), (float)(1 + world.Rng.NextDouble() * (world.Dimensions.Y - 2))),
                            _direction = Vector2.Transform(Vector2.UnitX, Matrix3x2.CreateRotation((float)(world.Rng.NextDouble() * Math.PI * 2)))
                        };
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            }
        }
    }
}
