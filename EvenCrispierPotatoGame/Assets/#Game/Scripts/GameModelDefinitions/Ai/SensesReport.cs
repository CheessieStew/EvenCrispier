using System.Collections.Generic;

namespace Scripts.GameModelDefinitions.Ai
{
    /// <summary>
    /// The summary of all the sensory input the animal gets from the world
    /// </summary>
    public class SensesReport
    {
        public IList<IEntity> VisibleEntities;
        public IList<float> Hormones;
        public IEntity ThisEntity;
        public bool LastResult;
    }
}