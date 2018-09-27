using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andromedroids
{
    abstract class Weapon
    {
        public enum Type { GatlingGun, RocketLauncher, CaliberCannon, Minelayer, PlasmaThrower, GuidedMissile }

        public static Type[] StartingTypes => new Type[] { Type.GatlingGun, Type.RocketLauncher, Type.CaliberCannon };


    }
}
