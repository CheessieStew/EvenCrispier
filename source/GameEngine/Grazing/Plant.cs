using System;

namespace GameEngine.Grazing
{
    public abstract partial class Entity
    {
        private class Plant : Entity
        {
            public override string Name => "Thistle";
            public override int IntBodyType => 0;
            public override string BodyType => "Plant";
            public int RegrowthRate;
            

            public Plant()
            {
                AddDescription("RegrowthRate", () => RegrowthRate);
                AddDescription(" ", () => " ");
                AddDescription("Init. plant count", () => _world.Settings.InitialPlantCount);
                AddDescription("Max plant count", () => _world.Settings.MaxPlantCount);
                AddDescription("New plant frequency", () => _world.Settings.NewPlantFrequency);
                AddDescription("Max plant mass", () => _world.Settings.MaxPlantMass);
                AddDescription("Min plant mass", () => _world.Settings.MinPlantMass);
                AddDescription("Max growth rate", () => _world.Settings.MaxGrowthRate);
                AddDescription("Min growth rate", () => _world.Settings.MinGrowthRate);

            }

            public override void Update()
            {
                if (Alive && Mass <= 0)
                    Die();
                if (Alive)
                {
                    Mass = Math.Min(MaxMass, Mass + RegrowthRate);
                }
            }

            protected override void Die()
            {
                base.Die();
                Vanish?.Invoke(this);
            }

            protected override int GetEaten(int biteSize, Entity e)
            {
                var chunk = Math.Min(Mass, biteSize);
                Mass -= chunk;
                return chunk;
            }


            internal override void Kill()
            {
                Die();
                Vanish?.Invoke(this);
            }
        }
        
    }
}
