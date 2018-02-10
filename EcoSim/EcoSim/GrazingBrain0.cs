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
    class QLearningBrainMk0 : IBrain
    {
        public string Name => "Mk0";
        private GameState _state;
        private Random _rng = new Random();

        private Dictionary<Action, float>[] _qTable;
        private float QValue(State s, Action a) => _qTable[s.Compressed].TryGetValue(a, out float v) ? v : 0;
        private Action _lastAction;
        private State _lastState;
        private float _lastHunger;

        private float _learningRate;
        private float _baseActionScore;

        public float ProbabilityExponent { get; set; } = 2f;
        public float StartLearningRate { get; set; } = 0.5f;
        public float StartBaseActionScore { get; set; } = 0.1f;
        public float Discount { get; set; } = 0.8f;
        public float LearningRateDamping { get; set; } = 0.9999f;
        public float BaseActionScoreDamping { get; set; } = 0.999f;
        public float CloseRange { get; set; } = 1f;

        public Instruction GetNextInstruction(SensesReport report)
        {
            lock (this)
            {
                _state.ApplyReport(report);
                //apply the current reward for last action
                if (_lastHunger > 0)
                {
                    _qTable[_lastState.Compressed][_lastAction] = QValue(_lastState, _lastAction) * (1 - _learningRate)
                        + _learningRate * ((_lastHunger - _state.Report.Hunger) + Discount * Action.Possibillities.Max(a => QValue(_state.Value, a)));
                }
                _lastHunger = _state.Report.Hunger;
                _lastState = _state.Value;

                _lastAction = PickAction();
                return _lastAction.ToInstruction(_state);
            }
        }

        private Action PickAction()
        {
            _baseActionScore *= BaseActionScoreDamping;
            _learningRate *= LearningRateDamping;
            IEnumerable<(Action action, float value)> possibilities = Action.Possibillities.
                Select(a => (action: a, value: QValue(_lastState, a))).OrderByDescending(av => av.value);
            var minv = possibilities.Last().value;
            possibilities = possibilities
                .Select(av => (av.action, (float)Math.Pow(av.value - minv + _baseActionScore, ProbabilityExponent)));
            var sumv = possibilities.Sum(av => av.value);
            possibilities = possibilities
                .Select(av => (av.action, av.value / sumv));
            var roll = _rng.NextDouble();
            foreach (var av in possibilities)
            {
                roll -= av.value;
                if (roll <= 0)
                    return av.action;
            }
            return new Action() { Kind = Action.ActionKind.Nothing };
        }

        public void Reset()
        {
            lock (this)
            {
                _baseActionScore = StartBaseActionScore;
                _learningRate = StartLearningRate;
                _qTable = new Dictionary<Action, float>[State.MaxValue];
                for (int i = 0; i < State.MaxValue; i++)
                    _qTable[i] = new Dictionary<Action, float>();
                _state = new GameState(this);
            }
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
                switch (Kind)
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
                                         && (ActionDirection)(((e.Position - s.Report.ThisEntity.Position).VectorAngle() - s.AnglePerDirection / 2) / s.AnglePerDirection) == a.Direction
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

            public long Compressed => CloseFoodDirections | Hunger << 8;
            public const int MaxValue = ((1 << 8) - 1) | ((1 << 5) - 1) << 8;
            /// <summary>
            /// 1 bit for each direction indicating food richness scaled by distance
            /// </summary>
            public byte FoodRichDirections;
            /// <summary>
            /// 1 bit for each direction indicating presence of food richness within Settings.Close range
            /// </summary>
            public byte CloseFoodDirections;



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
                    };
                    res.Hunger = (byte)(Penalty * ((1 << 5) - 1));
                    return res;
                }
            }


            public float[] RoundedMassesAvailable;
            public float[] CloseMassesAvailable;

            public float Penalty => Report.Hunger;
            public const int Directions = 8;
            public float AnglePerDirection => (float)Math.PI * 2 / Directions;

            public SensesReport Report { get; private set; }

            private QLearningBrainMk0 _brain;
            public GameState(QLearningBrainMk0 b)
            {
                _brain = b;
                RoundedMassesAvailable = new float[Directions];
                CloseMassesAvailable = new float[Directions];
            }

            public void ApplyReport(SensesReport report)
            {
                Report = report;
                for (int i = 0; i < Directions; i++)
                {
                    RoundedMassesAvailable[i] = 0;
                    CloseMassesAvailable[i] = 0;
                }
                foreach (var e in report.VisibleEntities)
                {
                    var relative = e.Position - report.ThisEntity.Position;
                    var distance = relative.Length();
                    var direction = (int)((relative.VectorAngle() - AnglePerDirection / 2) / AnglePerDirection);
                    RoundedMassesAvailable[direction] = e.AvailableMass / distance;
                    CloseMassesAvailable[direction] += distance > _brain.CloseRange ? 0 : e.AvailableMass;
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
}
