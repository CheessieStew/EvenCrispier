using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Grazing
{
    public interface IBrain
    {
        string Name { get; }
        
        void Reset();
        void NewGame();
        Instruction GetNextInstruction(SensesReport report);

        float ProbabilityExponent { get; set; }
        float StartLearningRate { get; set; }
        float StartBaseActionScore { get; set; }

        float Discount { get; set; }
        float LearningRateDamping { get; set; }
        float BaseActionScoreDamping { get; set; }
        float CloseRange { get; set; }
    }
}
                                                                             