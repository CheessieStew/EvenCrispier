using Scripts.GameModelDefinitions;
using Scripts.GameModelDefinitions.Ai;

namespace Scripts.FirstModel
{
    public class Plant : AbstractEntity
    {
        public Plant(World world, int id, int species, string appearanceCode) : base(world, id, species, EntityKind.Plant, Sex.Male, appearanceCode)
        {
        }

        public override void Update()
        {
            
        }
    }
}