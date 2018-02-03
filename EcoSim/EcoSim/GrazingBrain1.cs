using GameEngine.Grazing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Entity = GameEngine.Grazing.Entity;
namespace EcoSim.Grazing
{
    class QLearningBrainOne : IBrain
    {
        public object GetInternal()
        {
            return _qTable;
        }
        public void SetInternal(object o)
        {
            if (!(o is Dictionary<Action, float>[] qTable) || qTable.Length != State.MaxValue || qTable.Any(dict => dict == null))
                throw new InvalidOperationException();
            _qTable = qTable;
        }

        private GameState _state;
        private float _targetX;
        private float _targetY;
        private Random _rng = new Random();
        public float Discount = 0.8f;
        public float RandomActionChance = 0.1f;
        public float RandomFactor = -0.01f;
        public float CloseRange = 21;
        public float MediumRange = 44;
        private Dictionary<Action, float>[] _qTable;
        private float QValue(State s, Action a) => _qTable[s.Compressed].TryGetValue(a, out float v) ? v : 0;
        private Action _lastAction;
        private State _lastState;
        private float _lastHunger;
        
        public Instruction GetNextInstruction(SensesReport report)
        {
            _state.ApplyReport(report);
            //apply the current reward for last action
            if (_lastHunger > 0)
                _qTable[_lastState.Compressed][_lastAction] = 
                    (_lastHunger - _state.Report.Hunger)
                    + Discount * Action.Possibillities.Max(a => QValue(_state.Value, a));
            _lastHunger = _state.Report.Hunger;
            _lastState = _state.Value;

            _lastAction = PickAction();
            return _lastAction.ToInstruction(_state);
        }
        
        private Action PickAction()
        {
            RandomFactor *= 0.999f;
            RandomActionChance *= 0.999f;
            var possibillities = Action.Possibillities.
                Select(a => (action: a, value: _rng.NextDouble() * RandomFactor + QValue(_lastState, a))) //penalty for this action
                .OrderBy(av => av.value); //lowest-penalty action first
                
            var roll = _rng.NextDouble();
            if (roll >= RandomActionChance)
                return possibillities.First().action;
            roll = _rng.NextDouble();
            var chance = 0.5;
            foreach (var av in possibillities.Skip(1))
            {
                roll -= chance;
                if (roll <= 0)
                    return av.action;
                chance /= 2;
            }
            return new Action() { Kind = Action.ActionKind.Nothing };
        }

        public void Reset()
        {
            _qTable = new Dictionary<Action, float>[State.MaxValue];
            for (int i = 0; i < State.MaxValue; i++)
                _qTable[i] = new Dictionary<Action, float>();
            _state = new GameState(this);
        }

        public void NewGame()
        {
            _state = new GameState(this);
            _lastHunger = 0;
        }

        private struct Action : IEquatable<Action>
        {
            public ActionKind Kind;
            public ActionDirection Direction;

            public static IEnumerable<Action> Possibillities
            {
                get
                {
                    for (var kind = 0; kind < 4; kind++)
                    {
                        if (kind != 1)
                        for (var direction = 0; direction < 8 && kind > 0; direction++)
                        {
                            yield return new Action()
                            {
                                Kind = (ActionKind)kind,
                                Direction = (ActionDirection)direction
                            };
                        }
                    }
                }
            }

            public enum ActionKind
            {
                Nothing = 0,
                Go = 1,
                GoFast = 2,
                Eat = 3
            }

            public enum ActionDirection
            {
                Right = 0,
                Down = 2,
                Left = 4,
                Up = 6,
                
                DownRight = 1,
                
                DownLeft = 3,
                
                UpLeft = 5,
                
                UpRight = 7
            }

            public Instruction ToInstruction(GameState s)
            {
                GameEngine.Grazing.Entity.EntityDescription target;
                IEnumerable<GameEngine.Grazing.Entity.EntityDescription> targets;
                var a = this;
                switch(Kind)
                {
                    case ActionKind.Nothing:
                        return Instruction.DoNothing.Intentional;
                    case ActionKind.Eat:
                        targets = s.Report.VisibleEntities.Where(
                                    e => e.AvailableMass > 0 && (ActionDirection)(((e.Position - s.Report.ThisEntity.Position).VectorAngle() - s.AnglePerDirection / 2) / s.AnglePerDirection) == a.Direction);
                        target = targets.OrderBy(e => (e.Position - s.Report.ThisEntity.Position).Length()).FirstOrDefault();
                        if (target == null)
                            return Instruction.DoNothing.Unintentional;
                        return new Instruction.Eat(target.Id);
                    case ActionKind.Go:
                    case ActionKind.GoFast:
                        targets = s.Report.VisibleEntities.Where(
                                    e => e.AvailableMass > 0
                                         && (ActionDirection)(((e.Position - s.Report.ThisEntity.Position).VectorAngle() - s.AnglePerDirection/2) / s.AnglePerDirection) == a.Direction
                                         && (e.Position - s.Report.ThisEntity.Position).Length() > 1);

                        target = targets.OrderBy(e => (e.Position - s.Report.ThisEntity.Position).Length()).FirstOrDefault();
                        if (target != null)
                            return new Instruction.GoTo(target.PositionX, target.PositionY, Kind == ActionKind.GoFast ? 1 : 0.5f);
                        var dir = Vector2.Transform(Vector2.UnitX, Matrix3x2.CreateRotation((int)a.Direction * s.AnglePerDirection)) * 100;
                        dir += s.Report.ThisEntity.Position;
                        return new Instruction.GoTo(dir.X, dir.Y, Kind == ActionKind.GoFast ? 1 : 0.5f);


                }
                return Instruction.DoNothing.Unintentional;
            }

            public bool Equals(Action other) => Kind == other.Kind && Direction == other.Direction;
        }

        private struct State
        {
            //            public long Compressed => FoodRichDirections | CloseFoodDirections << 8 | MediumFoodDirections << 16 | Hunger << 24;
            //            public const int MaxValue = (1 << 8 - 1) | (1 << 8 - 1) << 8 | (1 << 8 - 1) << 16 |  (1 << 3 - 1) << 24;

            public long Compressed => CloseFoodDirections | FoodRichDirections << 8| Hunger << 16;
            public const int MaxValue = ((1 << 8) - 1) | ((1 << 8) - 1) << 8 | ((1 << 3) - 1) << 16;
            /// <summary>
            /// 1 bit for each direction indicating food richness scaled by distance
            /// </summary>
            public byte FoodRichDirections;
            /// <summary>
            /// 1 bit for each direction indicating presence of food richness within Settings.Close range
            /// </summary>
            public byte CloseFoodDirections;
            /// <summary>
            /// 1 bit for each direction indicating presence of food richness within Settings.Medium range
            /// </summary>
            public byte MediumFoodDirections;


            /// <summary>
            /// Current penalty, 3 bits
            /// </summary>
            public byte Hunger;
        }

        private class GameState
        {
            public State Value
            {
                get
                {
                    var res = new State();
                    for (int i = 0; i < 8; i++)
                    {
                        res.FoodRichDirections |= FoodRichScaled(i) ? (byte)(1 << i) : (byte)0;
                        res.CloseFoodDirections |= FoodRich(i, CloseMassesAvailable) ? (byte)(1 << i) : (byte)0;
                        res.MediumFoodDirections |= FoodRich(i, MediumMassesAvailable) ? (byte)(1 << i) : (byte)0;
                    };
                    res.Hunger = (byte)(Penalty * (1 << 3 - 1));
                    return res;
                }
            }


            public float[] RoundedMassesAvailable;
            public float[] CloseMassesAvailable;
            public float[] MediumMassesAvailable;

            public float Penalty => Report.Hunger;
            public const int Directions = 8;
            public float AnglePerDirection => (float)Math.PI * 2 / Directions;

            public SensesReport Report { get; private set; }

            private QLearningBrainOne _brain;
            public GameState(QLearningBrainOne b)
            {
                _brain = b;
                RoundedMassesAvailable = new float[Directions];
                CloseMassesAvailable = new float[Directions];
                MediumMassesAvailable = new float[Directions];

            }

            public void ApplyReport(SensesReport report)
            {
                Report = report;
                for (int i = 0; i < Directions; i++)
                {
                    RoundedMassesAvailable[i] = 0;
                    CloseMassesAvailable[i] = 0;
                    MediumMassesAvailable[i] = 0;
                }
                foreach(var e in report.VisibleEntities)
                {
                    var relative = e.Position - report.ThisEntity.Position;
                    var distance = relative.Length();
                    var direction = (int)((relative.VectorAngle() - AnglePerDirection/2) / AnglePerDirection);
                    RoundedMassesAvailable[direction] = e.AvailableMass / distance;
                    CloseMassesAvailable[direction] += distance > _brain.CloseRange ? 0 : e.AvailableMass;
                    MediumMassesAvailable[direction] += distance < _brain.CloseRange || distance > _brain.MediumRange ? 0 : e.AvailableMass;
                }
                RoundedMassesAvailable = Foods().ToArray();
            }


            private bool FoodRichScaled(int direction)
            {
                return RoundedMassesAvailable.Sum() / (Directions + 1) < RoundedMassesAvailable[direction];
            }

            private bool FoodRich(int direction, float[] masses)
            {
                return masses.Sum() / (Directions + 1) < masses[direction];
            }

            private IEnumerable<float> Foods()
            {
                for (int direction = 0; direction < Directions; direction++)
                {
                    float food = 0;
                    for (int i = -2; i <= 2; i++)
                    {
                        food += RoundedMassesAvailable[(i + direction + Directions) % Directions] / (Math.Abs(i) + 1);
                    }
                    yield return food;
                }
            }
 
        }
    }

    internal static class MathUtils
    {
        public static double ToDegrees(this float a) => a * 180 / Math.PI;

        public static float VectorAngle(this Vector2 v)
        {
            var res = (Math.Atan2(v.Y, v.X));
            return (float)(res < 0 ? res + Math.PI * 2 : res);
        }

    }
}
