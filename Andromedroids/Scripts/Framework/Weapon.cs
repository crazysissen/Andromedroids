using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andromedroids
{
    abstract class Weapon
    {
        public enum StartType
        {
            GatlingGun = 0, RocketLauncher = 1, CaliberCannon = 2
        }

        public enum Type
        {
            GatlingGun = 0, RocketLauncher = 1, CaliberCannon = 2/*, Minelayer = 3, PlasmaThrower = 4, GuidedMissile = 5, Nuclear*/
        }
    }
}
