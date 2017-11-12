using System;
using System.Threading;
using Scripts.GameModelDefinitions;

namespace Scripts.FirstModel
{
    public interface IInstructionDoer
    {
        bool GoTo(GoTo instruction);
        bool DoNothing();
        bool Eat(Eat instruction);
        bool Attack(Attack instruction);
        bool Reproduce(Reproduce instruction);
    }

    public abstract class FirstModelInstruction : GameModelDefinitions.Ai.Instruction
    {
        public abstract bool Do(IInstructionDoer doer);
    }

    public class FirstModelInstructionFactory : GameModelDefinitions.Ai.InstructionFactory<FirstModelInstruction>
    {
        public static void Register()
        {
            Factory = new FirstModelInstructionFactory();
        }
        
        public override FirstModelInstruction MakeInstruction(params float[] p)
        {
            switch ((int)p[0])
            {
                case 0:
                    return DoNothing.Intentional;
                case 1:
                    return new GoTo(new Vector(p[1], p[2]), p[3]);
                default:
                    return DoNothing.Unintentional;
            }
        }
    }
    
    public class DoNothing : FirstModelInstruction
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
    
    public class GoTo : FirstModelInstruction
    {
        public override string Name => "GoTo";
        public readonly Vector Where;
        public readonly float Speed;
        
        public GoTo(Vector where, float speed)
        {
            Where = where;
            Speed = speed;
        }
        
        public override bool Do(IInstructionDoer doer) => doer.GoTo(this);

        public override string ToString() => $"{Name}({Where})";
    }

    public class Eat : FirstModelInstruction
    {
        public override string Name => "Eat";

        public readonly int TargetEntitySpecies;
        public readonly int TargetEntityId;

        public Eat(int targetEntitySpecies, int targetEntityId)
        {
            TargetEntitySpecies = targetEntitySpecies;
            TargetEntityId = targetEntityId;
        }
        public override bool Do(IInstructionDoer doer) => doer.Eat(this);
        
        public override string ToString() => $"{Name}({TargetEntitySpecies},{TargetEntityId})";
    }
    
    public class Attack : FirstModelInstruction
    {
        public override string Name => "Attack";
        
        public readonly int TargetEntitySpecies;
        public readonly int TargetEntityId;

        public Attack(int targetEntitySpecies, int targetEntityId)
        {
            TargetEntitySpecies = targetEntitySpecies;
            TargetEntityId = targetEntityId;
        }

        public override bool Do(IInstructionDoer doer) => doer.Attack(this);
        
        public override string ToString() => $"{Name}({TargetEntitySpecies},{TargetEntityId})";
    }

    public class Reproduce : FirstModelInstruction
    {
        public override string Name => "Reproduce";
        
        public readonly int TargetEntitySpecies;
        public readonly int TargetEntityId;

        public Reproduce(int targetEntitySpecies, int targetEntityId)
        {
            TargetEntitySpecies = targetEntitySpecies;
            TargetEntityId = targetEntityId;
        }

        public override bool Do(IInstructionDoer doer) => doer.Reproduce(this);
        
        public override string ToString() => $"{Name}({TargetEntitySpecies},{TargetEntityId})";
    }

}