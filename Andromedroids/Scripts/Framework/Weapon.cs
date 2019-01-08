using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Andromedroids
{
    public enum WeaponType
    {
        GatlingGun = 0, RocketLauncher = 1, CaliberCannon = 2/*, Minelayer = 3, PlasmaThrower = 4, GuidedMissile = 5, Nuclear = 6*/
    }

    public sealed class Weapon
    {
        public enum StartType
        {
            GatlingGun = 0, RocketLauncher = 1, CaliberCannon = 2
        }

        

        static Texture2D[] readyWeaponTextures, cooldownWeaponTextures;

        public WeaponType WeaponType { get; }

        /// <summary>Max ammount of power before rest is discarded</summary>
        public float PowerMaximum { get; private set; }
        /// <summary>Current ammount of power channelled into the weapon</summary>
        public float Power { get; set; }
        /// <summary>How many shots are fired per second at max power</summary>
        public float FireRateAtMax { get; private set; }
        /// <summary>Speed at which bullets are launched</summary>
        public float Spread { get; private set; }
        public int Damage { get; private set; }
        public float Velocity { get; private set; }
        public float Cooldown { get; private set; }
        public float MaxCooldown { get; private set; }

        private GameController _controller;
        private Renderer.Sprite _renderer;
        private HashKey _key;
        private int _team;
        private float _powerMaximumDivisor;

        public Weapon(HashKey key, WeaponType type, Vector2 shipPosition, int slot, float rotation, int team, GameController controller)
        {
            if (key.Validate("Weapon Constructor"))
            {
                _team = team;
                _key = key;
                _controller = controller;

                WeaponType = type;
                Cooldown = 1;

                switch (WeaponType)
                {
                    case WeaponType.GatlingGun:
                        Damage = 2;
                        Spread = 14.0f;
                        FireRateAtMax = 0.65f;
                        PowerMaximum = 5.0f;
                        Velocity = 4.0f;
                        break;

                    case WeaponType.RocketLauncher:
                        Damage = 8;
                        FireRateAtMax = 0.20f;
                        PowerMaximum = 6.0f;
                        Velocity = 1.8f;
                        break;

                    case WeaponType.CaliberCannon:
                        Damage = 10;
                        FireRateAtMax = 0.35f;
                        PowerMaximum = 6.0f;
                        Velocity = 2.5f;
                        break;
                }

                // Saves dividing every frame
                _powerMaximumDivisor = 1 / PowerMaximum;

                if (readyWeaponTextures == null)
                {
                    readyWeaponTextures = new Texture2D[3];
                    cooldownWeaponTextures = new Texture2D[3];

                    for (int i = 0; i < 3; i++)
                    {
                        readyWeaponTextures[i] = ContentController.Get<Texture2D>("Weapon" + ((WeaponType)i).ToString() + 1);
                        cooldownWeaponTextures[i] = ContentController.Get<Texture2D>("Weapon" + ((WeaponType)i).ToString() + 1);
                    }
                }

                _renderer = new Renderer.Sprite(new Layer(MainLayer.Main, -1), cooldownWeaponTextures[(int)type], Vector2.Zero, Vector2.One, Color.White, 0, new Vector2(0, 1), SpriteEffects.None);
                SetPosition(shipPosition, slot, rotation);
            }
        }

        public void Update(float deltaTime)
        {
            float decrease = deltaTime * FireRateAtMax * Power * _powerMaximumDivisor;

            Cooldown = Cooldown > decrease ? Cooldown - decrease : 0;
        }

        public void TryFire(Vector2 playerOrigin, int slot, float currentRotation)
        {
            if (Cooldown <= 0)
            {
                Fire(playerOrigin, slot, currentRotation);
                Cooldown = 1;
            }
        }

        public void Fire(Vector2 origin, int slot, float currentRotation)
        {
            Vector2[] weaponOffsets =
            {
                new Vector2(29, -7.5f), new Vector2(29, 1.5f), new Vector2(29, 10.5f),
                new Vector2(-29, -7.5f), new Vector2(-29, 1.5f), new Vector2(-29, 10.5f)
            };

            Vector2 position = origin + (weaponOffsets[slot] / Camera.WORLDUNITPIXELS * 2).Rotate(currentRotation);
            float rotation = ForwardRotation(currentRotation, slot);
            Bullet bullet = new Bullet(_key, (Bullet.BulletType)WeaponType, position, (new Vector2(0, -1)).Rotate(rotation) * Velocity, Damage, rotation, _controller);

            _controller.AddBullet(bullet, _team);
        }

        public void SetPosition(Vector2 shipPosition, int slot, float rotation)
        {
            Point[] weaponOffsets =
            {
                new Point(9, -10), new Point(9, -1), new Point(9, 8),
                new Point(-9, -5), new Point(-9, 4), new Point(-9, 13)
            };


            Vector2 targetPosition = (weaponOffsets[slot].ToVector2() / Camera.WORLDUNITPIXELS * 2).Rotate(rotation);
            _renderer.Position = shipPosition + targetPosition;
            _renderer.Rotation = ForwardRotation(rotation, slot);
        }

        public float ForwardRotation(float rotation, int slot) 
            => rotation + ((float)Math.PI) * (slot > 2 ? 1.5f : 0.5f);
    }
}
