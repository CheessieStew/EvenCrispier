using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Grazing
{
    public interface IBrain
    {
        object GetInternal();
        void SetInternal(object o);
        void Reset();
        void NewGame();
        Instruction GetNextInstruction(SensesReport report);
    }
}
