using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Andromedroids
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    sealed class ShipAI : Attribute
    {
        public string MenuName { get; private set; }

        public ShipAI(string menuName)
        {
            MenuName = menuName;
        }
    }

    abstract unsafe class ShipPlayer : ManagedWorldObject
    {
        public string PlayerName { get; private set; }
        public string PlayerDescription { get; private set; }
        public Color PlayerHullColor { get; private set; }
        public Color PlayerDecalColor { get; private set; }
        public Texture2D PlayerTexture { get; private set; }


        private List<double> elapsedTimes;
        private Thread playerThread;
        private Renderer.Sprite renderer;
        private int framesSkipped = 0;

        protected abstract StartupConfig GetConfig();
        protected abstract void Initialize();
        protected abstract void Update();

        /// <summary>
        /// FRAMEWORK. NOT to be used in the AI. Will register as a cheat.
        /// </summary>
        public void FW_Setup(HashKey key, Vector2 position, float rotation)
        {
            if (key.Validate("ShipPlayer.Setup"))
            {
                elapsedTimes = new List<double>();

                StartupConfig config = GetConfig();
                ShipClassPrerequesite prerequesite = config.Prerequesite;
                Weapon.StartType[] weaponTypes = config.Weapons;

                PlayerName = config.Name;
                PlayerDescription = config.Description;
                PlayerHullColor = config.HullColor;
                PlayerDecalColor = config.DecalColor;

                CreateThread();

                Texture2D texture = prerequesite.Sprite;
                PlayerTexture = NewTexture(texture, PlayerHullColor, PlayerDecalColor);

                renderer = new Renderer.Sprite(PlayerTexture, position, Vector2.One, Color.White, rotation, new Vector2(0.5f, 0.5f), SpriteEffects.None);
            }
        }

        public void FW_Update(HashKey key, GameTime gameTime, float scaledDeltaTime)
        {
            if (key.Validate("ShipPlayer.Update"))
            {
                if (playerThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    framesSkipped++;
                    return;
                }

                CreateThread();
                playerThread.Start();
            }
        }

        private void RunUpdate()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Update();

            double elapsed = stopwatch.Elapsed.TotalMilliseconds;
        }

        public void FW_Draw(HashKey key)
        {
            if (key.Validate("ShipPlayer.Draw"))
            {

            }
        }

        public double FW_GetElapsedTimes()
        {
            double totalElapsedTimes = elapsedTimes.Sum();
            int count = elapsedTimes.Count;

            elapsedTimes.Clear();

            return totalElapsedTimes / count;
        }

        private void CreateThread()
        {
            playerThread = new Thread(Update)
            {
                Name = PlayerName
            };
        }

        private Texture2D NewTexture(Texture2D sprite, Color hullColor, Color decalColor)
        {
            Texture2D newSprite = sprite;
            Color[] colors = new Color[newSprite.Height * newSprite.Width];
            newSprite.GetData(colors);

            for (int i = 0; i < colors.Length; i++)
            {
                Color color = colors[i];

                if (color.A == 0)
                {
                    continue;
                }

                if (color == Color.Blue)
                {
                    colors[i] = hullColor;
                    continue;
                }

                if (color == Color.Red)
                {
                    colors[i] = decalColor;
                    continue;
                }

                colors[i] = Color.White;
            }

            newSprite.SetData(colors);
            return newSprite;
        }
    }
}
