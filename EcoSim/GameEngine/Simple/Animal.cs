using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Simple
{


    public abstract partial class Entity
    {


        private class Animal : Entity, IInstructionDoer
        {
            public override int IntBodyType => 1;
            private IBrain _brain;
            public int Energy { get; private set; }
            private AnimalSpecies _species;
            public AnimalSpecies Species
            {
                get => _species;
                set
                {
                    _species = value;
                    MaxHealth = _species.Traits.MaxHealth;
                    Health = MaxHealth;
                    foreach (var desc in Species.Descriptions)
                    {
                        _descriptions["species_" + desc.Name] = desc;
                    }
                }
            }
            public override string BodyType => "Animal";
            private bool _lastResult;
            private int _massLeft;

            public Animal()
            {
                AddDescription("MassLeft", () => _massLeft);
            }

            public bool Attack(Instruction.Attack instruction)
            {
                var target = _world.Entities[instruction.TargetEntityId] as Entity;
                if ((target.Position - Position).Length() > _world.Settings.InteractionDistance)
                    return false;
                return target.TakeDamage(Species.Traits.Strength, this);
            }

            public bool DoNothing()
            {
                return true;
            }

            public bool Eat(Instruction.Eat instruction)
            {
                var target = _world.Entities[instruction.TargetEntityId] as Entity;
                if ((target.Position - Position).Length() > _world.Settings.InteractionDistance)
                    return false;
                var chunk = target.GetEaten(Math.Min(Species.Traits.StomachCapacity, Species.Traits.BiteSize), this);
                Energy += chunk;
                return chunk > 0;
            }

            public bool GoTo(Instruction.GoTo instruction)
            {
                var direction = (instruction.Where - Position);
                direction *= _world.DistanceScale * Species.Traits.Speed * instruction.Speed / direction.Length();
                _position += direction;
                _position.X = Math.Min(_world.Dimensions.X, Math.Max(0, _position.X));
                _position.Y = Math.Min(_world.Dimensions.Y, Math.Max(0, _position.Y));
                UpdatePosition?.Invoke();
                return true;
            }



            public bool Reproduce(Instruction.Reproduce instruction)
            {
                throw new NotImplementedException();
            }

            public override void Update()
            {
                if (Alive && Health <= 0)
                    Die();
                if (Alive)
                {
                    _lastResult = _brain.GetNextInstruction(GetSensesReport()).Do(this);
                }
                {
                    Health--;
                    if (Health < -20)
                        Vanish?.Invoke(this);
                }
            }

            private SensesReport GetSensesReport() => new SensesReport
            {
                LastResult = _lastResult,
                Health = ((float)Health)/MaxHealth,
                Energy = ((float)Energy)/Species.Traits.StomachCapacity,
                ThisEntity = GetDescription(),
                VisibleEntities = _world.VisibleEntities(Position, Species.Traits.SightRange).Select(e => e.GetDescription()).ToList()
            };

            protected override int GetEaten(int biteSize, Entity e)
            {
                if (Alive)
                    return 0;
                var chunk = Math.Max(_massLeft, biteSize);
                _massLeft -= chunk;
                return chunk;            
            }

            protected override void Die()
            {
                base.Die();
                _massLeft = Species.Traits.Mass;
            }
        }
    }
}
