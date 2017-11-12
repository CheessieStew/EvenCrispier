using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.GameModelDefinitions;
using Scripts.GameModelDefinitions.Ai;

namespace Scripts.FirstModel
{
	public class World : AbstractWorld
	{
		public int CorpseDecayTime = 20;
		public float BaseSightRange = 10;
		public float BaseMovementSpeed = 10;
		public float InteractionRange = 1;
		
		
		public IList<AnimalSpecies> AnimalSpecies;
		public IList<PlantSpecies> PlantSpecies;
		public World(Vector dimensions, IList<AnimalSpecies> animalSpecies, IList<PlantSpecies> plantSpecies, float plantProbability, int entityNumber) : base(dimensions)
		{
			FirstModelInstructionFactory.Register();
			AnimalSpecies = animalSpecies;
			PlantSpecies = plantSpecies;
		}

		public IEnumerable<IEntity> EntitiesInRange(Vector position, float f) => Entities.Where(e => (position - e.Position).Magnitude <= f);

	}
}
