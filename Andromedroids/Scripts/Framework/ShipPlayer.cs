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
    abstract class ShipPlayer
    {
        public string PlayerName { get; private set; }
        public string PlayerDescription { get; private set; }
        public Color PlayerColor { get; private set; }

        public abstract StartupConfig GetConfig();

        /// <summary>
        /// FRAMEWORK. NOT to be used in the AI. Will register as a cheat.
        /// </summary>
        public void FW_Setup(HashKey key)
        {
            if (key.Validate("ShipPlayer.Setup"))
            {
                StartupConfig config = GetConfig();
                ShipClassPrerequesite prerequesite = config.Prerequesite;
                Weapon.StartType[] weaponTypes = config.Weapons;

                PlayerName = config.Name;
                PlayerDescription = config.Description;
                PlayerColor = config.Color;

                return;
            }
        }
    }
}
