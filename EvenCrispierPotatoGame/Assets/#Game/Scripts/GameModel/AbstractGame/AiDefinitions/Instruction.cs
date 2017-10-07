namespace Scripts.GameModel.AbstractGame.AiDefinitions
{
    public abstract class Instruction
    {
        public abstract string Name { get; }
    }

    public class DoNothing : Instruction
    {
        public override string Name => "DoNothing";
    }
    
    public class GoTo : Instruction
    {
        public override string Name => "GoTo";
    }

    public class Eat : Instruction
    {
        public override string Name => "Eat";
        public readonly int TargetEntityId;

        public Eat(int targetEntityId)
        {
            TargetEntityId = targetEntityId;
        }
    }
}