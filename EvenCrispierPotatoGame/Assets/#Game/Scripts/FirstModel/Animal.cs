using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.GameModelDefinitions;
using Scripts.GameModelDefinitions.Ai;

namespace Scripts.FirstModel
{  
    public class Animal : AbstractEntity, IInstructionDoer
    {
        private readonly IBrain _brain;
        public bool LastInstructionResult { get; protected set; }
        private new  World World { get; }
        private new AnimalSpecies Species { get; }
        private AnimalBiology _biology;
        
        public Animal(World world, int id, AnimalSpecies species, Sex sex, IBrain brain, string appearanceCode)
            : base(world,id,species.Id, EntityKind.Animal, sex, appearanceCode)
        {
            Species = species;
            World = world;
            _brain = brain;
            _biology = new AnimalBiology
            {
                Hunger = species.Traits.StomachCapacity,
                Health = species.Traits.Toughness,
                Energy = Species.Traits.EnergyCapacity
            };
            MassLeft = species.Traits.Mass;
            world.AddEntity(this);
        }
        
        public override void Update()
        {
            Age += 1;
            if (State == State.Alive)
            {
                if (Age >= Species.Traits.LifeExpectancy)
                    Die();
                var instruction = _brain.GetNextInstruction<FirstModelInstruction>(GetSensesReport());
                LastInstruction = instruction;
                LastInstructionResult = instruction.Do(this);
            }
            else
            {
                if (Age - Species.Traits.LifeExpectancy > World.CorpseDecayTime)
                {
                    RemoveEntity();
                }
            }
        }

        private void Die()
        {
            State = State.Dead;
        }
        
        protected SensesReport GetSensesReport()
        {
            return new SensesReport
                {
                    VisibleEntities = World.EntitiesInRange(Position, World.BaseSightRange * Species.Traits.SightRange).ToList(),
                    ThisEntity = this,
                    Hormones = new List<float>
                    {
                        (float)_biology.Health/Species.Traits.Toughness,
                        (float)_biology.Hunger/Species.Traits.StomachCapacity, 
                        (float)_biology.Energy/Species.Traits.EnergyCapacity 
                    },
                    LastResult = LastInstructionResult
                };
        }

        public bool GoTo(GoTo instruction)
        {
            throw new NotImplementedException();
        }

        public bool DoNothing()
        {
            var restingMetabolism = Species.Traits.Metabolism * 2;
            var burnedFood = 0;
            burnedFood += RestoreHealth(Math.Min(restingMetabolism - burnedFood, _biology.Hunger));
            RestoreEnergy(Math.Min(restingMetabolism - burnedFood, _biology.Hunger));
            return true;
        }

        private int RestoreEnergy(int foodAvailable)
        {
            var missingEnergy = Species.Traits.EnergyCapacity - _biology.Energy;
            if (missingEnergy <= 0) return 0;
            var foodToBurn = Species.Traits.EnergyEffectiveness * missingEnergy;
            foodToBurn = Math.Min(foodToBurn, foodAvailable);
            var energyToRestore = foodToBurn / Species.Traits.EnergyEffectiveness;
            foodToBurn = Species.Traits.EnergyEffectiveness * energyToRestore;
            _biology.Hunger -= foodToBurn;
            _biology.Energy += energyToRestore;
            return foodToBurn;
        }
        
        private int RestoreHealth(int foodAvailable)
        {
            var missingHealth = Species.Traits.Toughness - _biology.Health;
            if (missingHealth <= 0) return 0;
            var foodToBurn = Species.Traits.HealingCapabilities * missingHealth;
            foodToBurn = Math.Min(foodToBurn, foodAvailable);
            var healthTorestore = foodToBurn / Species.Traits.HealingCapabilities;
            foodToBurn = Species.Traits.HealingCapabilities * healthTorestore;
            _biology.Hunger -= foodToBurn;
            _biology.Health += healthTorestore;
            return foodToBurn;
        }
        
        public bool Eat(Eat instruction)
        {
            var foodToEat = Species.Traits.StomachCapacity - _biology.Hunger;
            var target = World.GetEntity(instruction.TargetEntitySpecies, instruction.TargetEntityId);
            if (target == null)
                return false;
            var distance = (target.Position - Position).Magnitude;
            if (distance > World.InteractionRange || foodToEat <= 0 || target.Kind==EntityKind.Animal && target.State != State.Dead || target.MassLeft <= 0) return false;
            foodToEat = Math.Min(Math.Min(target.MassLeft, foodToEat), Species.Traits.StomachCapacity / 4);
            target.MassLeft -= foodToEat;

            if (target.Kind == EntityKind.Animal && !Species.Diet.HasFlag(Diet.Meat)
                || target.Kind == EntityKind.Plant && !Species.Diet.HasFlag(Diet.Plants))
            {
                _biology.Health -= Species.Traits.Toughness / 10;
            }
            else
            {
                _biology.Hunger += foodToEat;
            }
            return true;

        }


        public bool Attack(Attack instruction)
        {
            throw new NotImplementedException();
        }

        public bool Reproduce(Reproduce instruction)
        {
            throw new NotImplementedException();
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
        /// Decreased by wounds, restored by burning food while resting. Zero means death
        /// </summary>
        public int Health;
        
        /// <summary>
        /// Decreased over time to increase Energy, restored by food
        /// </summary>
        public int Hunger;
        
        /// <summary>
        /// Decreased by actions, increased by burning food
        /// </summary>
        public int Energy;
    }
   
}