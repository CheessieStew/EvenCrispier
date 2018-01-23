using GameEngine.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoSim
{
    class StupidBrain : IBrain
    {

        private float _targetX;
        private float _targetY;
        private Random _rng = new Random();


        public Instruction GetNextInstruction(SensesReport report)
        {
            var difx = report.ThisEntity.PositionX - _targetX;
            var dify = report.ThisEntity.PositionY - _targetY;
            if (Math.Sqrt(difx * difx + dify * dify) <= 1)
            {
                _targetX = (float)(_rng.NextDouble() * 500);
                _targetY = (float)(_rng.NextDouble() * 500);
            }
            return new Instruction.GoTo(_targetX, _targetY, 1);
        }

        

    }
}
