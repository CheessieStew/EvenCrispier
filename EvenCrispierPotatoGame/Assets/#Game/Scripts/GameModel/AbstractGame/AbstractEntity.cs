using Scripts.GameModel.AbstractGame.AiDefinitions;

namespace Scripts.GameModel.AbstractGame
{
    public abstract class AbstractEntity : IEntity
    {
        public EntityKind Kind { get; }
        public int Id { get; }
        public int Species { get; }
        public Sex Sex { get; }
        public State State { get; protected set; }
        public int AppearanceCode { get; }
        public Emotion Emotion { get; protected set; }
        public Vector Position { get; protected set; }
        public int Age { get; protected set; }

        protected IWorld World;
        
        protected AbstractEntity(IWorld world, int id, int species, EntityKind entityKind, Sex sex, int appearanceCode)
        {
            World = world;
            Id = id;
            Species = species;
            Kind = entityKind;
            Sex = sex;
            AppearanceCode = appearanceCode;
        }

        public abstract void Update();
    }
}