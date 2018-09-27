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
    enum ShipClass { Freighter, Fighter }

    struct StartupConfig
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Color Color { get; private set; }
        public ShipClass Class { get; private set; }
        public Weapon.Type[] Weapons { get; private set; }
        public ShipClassPrerequesite Prerequesite { get; private set; }

        public StartupConfig(string name, string description, Color color, ShipClass shipClass, Weapon.Type[] types)
        {
            Prerequesite = new ShipClassPrerequesite(shipClass);

            Name = name;
            Description = description;
            Color = color;
            Class = shipClass;
            Weapons = new Weapon.Type[Prerequesite.WeaponCount];

            Weapon.Type[] allowedTypes = Weapon.StartingTypes;

            for (int i = 0; i < Weapons.Length; ++i)
            {
                if (i >= types.Length)
                {
                    Weapons[i] = Weapon.Type.GatlingGun;
                }

                Weapons[i] = types[i];
            }
        }
    }

    struct ShipClassPrerequesite
    {
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
                case ShipClass.Freighter:

                    WeaponCount = 6;
                    Health = 500;
                    Energy = 12;
                    Speed = 0.6f;
                    Acceleration = 0.2f;
                    RotationalSpeed = 20.0f;
                    RotationalAcceleration = 5.0f;
                    return;

                case ShipClass.Fighter:

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
