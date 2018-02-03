using GameEngine.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoSim.Simple
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

    class QLearningBrainOne : IBrain
    {
        private float _targetX;
        private float _targetY;
        private Random _rng = new Random();
        public float Discount;
        public List<Action> PossibleActions;
        private float QValue(GameState s, Action a)
        {   //
            ////r + Di max_{a'} Q(s',a')
            //var r = 0; //ImmediateReward();
            //
            //var res = r + Discount * PossibleActions.Max(a => QValue(new GameState(), a);
            return 0;
        }
        


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

        private class GameState
        {
        }
    }
}
