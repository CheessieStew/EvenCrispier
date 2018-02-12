using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Api
{
    public interface IEntity
    {
        string BodyType { get; }
        float XPos { get; }
        float YPos { get; }
        int Id { get; }
        IList<EntityVariable> Variables { get; }
        string Name { get; }

        void Update();
        event Action UpdatePosition;
    }

    public class EntityVariable
    {
        public delegate object DescriptionAccessor();
        public string Name { get; }
        public object Value => _accessor();
        private DescriptionAccessor _accessor;

        public EntityVariable(string name, DescriptionAccessor accessor)
        {
            Name = name;
            _accessor = accessor;
        }
    }
}
