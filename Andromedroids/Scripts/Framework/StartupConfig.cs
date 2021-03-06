﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Andromedroids
{
    public enum ShipClass { Scorpion, Hadronic, Bulwark, Ironclad }

    public struct StartupConfig
    {
        const int
            WEAPONCOUNT = 6;

        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public ShipClass Class { get; set; }
        public Color HullColor { get; set; }
        public Color DecalColor { get; set; }
        public Weapon.StartType[] Weapons { get; set; }
    }
}
