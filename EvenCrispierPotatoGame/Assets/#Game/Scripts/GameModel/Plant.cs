using Scripts.GameModel.AbstractGame;
using Scripts.GameModel.AbstractGame.AiDefinitions;

namespace Scripts.GameModel
{
    public class Plant : AbstractEntity
    {
        public Plant(IWorld world, int id, int species) : base(world, id, species, EntityKind.Plant, Sex.Male)
        {
        }

        public override void Update()
        {
            
        }
    }
}