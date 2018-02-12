using GameEngine.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameEngine.Mock
{
    public class MockWorld : IWorld
    {
        private IDictionary<int, IEntity> _entities = new Dictionary<int, IEntity>();
        public IDictionary<int, IEntity> Entities => _entities;

        private Random _rng = new Random();
        public Random Rng => _rng;


        private Vector2 _dimensions = new Vector2(555, 555);
        public Vector2 Dimensions => _dimensions;
        public float Height => _dimensions.X;
        public float Width => _dimensions.Y;

        public float DistanceScale => 22f;
        private int _turnCounter;
        public int TurnCounter => _turnCounter;


        public event EntityEventHandler NewEntity;
        public event EntityEventHandler EntityVanished;

        private EntityFactory _factory;

        public MockWorld(EntityFactory factory)
        {
            _factory = factory;
        }

        private void CreateEntity(string type)
        {
            var newEntity = _factory.GetEntity(this, type);
            _entities[newEntity.Id] = newEntity;
            NewEntity?.Invoke(newEntity);
        }

        private void VanishEntity(IEntity e)
        {
            _entities.Remove(e.Id);
            EntityVanished?.Invoke(e);
        }


        public void Initialize()
        {
            for (int i = 0; i < 60; i++)
            {
                CreateEntity("Plant");
            }
        }

        public void NextFrame()
        {           
            _turnCounter++;

            foreach (var entity in _entities.Values)
            {
                entity.Update();
            }
            if (_turnCounter % 25 == 0)
            {
                VanishEntity(Entities[Entities.Keys.ElementAt(Rng.Next() % Entities.Count)]);
            }
            if (_turnCounter % 15 == 0)
            {
                CreateEntity("Animal");
            }
        }
    }
}
