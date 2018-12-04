using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Andromedroids
{
    sealed class Weapon
    {
        public enum StartType
        {
            GatlingGun = 0, RocketLauncher = 1, CaliberCannon = 2
        }

        public enum Type
        {
            GatlingGun = 0, RocketLauncher = 1, CaliberCannon = 2/*, Minelayer = 3, PlasmaThrower = 4, GuidedMissile = 5, Nuclear = 6*/
        }

        static Texture2D[] readyWeaponTextures, cooldownWeaponTextures;

        public Type WeaponType { get; }
        public int PowerNeeded { get; private set; }
        public int PowerMaximum { get; private set; } //TODO

        private Renderer.Sprite renderer;
        private int team;

        public Weapon(HashKey key, Type type, Vector2 shipPosition, int slot, float rotation, int team)
        {
            if (key.Validate("Weapon Constructor"))
            {
                this.team = team;

                WeaponType = type;

                switch (WeaponType)
                {
                    case Type.GatlingGun:
                        PowerNeeded = 2;
                        PowerMaximum = 5;
                        break;
                    case Type.RocketLauncher:
                        PowerNeeded = 4;
                        PowerMaximum = 6;
                        break;
                    case Type.CaliberCannon:
                        PowerNeeded = 3;
                        PowerMaximum = 6;
                        break;
                }

                if (readyWeaponTextures == null)
                {
                    readyWeaponTextures = new Texture2D[3];
                    cooldownWeaponTextures = new Texture2D[3];

                    for (int i = 0; i < 3; i++)
                    {
                        readyWeaponTextures[i] = ContentController.Get<Texture2D>("Weapon" + ((Type)i).ToString() + 1);
                        cooldownWeaponTextures[i] = ContentController.Get<Texture2D>("Weapon" + ((Type)i).ToString() + 1);
                    }
                }

                renderer = new Renderer.Sprite(new Layer(MainLayer.Main, -1), cooldownWeaponTextures[(int)type], Vector2.Zero, Vector2.One, Color.White, 0, new Vector2(0, 1), SpriteEffects.None);
                SetPosition(shipPosition, slot, rotation);
            }
        }

        public void TryFire(int power)
        {
            if (power >= PowerNeeded)
            {
                
            }
        }

        public void SetPosition(Vector2 shipPosition, int slot, float rotation)
        {
            Point[] weaponOffsets =
            {
                new Point(9, -10), new Point(9, -1), new Point(9, 8),
                new Point(-9, -5), new Point(-9, 4), new Point(-9, 13)
            };


            Vector2 targetPosition = (weaponOffsets[slot].ToVector2() / Camera.WORLDUNITPIXELS * 2).Rotate(rotation);
            renderer.Position = shipPosition + targetPosition;
            renderer.Rotation = rotation + ((float)Math.PI * 0.5f) * (slot > 2 ? 3 : 1);
        }
    }
}
