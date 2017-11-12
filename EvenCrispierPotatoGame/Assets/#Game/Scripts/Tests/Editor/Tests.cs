using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Scripts.FirstModel;
using Scripts.GameModelDefinitions;
using Scripts.GameModelDefinitions.Ai;

namespace Scripts.Tests.Editor
{
	
	public class Tests
	{
		private readonly AnimalSpecies _rabbit = new AnimalSpecies("Rabbit", 1,
			new AnimalTraits(500, 3000, 10, 5, 10, 70, 1000, 10, 10, 50, 100),
			Diet.Plants
			);
		
		class MockBrain : IBrain
		{
			public T GetNextInstruction<T>(SensesReport report) where T : Instruction
			{
				return InstructionFactory<T>.Factory.MakeInstruction(0.5f, 0.5f);
			}
		}
		
		[Test]
		public void UpdateStability() {
			
			var brain = new MockBrain();
			var world = new World(new Vector(1,1),Enumerable.Empty<AnimalSpecies>().ToList(),Enumerable.Empty<PlantSpecies>().ToList(), 0, 10);
			var animal = new Animal(world, 1, _rabbit, Sex.Male, brain, "asdf");
			animal.Update();
		}
	}
}
