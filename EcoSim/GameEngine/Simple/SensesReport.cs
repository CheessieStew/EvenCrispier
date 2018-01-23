using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Simple
{
    public class SensesReport
    {
        public float Health;
        public float Energy;

        public bool LastResult;
        public IList<Entity.EntityDescription> VisibleEntities;
        public Entity.EntityDescription ThisEntity;
    }

    
}
