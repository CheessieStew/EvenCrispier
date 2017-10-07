using System;
using Scripts.GameModel.AbstractGame;
using Scripts.GameModel.AbstractGame.AiDefinitions;

namespace Scripts.GameModel
{

    
    public class Animal : AbstractEntity
    {
        private readonly IBrain _brain;
        private bool _lastInstructionResult;
        public readonly AnimalTraits Stats;
        
        public Animal(IWorld world, int id, int species, Sex sex, AnimalTraits stats, IBrain brain, int appearanceCode)
            : base(world,id,species, EntityKind.Animal, sex, appearanceCode)
        {
            _brain = brain;
            Stats = stats;
        }
        
        public override void Update()
        {
            _lastInstructionResult = DoInstruction(_brain.GetNextInstruction(GetSensesReport()));
        }

        protected bool DoInstruction(Instruction instruction)
        {
            return true;
        }

        protected SensesReport GetSensesReport()
        {
            return new SensesReport();
        }
    }

    [Flags]
    public enum Diet
    {
        Meat = 1,
        Fruit = 2,
        Plants = 4
    }
    
    public struct AnimalBiology
    {
        /// <summary>
        /// Decreased by wounds or poisons, restored by rest while well fed. Zero means death
        /// </summary>
        public int Health;
        
        /// <summary>
        /// Decreased over time, restored by food
        /// </summary>
        public int Hunger;
        
        /// <summary>
        /// 
        /// </summary>
        public int Energy;
    }
    
    public struct AnimalTraits
    {
        /// <summary>
        /// Maximal number of turns this animal can live
        /// </summary>
        public readonly int LifeExpectancy;

        /// <summary>
        /// How many turns after birth this animal becomes an adult
        /// </summary>
        public readonly int GrowthTime;
        
        /// <summary>
        /// How many pieces of meat this animal's carcass provides
        /// </summary>
        public int Mass;
        
        /// <summary>
        /// Range at which this animal can notice other entities
        /// </summary>
        public int SightRange;
        
        /// <summary>
        /// How strong this animal can attack other animals 
        /// </summary>
        public int Damage;

        /// <summary>
        /// How fast this animal moves
        /// </summary>
        public int Speed;
        
        /// <summary>
        /// How resistant to damage this animal is 
        /// </summary>
        public int Toughness;

        public AnimalTraits(int lifeExpectancy, int growthTime, int mass, int sightRange, int damage, int speed, int toughness)
        {
            LifeExpectancy = lifeExpectancy;
            GrowthTime = growthTime;
            Mass = mass;
            SightRange = sightRange;
            Damage = damage;
            Speed = speed;
            Toughness = toughness;
        }
    }
}