
namespace GameEngine.Api
{
    public abstract class EntityFactory
    {
        private int _nextId = 0;

        protected abstract IEntity MakeEntity(int id, IWorld world, params object[] args);
        public IEntity GetEntity(IWorld world, params object[] args) => MakeEntity(_nextId++, world, args);

        public void Reset()
        {
            _nextId = 0;
        }
    }
}
