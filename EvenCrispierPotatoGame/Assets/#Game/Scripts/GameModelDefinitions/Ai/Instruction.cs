using System;
using System.Collections.Generic;

namespace Scripts.GameModelDefinitions.Ai
{
    public abstract class Instruction
    {
        public abstract string Name { get; }
    }

    public  abstract class InstructionFactory<T> where T:Instruction
    {
        public abstract T MakeInstruction(params float[] p);
        public static InstructionFactory<T> Factory;
    }
}