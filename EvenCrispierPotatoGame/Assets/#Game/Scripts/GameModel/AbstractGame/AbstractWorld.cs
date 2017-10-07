using System.Collections.Generic;
using Scripts.GameModel.AbstractGame.AiDefinitions;

namespace Scripts.GameModel.AbstractGame
{
    public abstract class AbstractWorld : IWorld
    {
        protected Dictionary<int, AbstractEntity> Entities;
        protected Dictionary<int, Dictionary<int, AbstractEntity>> Species;
        public Vector Dimensions { get; }

        protected AbstractWorld(Vector dimensions)
        {
            Dimensions = dimensions;
        }
        
        public virtual void NextFrame()
        {
            foreach (var entity in Entities.Values)
            {
                entity.Update();
            }
        }
    }
}