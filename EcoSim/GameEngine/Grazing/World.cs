using GameEngine.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Grazing
{
    public class World : IWorld
    {
        public Settings Settings;
        private IDictionary<int, Entity> _entities = new Dictionary<int, Entity>();
        public IDictionary<int, IEntity> Entities => new WrapDictionary(_entities);
        private class WrapDictionary : IDictionary<int, IEntity>
        {
            public WrapDictionary(IDictionary<int, Entity> inner)
            {
                _inner = inner;
            }

            private IDictionary<int, Entity> _inner;

            public IEntity this[int key] { get => _inner[key]; set => throw new InvalidOperationException(); }

            public ICollection<int> Keys => _inner.Keys;

            public ICollection<IEntity> Values => (ICollection<IEntity>)_inner.Values;

            public int Count => _inner.Count;

            public bool IsReadOnly => _inner.IsReadOnly;

            public void Add(int key, IEntity value)
            {
                throw new InvalidOperationException();
            }

            public void Add(KeyValuePair<int, IEntity> item)
            {
                throw new InvalidOperationException();
            }

            public void Clear()
            {
                _inner.Clear();
            }

            public bool Contains(KeyValuePair<int, IEntity> item)
            {
                throw new InvalidOperationException();
            }

            public bool ContainsKey(int key)
            {
                return _inner.ContainsKey(key);
            }

            public void CopyTo(KeyValuePair<int, IEntity>[] array, int arrayIndex)
            {
                throw new InvalidOperationException();
            }

            public IEnumerator<KeyValuePair<int, IEntity>> GetEnumerator()
            {
                foreach (var kvp in _inner)
                    yield return new KeyValuePair<int, IEntity>(kvp.Key, kvp.Value);
            }

            public bool Remove(int key)
            {
                return _inner.Remove(key);
            }

            public bool Remove(KeyValuePair<int, IEntity> item)
            {
                throw new InvalidOperationException();
            }

            public bool TryGetValue(int key, out IEntity value)
            {
                throw new InvalidOperationException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _inner.GetEnumerator();
            }
        }
        

        private Random _rng = new Random();
        public Random Rng => _rng;


        private Vector2 _dimensions = new Vector2(500, 500);
        public Vector2 Dimensions => _dimensions;
        public float Height => _dimensions.X;
        public float Width => _dimensions.Y;

        public float DistanceScale => Settings.MovementSpeed;
        private int _turnCounter;
        public int TurnCounter => _turnCounter;
        private int _plantCount;

        public event EntityEventHandler NewEntity;
        public event EntityEventHandler EntityVanished;

        private EntityFactory _factory;

        private Func<IBrain> _brainFac;

        public World(EntityFactory factory, Settings settings, Func<IBrain> brainFac)
        {
            Settings = settings;
            _brainFac = brainFac;
            _factory = factory;
        }

        private float Roll(float min, float max)
        {
            return (float)Rng.NextDouble() * (max - min) + min;
        }

        private void CreatePlant()
        {
            _plantCount++;
            var mass = (int)Roll(Settings.MinPlantMass, Settings.MaxPlantMass);
            var growthRate = (int)Roll(Settings.MinGrowthRate, Settings.MaxGrowthRate);
            var newEntity = _factory.GetEntity(this, "Plant",
                (Action<IEntity>)(plnt =>
            {
                VanishEntity(plnt);
                _plantCount--;
            })
            , mass, growthRate);
            if (!(newEntity is Entity e))
                throw new InvalidOperationException();
            _entities[newEntity.Id] = e;
            NewEntity?.Invoke(newEntity);
        }
        

        public void KillAnimal()
        {
            _animal.Kill();
            Log($"killed by user");
        }

        public void ResetCounter()
        {
            _turnCounter = 0;
            Log($"turn counter set to 0");

        }
        
        public event Action<string> Log;
        private Entity _animal;

        private void CreateAnimal()
        {
            var newEntity = _factory.GetEntity(this, 
                "Animal",
                (Action<IEntity>)(animal =>
                {

                    var age = animal.Variables.First(v => v.Name == "Age").Value;
                    Log($"{TurnCounter}\t{age}");
                    VanishEntity(animal);
                    _brainFac().NewGame();
                    Initialize();
                })
                ,
                new Vector2((float)(1 + Rng.NextDouble() * (Dimensions.X - 2)), (float)(1 + Rng.NextDouble() * (Dimensions.Y - 2))), _brainFac());
            if (!(newEntity is Entity e))
                throw new InvalidOperationException();
            _animal = e;
            _entities[newEntity.Id] = e;
            NewEntity?.Invoke(newEntity);
        }

        private void VanishEntity(IEntity e)
        {
            _entities.Remove(e.Id);
            EntityVanished?.Invoke(e);
        }


        public void Initialize()
        {
            foreach (var e in _entities.Values.ToList())
            {
                VanishEntity(e);
            }
            _plantCount = 0;
            _factory.Reset();
            for (int i = 0; i < Settings.InitialPlantCount; i++)
            {
                CreatePlant();
            }
            CreateAnimal();
        }

        public void Vegetation()
        {
            if (_turnCounter % Settings.NewPlantFrequency == 0)
            {
                CreatePlant();
            }
        }

        public void NextFrame()
        {       
            _turnCounter++;

            foreach (var entity in _entities.Values.ToList())
            {
                entity.Update();
            }
            if (Settings.MaxPlantCount > _plantCount)
                Vegetation();
        }

        internal IEnumerable<Entity> VisibleEntities(Vector2 position, float sightRange)
        {
            //todo: some tree stuff if optimised search is needed
            return _entities.Values.Where(e => (e.Position - position).Length() <= sightRange);
        }
    }
}
