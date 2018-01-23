using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Simple
{
    public interface IInstructionDoer
    {
        bool GoTo(Instruction.GoTo instruction);
        bool DoNothing();
        bool Eat(Instruction.Eat instruction);
        bool Attack(Instruction.Attack instruction);
        bool Reproduce(Instruction.Reproduce instruction);
    }
    
    public abstract class Instruction
    {
        public abstract string Name { get; }
        public abstract bool Do(IInstructionDoer doer);
        public class DoNothing : Instruction
        {
            public static readonly DoNothing Intentional = new DoNothing(true);
            public static readonly DoNothing Unintentional = new DoNothing(false);
            public readonly bool IsIntentional;

            public override string Name => "DoNothing";
            public override bool Do(IInstructionDoer doer) => doer.DoNothing();
            public override string ToString() => Name;

            private DoNothing(bool intentional)
            {
                IsIntentional = intentional;
            }
        }

        public class GoTo : Instruction
        {
            public override string Name => "GoTo";
            public readonly Vector2 Where;
            public readonly float Speed;

            public GoTo(float x, float y, float speed)
            {
                Where = new Vector2(x, y);
                Speed = Math.Min(1,speed);
            }

            public override bool Do(IInstructionDoer doer) => doer.GoTo(this);

            public override string ToString() => $"{Name}({Where.X},{Where.Y})";
        }

        public class Eat : Instruction
        {
            public override string Name => "Eat";

            public readonly int TargetEntityId;

            public Eat(int targetEntityId)
            {
                TargetEntityId = targetEntityId;
            }
            public override bool Do(IInstructionDoer doer) => doer.Eat(this);

            public override string ToString() => $"{Name}({TargetEntityId})";
        }

        public class Reproduce : Instruction
        {
            public override string Name => "Reproduce";

            public readonly int TargetEntityId;

            public Reproduce(int targetEntityId)
            {
                TargetEntityId = targetEntityId;
            }

            public override bool Do(IInstructionDoer doer) => doer.Reproduce(this);

            public override string ToString() => $"{Name}({TargetEntityId})";
        }


        public class Attack : Instruction
        {
            public override string Name => "Attack";

            public readonly int TargetEntityId;

            public Attack(int targetEntityId)
            {
                TargetEntityId = targetEntityId;
            }

            public override bool Do(IInstructionDoer doer) => doer.Attack(this);

            public override string ToString() => $"{Name}({TargetEntityId})";
        }

    }


}
