using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// Don't change
namespace Andromedroids.Players 
{
    class TestPlayer : ShipPlayer
    {
        public TestPlayer()
        {
            base.
        }

        /// <summary>
        /// Requested initially when the game starts. This is where you customize your ship.
        /// </summary>
        public override StartupConfig GetConfig()
        {
            return new StartupConfig
            {
                Name        = "Test Player",
                Description = "Fitting description.",
                Color       = Color.Blue,
                Class       = ShipClass.Freighter,
                Weapons     = new Weapon.StartType[]
                {
                    Weapon.StartType.CaliberCannon,
                    Weapon.StartType.CaliberCannon,
                    Weapon.StartType.GatlingGun,
                    Weapon.StartType.GatlingGun,
                    Weapon.StartType.GatlingGun,
                    Weapon.StartType.GatlingGun
                }
            };
        }
    }
}
