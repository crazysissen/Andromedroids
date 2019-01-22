using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Andromedroids
{
    public class Bullet
    {
        const int
            BULLETTYPECOUNT = 3;

        public enum BulletType
        {
            GatlingBullet, StandardRocket, CaliberShot, Mine, PlasmaBolt, GuidedRocket, Nuke
        }

        static Texture2D speck;
        static Texture2D[] bulletTextures;

        public float HitRadius { get; set; } = 0.05f;
        public float Rotation { get; set; }
        public int Damage { get; set; }
        //public bool Guided { get; set; }
        //public float? Timer { get; set; }
        public float? BlastRadius { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Position { get; set; }
        public BulletType Type { get; set; }

        private Renderer.Sprite renderer;
        private GameController controller;
        private HashKey key;
        //private float remainingTime;

        public Bullet(HashKey key, BulletType type, Vector2 position, Vector2 velocity, int damage, float rotation, GameController controller)
        {
            if (key.Validate("Bullet Constructor"))
            {
                this.key = key;
                this.controller = controller;

                Damage = damage;
                Position = position;
                Velocity = velocity;
                Rotation = rotation;

                if (speck == null)
                {
                    speck = ContentController.Get<Texture2D>("Speck");
                }

                if (bulletTextures == null)
                {
                    bulletTextures = new Texture2D[BULLETTYPECOUNT];

                    for (int i = 0; i < bulletTextures.Length; i++)
                    {
                        bulletTextures[i] = ContentController.Get<Texture2D>(((BulletType)i).ToString());
                    }
                }

                renderer = new Renderer.Sprite(new Layer(MainLayer.Main, 1), bulletTextures[(int)type], position, Vector2.One, Color.White, rotation, new Vector2(0.5f, 0.5f), SpriteEffects.None);


            }
        }

        public void Update(float deltaTime, PlayerManager targetPlayer)
        {
            if (IsColliding(targetPlayer.Player))
            {
                Trigger(targetPlayer);
            }

            Position += Velocity * deltaTime;

            renderer.Position = Position;
            renderer.Rotation = Rotation;
        }

        public void Trigger(PlayerManager targetPlayer)
        {
            targetPlayer.FW_Damage(key, Damage);
            controller.RemoveBullet(this, (targetPlayer.PlayerNumber + 1) % 2);
            Dispose(); 
        }

        public void Dispose()
        {
            renderer.Destroy();
        }

        bool IsColliding(ShipPlayer player)
            => Vector2.Distance(player.Position, Position) < ShipPlayer.HITRADIUS + HitRadius;
    }

    public struct BulletInfo
    {
        public Bullet.BulletType Type { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }

        public int Damage { get; set; }
        public float? BlastRadius { get; set; }
        //public bool Guided { get; set; }
    }
}
