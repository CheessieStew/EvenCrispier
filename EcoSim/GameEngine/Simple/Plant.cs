using GameEngine.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Simple
{
    public abstract partial class Entity
    {
        private class Plant : Entity
        {
            public override int IntBodyType => 0;
            public int Mass;
            public int MassLeft { get; private set; }
            public override string BodyType => "Plant";
            public int RegrowthRate;
            

            public Plant()
            {
                AddDescription("MassLeft", () => MassLeft);
                AddDescription("MaxMass", () => Mass);
                AddDescription("RegrowthRate", () => RegrowthRate);                
            }

            public override void Update()
            {
                if (Alive && Health <= 0)
                    Die();
                if (Alive)
                {
                    MassLeft = Math.Max(Mass, MassLeft + RegrowthRate);
                }
                {
                    Health--;
                    if (Health < -20)
                        Vanish?.Invoke(this);
                }

            }

            protected override void Die()
            {
                base.Die();
                RegrowthRate = 0;
            }

            protected override int GetEaten(int biteSize, Entity e)
            {
                var chunk = Math.Max(MassLeft, biteSize);
                MassLeft -= chunk;
                return chunk;
            }
        }
        
    }
}
