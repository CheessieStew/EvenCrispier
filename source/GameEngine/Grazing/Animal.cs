using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Grazing
{
    //todo: descriptions from settings


    public abstract partial class Entity
    {
        private class Animal : Entity, IInstructionDoer
        {
            public override string Name => "Donkey";
            public override int IntBodyType => 1;
            public IBrain Brain;
            public override int MaxMass => _world.Settings.StomachCapacity;


            public override string BodyType => "Animal";

            private Instruction _lastInstruction;
            private bool _lastResult;
            private int _deltaHunger;
            public int Age { get; private set; }

            public Animal()
            {
                AddDescription("Age", () => Age);
                AddDescription("Instruction", () => _lastInstruction.ToString());
                AddDescription("Delta hunger", () => _deltaHunger);
                AddDescription(" ", () => " ");
                AddDescription("Brain", () => Brain.Name);
                AddDescription("BrainBaseActionScore", () => _world.Settings.BrainBaseActionScore);
                AddDescription("BrainDiscount", () => _world.Settings.BrainDiscount);
                AddDescription("BrainLearningRate", () => _world.Settings.BrainLearningRate);
                AddDescription("BrainLearningRateDamping", () => _world.Settings.BrainLearningRateDamping);
                AddDescription("BrainProbabilityExponent", () => _world.Settings.BrainProbabilityExponent);
                AddDescription("  ", () => " ");
                AddDescription("Bite size", () => _world.Settings.BiteSize);
                AddDescription("Movement speed", () => _world.Settings.MovementSpeed);
                AddDescription("Interaction distance", () => _world.Settings.InteractionDistance);
                AddDescription("Sight range", () => _world.Settings.SightRange);
                AddDescription("Mass loss per turn", () => _world.Settings.PassiveWork);
                AddDescription("Movement mass loss", () => _world.Settings.MovementWork);
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
                var chunk = target.GetEaten(Math.Min(MaxMass - Mass, _world.Settings.BiteSize), this);
                Mass += chunk;
                return chunk > 0;
            }

            public bool GoTo(Instruction.GoTo instruction)
            {
                var direction = (instruction.Where - Position);
                var old = _position;
                _position += direction * _world.Settings.MovementSpeed * instruction.Speed / direction.Length();
                _position.X = Math.Min(_world.Dimensions.X, Math.Max(0, _position.X));
                _position.Y = Math.Min(_world.Dimensions.Y, Math.Max(0, _position.Y));
                Mass -= (int)(_world.Settings.MovementWork * (old - _position).Length() / _world.Settings.MovementSpeed);
                UpdatePosition?.Invoke();
                return old != _position;
            }

            protected override void Die()
            {

                if (_world.Settings.LogHunger)
                    _world.LogMessage($"Hunger\t{_world.TurnCounter}\t{Mass}");
                base.Die();
            }

            public override void Update()
            {
                _deltaHunger = Mass;
                if (Alive && Mass <= 0)
                    Die();
                if (Alive)
                {
                    if (_world.Settings.LogHunger && _world.TurnCounter % _world.Settings.LogHungerEvery == 0)
                        _world.LogMessage($"Hunger\t{_world.TurnCounter}\t{Mass}");
                    Age++;
                    _lastInstruction = Brain.GetNextInstruction(GetSensesReport());
                    _lastResult = _lastInstruction.Do(this);
                    Mass -= _world.Settings.PassiveWork;
                }
                _deltaHunger -= Mass;
            }

            private SensesReport GetSensesReport() => new SensesReport
            {
                Hunger = 1 - ((float)Mass) / MaxMass,
                ThisEntity = GetDescription(),
                VisibleEntities = _world.VisibleEntities(Position, _world.Settings.SightRange).Where(e => e.Id != Id).Select(e => e.GetDescription()).ToList()

            };

            protected override int GetEaten(int biteSize, Entity e)
            {
                return 0;
            }

            internal override void Kill()
            {
                Die();
            }
        }

    }
}
