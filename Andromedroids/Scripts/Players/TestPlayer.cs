using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Andromedroids
{
    // Don't remove, this is how the game will find your AI
    // The string entered will be the menu display name
    [ShipAI("Test Player")]
    class TestPlayer : ShipPlayer
    {
        /// <summary>
        /// Requested initially when the game starts. This is where you customize your ship.
        /// </summary>
        protected override StartupConfig GetConfig()
        {
            return new StartupConfig(ShipClass.Hammerhead)
            {
                Name        = "Test Player",
                Description = "Description.",
                HullColor   = new Color(200, 200, 200),
                DecalColor  = new Color(20, 20, 20),
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

        /// <summary>
        /// Where you write your initialization logic. Runs only once at the start of the game/round.
        /// </summary>
        protected override void Initialize()
        {

        }

        /// <summary>
        /// Updated once per frame, this is where you write the real-time logic for your ship. 
        /// Keep in mind that slow code isn't respected, if you take too long you will skip frames.
        /// </summary>
        protected override void Update()
        {

        }
    }
}
