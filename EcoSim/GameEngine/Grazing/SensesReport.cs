using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Grazing
{
    public class SensesReport
    {
        public float Hunger;
        public bool LastResult;
        public IList<Entity.EntityDescription> VisibleEntities;
        public Entity.EntityDescription ThisEntity;
    }

    
}
