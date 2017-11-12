using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.FirstModel;
using Scripts.GameModelDefinitions;
using Scripts.GameModelDefinitions.Ai;
using UnityEngine;

namespace Scripts
{
	public class WorldTracker : MonoBehaviour
	{
		public EntityTracker AnimalTrackerPrefab;
		public EntityTracker PlantTrackerPrefab;
		public float Width;
		public float Height;
		public float PlantDensity;
		public int StartEntityAmmount;
		public float TimePerTurn;
		public AbstractWorld World;

		public Dictionary<Tuple<int, int>, EntityTracker> Trackers;

		private float _timer;
		
		
		void Start ()
		{
			var species = MakeSpecies();
			World = new World(new Vector(Width,Height), species.Where(s => s is AnimalSpecies) as List<AnimalSpecies>,
				species.Where(s => s is PlantSpecies) as List<PlantSpecies>,  PlantDensity, StartEntityAmmount );
			World.NewEntity += NewEntity;
			World.EntityRemoved += EntityRemoved;
		}

		private void EntityRemoved(AbstractEntity e)
		{
			var idx = Tuple.Create(e.Species, e.Id);
			Destroy(Trackers[idx].gameObject);
			Trackers.Remove(idx);
		}
		
		private void NewEntity(AbstractEntity e)
		{
			EntityTracker newTracker;
			switch (e.Kind)
			{
				case EntityKind.Animal:
					newTracker = Instantiate(AnimalTrackerPrefab);
					break;
						
				case EntityKind.Plant:
					newTracker = Instantiate(PlantTrackerPrefab);
					break;
				default:
					throw new NotImplementedException();
			}
			newTracker.TrackedAnimal = e;
			Trackers.Add(Tuple.Create(e.Species, e.Id),newTracker);
		}
		
		public List<Species> MakeSpecies()
		{
			return new List<Species>
			{
				new AnimalSpecies("Rabbit", 1,
					new AnimalTraits(500, 3000, 10, 5, 10, 70, 1000, 10, 10, 50, 100),
					Diet.Plants),
				new PlantSpecies("Eggplant", 2,
					new PlantTraits(8000,1000,5))
			};
		}

		public void NextTurn()
		{
			World.NextFrame();
			foreach (var tracker in Trackers.Values)
			{
				tracker.UpdateExternal(TimePerTurn);
			}
		}
		
		private void Update()
		{
			_timer += Time.deltaTime;
			if (_timer >= TimePerTurn)
			{
				_timer -= TimePerTurn;
				NextTurn();
			}
		}
	}
}
