using GameEngine.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Simple
{
    public class World : IWorld
    {
        public List<AnimalSpecies> SpeciesList;
        public Settings Settings;
        private IDictionary<int, Entity> _entities = new Dictionary<int, Entity>();
        public IDictionary<int, IEntity> Entities => (IDictionary<int, IEntity>)_entities;


        private Random _rng = new Random();
        public Random Rng => _rng;


        private Vector2 _dimensions = new Vector2(500, 500);
        public Vector2 Dimensions => _dimensions;
        public float Height => _dimensions.X;
        public float Width => _dimensions.Y;

        public float DistanceScale => Settings.MovementDistanceScale;
        private int _turnCounter;
        public int TurnCounter => _turnCounter;


        public event EntityEventHandler NewEntity;
        public event EntityEventHandler EntityVanished;

        private EntityFactory _factory;

        private Func<IBrain> _brainFac;

        public World(EntityFactory factory, Settings settings, List<AnimalSpecies> speciesList, Func<IBrain> brainFac)
        {
            Settings = settings;
            SpeciesList = speciesList;
            _brainFac = brainFac;
            _factory = factory;
        }

        private float Roll(float min, float max)
        {
            return (float)Rng.NextDouble() * (min - max) + min;
        }

        private void CreatePlant()
        {
            var mass = (int)Roll(Settings.MinPlantMass, Settings.MaxPlantMass);
            var growthRate = (int)Roll(Settings.MinGrowthRate, Settings.MaxGrowthRate);
            var newEntity = _factory.GetEntity(this, "Plant", (Action<IEntity>)VanishEntity, mass, growthRate);
            if (!(newEntity is Entity e))
                throw new InvalidOperationException();
            _entities[newEntity.Id] = e;
            NewEntity?.Invoke(newEntity);
        }

        private void CreateAnimal(AnimalSpecies s, Vector2 position)
        {
            var newEntity = _factory.GetEntity(this, "Animal", (Action<IEntity>)VanishEntity, s, position, _brainFac());
            if (!(newEntity is Entity e))
                throw new InvalidOperationException();
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
            for (int i = 0; i < 60; i++)
            {
                CreatePlant();
            }
            foreach (var species in SpeciesList)
            {
                for (int i = 0; i < species.InitialCount; i++)
                {
                    Vector2 position = new Vector2();
                    CreateAnimal(species, position);
                }
            }
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
            Vegetation();
        }

        internal IEnumerable<Entity> VisibleEntities(Vector2 position, float sightRange)
        {
            //todo: some tree stuff if optimised search is needed
            return _entities.Values.Where(e => (e.Position - position).Length() <= sightRange);
        }
    }
}
