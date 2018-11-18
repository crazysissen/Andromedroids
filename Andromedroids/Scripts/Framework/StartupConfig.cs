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
    enum ShipClass { Scorpion, Hadronic, Bulwark, Ironclad }

    struct StartupConfig
    {
        const int
            WEAPONCOUNT = 6;

        public string Name { get; set; }
        public string Description { get; set; }
        public Color HullColor { get; set; }
        public Color DecalColor { get; set; }
        public ShipClass Class { get; set; }
        public Weapon.StartType[] Weapons { get; set; }

        public StartupConfig(ShipClass shipClass)
        {
            Name = "";
            Description = "";
            HullColor = Color.White;
            DecalColor = Color.White;
            Class = shipClass;
            Weapons = new Weapon.StartType[WEAPONCOUNT];
        }
    }
}
