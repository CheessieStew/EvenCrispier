namespace Scripts.GameModel.AbstractGame.AiDefinitions
{
    public enum EntityKind
    {
        Animal,
        Plant,
        Fruit
    }
   
    public enum State
    {
        Alive,
        Unconsious,
        Dead
    }

    public enum Sex
    {
        Male,
        Female
    }
    
    public interface IEntity
    {
        int Id { get; }
        int Species { get; }
        int Age { get; }
        EntityKind Kind { get; }
        Sex Sex { get; }
        State State { get; }
        int AppearanceCode { get; }
        Emotion Emotion { get; }
        Vector Position { get; }
    }

    public enum Emotion
    {
        Angry,
        Happy,
        Sad
    }

}