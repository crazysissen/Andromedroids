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


        /// <summary>
        /// Requested initially when the game starts. This is where you customize your ship.
        /// </summary>
        public override StartupConfig GetConfig()
        {
            return new StartupConfig(ShipClass.Scorpion)
            {
                Name        = "My Player",              // Ingame display name
                Description = "Description.",           // Ship description
                HullColor   = new Color(200, 200, 200),
                DecalColor  = new Color(20, 20, 20),
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
        /// Where you write your initialization logic. Runs only once at the start of the game/round.
        /// You get to take as much time as you want here up to a maximum of five seconds, where the player is disqualified.
        /// </summary>
        public override void Initialize()
        {

        }

        /// <summary>
        /// Updated once per frame, this is where you write the real-time logic for your ship. 
        /// Keep in mind that slow code is disregarded; if you take too long you will skip frames, a major disadvantage.
        /// </summary>
        public override Configuration Update()
        {
            // Math, logic, 

            Configuration config = new Configuration()
            {

            };

            return config;
        }

        /// <summary>
        /// Activated when a new weapon is picked up, and requests the weapon slot to be filled/replaced.
        /// </summary>
        public override int ReplaceWeapon(Weapon.Type weaponType)
        {
            return 0;
        }

        /// <summary>
        /// Activated when a powerup is collected.
        /// </summary>
        public override void PowerupActivation(Powerup powerupType)
        {

        }
    }
}