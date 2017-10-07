namespace Scripts.GameModel.AbstractGame.AiDefinitions
{
    public interface IBrain
    {
        Instruction GetNextInstruction(SensesReport report);
    }

}