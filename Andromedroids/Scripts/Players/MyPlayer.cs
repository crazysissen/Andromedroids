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
    // Don't remove this attribute, it's how the game will find your AI!
    // The string entered will be the menu display name, which is cropped to a maximum of 16 characters.
    // The bool entered will determine if it is to be eglible for a quickstart, there must be exactly two AIs labled this way for it to work.
    [ShipAI("My Player", true)]
    class MyPlayer : ShipPlayer
    {

        // Feel free to add your own member variables!

        static int count;

        /// <summary>
        /// Requested initially when the game starts. This is where you customize your ship.
        /// </summary>
        public override StartupConfig GetConfig()
        {
            count++;

            return new StartupConfig()
            {
                Name        = "My Player",              // Full ingame display name, 16 letters max
                ShortName   = "PLR-" + count,           // Short display name, 5 letters
                Description = "Description!",           // Ship description
                Class       = ShipClass.Scorpion,
                HullColor   = new Color(0x80_CF_CF_CF),
                DecalColor  = count == 1 ? Color.Blue : Color.Red,
                //= new Color(0, 0, 50),
                Weapons     = new Weapon.StartType[]    // 6 weapons, overflow will be ignored, lack will be filled with [Gatling Gun].
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
        /// Where you write your initialization logic. Called only once at the start of the game/round.
        /// Your AI gets a maximum of three seconds, and is disqualified if this is exceeded.
        /// </summary>
        public override void Initialize()
        {
            // Math, logic, etc.
        }

        /// <summary>
        /// Called once per frame, this is where you write the real-time logic for your ship. 
        /// All the input is sent through the return value Configuration, which in turn will affect your ship
        /// Keep in mind that slow code is disregarded; if you take too long you will skip frames, a potential disadvantage.
        /// Delta time is added up if you skip frames, and is 0.0f on frame one (divide by zero exceptions are possible).
        /// </summary>
        public override Configuration Update(float deltaTime)
        {
            // Math, logic, etc.

            // If the total power returned exceeds base.TotalPower all settings will default to zero.
            Configuration config = new Configuration()
            {
                thrusterPower = 0,
                shieldPower = 0,
                rotationPower = 0,

                targetRotation = 0,

                weaponPower = new int[]
                {
                    0, 0, 0, 0, 0, 0
                },

                weaponFire = new bool[]
                {
                    false, false, false, false, false, false
                }
            };

            return config;
        }

        /// <summary>
        /// Called when a new weapon is picked up, and requests the weapon slot to be filled/replaced.
        /// </summary>
        public override int ReplaceWeapon(Weapon.Type weaponType)
        {
            // Math, logic, etc.

            return 0;
        }

        /// <summary>
        /// Called when a powerup is collected.
        /// </summary>
        public override void PowerupActivation(Powerup powerupType)
        {
            
        }
    }
}
