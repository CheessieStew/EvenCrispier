using System.Security;

namespace Scripts.FirstModel
{
    public abstract class Species
    {
        public readonly string Name;
        public readonly int Id;

        protected Species(string name, int id)
        {
            Name = name;
            Id = id;
        }
    }

    public class AnimalSpecies : Species
    {
        public readonly AnimalTraits Traits;
        public readonly Diet Diet;

        public AnimalSpecies(string name, int id, AnimalTraits traits, Diet diet)
            : base(name, id)
        {
            Diet = diet;
            Traits = traits;
        }
    }

    public class PlantSpecies : Species
    {
        public PlantTraits Traits;
        public PlantSpecies(string name, int id, PlantTraits traits)
            : base(name, id)
        {
            Traits = traits;
        }
    }

    public struct PlantTraits
    {
        /// <summary>
        /// How rich in food this plant can be
        /// </summary>
        public readonly int FoodStorage;

        public readonly int RegrowthRate;

        public readonly int RegrowthTime;

        public PlantTraits(int foodStorage, int regrowthRate, int regrowthTime)
        {
            FoodStorage = foodStorage;
            RegrowthRate = regrowthRate;
            RegrowthTime = regrowthTime;
        }
    }
    
    public struct AnimalTraits
    {
        /// <summary>
        /// Maximal number of turns this animal can live
        /// </summary>
        public readonly int LifeExpectancy;

        /// <summary>
        /// How much food this animal's carcass provides
        /// </summary>
        public readonly int Mass;
        
        /// <summary>
        /// Range at which this animal can notice other entities
        /// </summary>
        public readonly int SightRange;
        
        /// <summary>
        /// How strong this animal can attack other animals 
        /// </summary>
        public readonly int Damage;

        /// <summary>
        /// How fast this animal moves
        /// </summary>
        public readonly int Speed;
        
        /// <summary>
        /// How resistant to damage this animal is 
        /// </summary>
        public readonly int Toughness;

        /// <summary>
        /// How much food this animal can have stored
        /// </summary>
        public readonly int StomachCapacity;

        /// <summary>
        /// How much food produces a unit of energy
        /// </summary>
        public readonly int EnergyEffectiveness;

        /// <summary>
        /// How much food produces a unit of health
        /// </summary>
        public readonly int HealingCapabilities;

        /// <summary>
        /// How much food can be burned in a turn
        /// </summary>
        public readonly int Metabolism;
        
        /// <summary>
        /// How much stuff this animal can do without resting
        /// </summary>
        public readonly int EnergyCapacity;
        
        public AnimalTraits(int lifeExpectancy, int mass, int sightRange, int damage, int speed, int toughness, int stomachCapacity, int energyEffectiveness, int healingCapabilities, int metabolism, int energyCapacity)
        {
            LifeExpectancy = lifeExpectancy;
            Mass = mass;
            SightRange = sightRange;
            Damage = damage;
            Speed = speed;
            Toughness = toughness;
            StomachCapacity = stomachCapacity;
            EnergyEffectiveness = energyEffectiveness;
            HealingCapabilities = healingCapabilities;
            Metabolism = metabolism;
            EnergyCapacity = energyCapacity;
        }
    }
}