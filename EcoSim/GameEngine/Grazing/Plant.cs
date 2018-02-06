using GameEngine.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Grazing
{
    public abstract partial class Entity
    {
        private class Plant : Entity
        {
            public override int IntBodyType => 0;
            public override string BodyType => "Plant";
            public int RegrowthRate;
            

            public Plant()
            {
                AddDescription("RegrowthRate", () => RegrowthRate);                
            }

            public override void Update()
            {
                if (Alive && Mass <= 0)
                    Die();
                if (Alive)
                {
                    Mass = Math.Min(MaxMass, Mass + RegrowthRate);
                }
                {
                    Mass--;
                    if (Mass < -20)
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
