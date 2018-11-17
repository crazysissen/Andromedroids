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
    enum ShipClass { Hammerhead, Fighter }

    struct StartupConfig
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Color HullColor { get; set; }
        public Color DecalColor { get; set; }
        public ShipClass Class { get; set; }
        public ShipClassPrerequesite Prerequesite { get; set; }
        public Weapon.StartType[] Weapons { get; set; }

        public StartupConfig(ShipClass shipClass)
        {
            Prerequesite = new ShipClassPrerequesite(shipClass);

            Name = "";
            Description = "";
            HullColor = Color.White;
            DecalColor = Color.White;
            Class = shipClass;
            Weapons = new Weapon.StartType[Prerequesite.WeaponCount];
        }
    }

    struct ShipClassPrerequesite
    {
        public Texture2D Sprite { get; private set; }
        public int WeaponCount { get; private set; }
        public int Health { get; private set; }
        public int Energy { get; private set; }
        public float Speed { get; private set; }
        public float Acceleration { get; private set; }
        public float RotationalSpeed { get; private set; }
        public float RotationalAcceleration { get; private set; }

        public ShipClassPrerequesite(ShipClass shipClass)
        {
            switch (shipClass)
            {
                case ShipClass.Hammerhead:

                    Sprite = ContentController.Get<Texture2D>("Hammerhead");
                    WeaponCount = 6;
                    Health = 500;
                    Energy = 12;
                    Speed = 0.6f;
                    Acceleration = 0.2f;
                    RotationalSpeed = 20.0f;
                    RotationalAcceleration = 5.0f;
                    return;

                case ShipClass.Fighter:

                    Sprite = ContentController.Get<Texture2D>("Hammerhead");
                    WeaponCount = 5;
                    Health = 350;
                    Energy = 14;
                    Speed = 0.7f;
                    Acceleration = 0.4f;
                    RotationalSpeed = 30.0f;
                    RotationalAcceleration = 10.0f;
                    return;
            }

            throw new Exception("Tried to construct a nonexistent ship class.");
        }
    }
}
