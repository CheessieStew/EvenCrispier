namespace Scripts.GameModelDefinitions.Ai
{
    public interface IBrain
    {
        T GetNextInstruction<T>(SensesReport report) where T:Instruction;
        
    }


}