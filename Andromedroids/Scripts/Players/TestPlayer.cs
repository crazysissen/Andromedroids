using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Andromedroids.Scripts.Players
{
    class TestPlayer : ShipPlayer
    {
        /// <summary>
        /// Requested initially when the game starts. This is where you customize your ship.
        /// </summary>
        public override StartupConfig GetConfig()
        {
            return new StartupConfig
            (
                name:           "Test Player",
                description:    "This is the test player.",
                color:          Color.Blue,
                shipClass:      ShipClass.Freighter,
                types:          new Weapon.Type[] 
                {
                    Weapon.Type.CaliberCannon,
                    Weapon.Type.CaliberCannon,
                    Weapon.Type.GatlingGun,
                    Weapon.Type.GatlingGun,
                    Weapon.Type.GatlingGun,
                    Weapon.Type.GatlingGun
                }
            );
        }
    }
}
