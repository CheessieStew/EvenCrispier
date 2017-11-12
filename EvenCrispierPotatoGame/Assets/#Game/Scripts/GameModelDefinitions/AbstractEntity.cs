using Scripts.GameModelDefinitions.Ai;

namespace Scripts.GameModelDefinitions
{
    public abstract class AbstractEntity : IEntity
    {
        public Instruction LastInstruction { get; protected set; }
        public EntityKind Kind { get; }
        public int Id { get; }
        public int Species { get; }
        public Sex Sex { get; }
        public State State { get; protected set; }
        public string AppearanceCode { get; }
        public Vector Position { get; protected set; }
        public int Age { get; protected set; }
        public int MassLeft;

        protected AbstractWorld World;
        
        protected AbstractEntity(AbstractWorld world, int id, int species, EntityKind entityKind, Sex sex, string appearanceCode)
        {
            World = world;
            Id = id;
            Species = species;
            Kind = entityKind;
            Sex = sex;
            AppearanceCode = appearanceCode;
        }

        public abstract void Update();

        public void RemoveEntity()
        {
            World.RemoveEntity(this);
        }
    }
}