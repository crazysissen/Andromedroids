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
        public bool Quickstart { get; private set; }

        public ShipAI(string menuName, bool quickstart = false)
        {
            MenuName = menuName;
            Quickstart = quickstart;
        }
    }

    /// <summary>Contains the player itself, and a bunch of information to be used in an AI.
    /// Remmber that changing these will bear no effect.</summary>
    abstract class ShipPlayer : ManagedWorldObject
    {
        public PlayerManager Manager { get; private set; }

        public bool FirstFrame { get; set; }
        public int TotalPower { get; set; }
        public int UnusedPower { get; set; }

        /// <summary>The time needed between shots for given weapon</summary>
        public float[] WeaponCooldown { get; set; }
        /// <summary>Remaining cooldown for given weapon</summary>
        public float[] WeaponCooldownRemaining { get; set; }
        /// <summary>Current power in the weapon</summary>
        public float[] WeaponPower { get; set; }
        /// <summary>How much power the weapon needs to work at full efficiency</summary>
        public float[] WeaponPowerNeeded { get; set; }
        /// <summary>If the weapon is ready to fire</summary>
        public bool[] WeaponReady { get; set; }

        /// <summary>Currently active powerups</summary>
        public Powerup[] ActivePowerups { get; set; }

        public abstract StartupConfig GetConfig();
        public abstract void Initialize();
        public abstract Configuration Update(float deltaTime);
        public abstract int ReplaceWeapon(Weapon.Type weaponType);
        public abstract void PowerupActivation(Powerup powerupType);
    }

    sealed class PlayerManager
    {
        public ShipPlayer Player { get; private set; }

        public string PlayerName { get; private set; }
        public string ShortName { get; private set; }
        public string PlayerDescription { get; private set; }
        public Color PlayerHullColor { get; private set; }
        public Color PlayerDecalColor { get; private set; }
        public Texture2D PlayerTexture { get; private set; }

        private ManualResetEvent frameStart = new ManualResetEvent(false);
        private XNAController controller;
        private Thread playerThread, startThread;
        private Renderer.Sprite renderer;
        private HashKey key;
        private double currentTimePeriod, totalTime;
        private int currentFrameCount, totalFrameCount;
        private bool updateFinish, run;

        public PlayerManager(HashKey key, XNAController controller, ShipPlayer player)
        {
            if (key.Validate("PlayerManager Constructor"))
            {
                this.controller = controller;
                this.key = key;

                Player = player;
            }
        }

        /// <summary>
        /// FRAMEWORK. NOT to be used in the AI. Will register as a cheat.
        /// </summary>
        public void FW_Setup(HashKey key, Vector2 position, float rotation)
        {
            if (key.Validate("ShipPlayer.Setup"))
            {
                StartupConfig config = Player.GetConfig();
                Weapon.StartType[] weaponTypes = config.Weapons;

                PlayerName = config.Name.Length <= 16 ? config.Name : config.Name.Substring(0, 16);
                ShortName = config.ShortName.Length <= 5 ? config.ShortName : config.ShortName.Substring(0, 5);
                PlayerDescription = config.Description;
                PlayerHullColor = config.HullColor;
                PlayerDecalColor = config.DecalColor;

                CreateThread();

                Player.SetRotation(key, rotation);
                Player.SetPosition(key, position);

                Texture2D texture = ContentController.Get<Texture2D>(config.Class.ToString());
                PlayerTexture = NewTexture(texture, PlayerHullColor, PlayerDecalColor);

                renderer = new Renderer.Sprite(Layer.Default, PlayerTexture, position, Vector2.One, Color.White, rotation, new Vector2(0.5f, 0.5f), SpriteEffects.None);

                Debug.WriteLine(PlayerName + ": SETUP");
            }
        }

        public void FW_Initialize(HashKey key)
        {
            if (key.Validate("ShipPlayer.Initialize"))
            {
                Debug.WriteLine(PlayerName + ": INITIALIZE");

                startThread = new Thread(Player.Initialize)
                {
                    Name = PlayerName + "[START THREAD]"
                };
                startThread.Start();
            }
        }

        public int FW_Start(HashKey key)
        {
            if (key.Validate("ShipPlayer.Start"))
            {
                Debug.WriteLine(PlayerName + ": START");

                CreateThread();
                playerThread.Start();

                run = true;

                Player.SetVelocity(key, Vector2.One);

                return playerThread.ManagedThreadId;
            }

            return 0;
        }

        /// <summary>
        /// FRAMEWORK. NOT to be used in the AI. Will register as a cheat.
        /// </summary>
        public void FW_Update(HashKey key, GameTime gameTime, float scaledDeltaTime)
        {
            if (key.Validate("ShipPlayer.Update"))
            {
                renderer.Position = Player.Position;
                renderer.Rotation = Player.Rotation;

                frameStart.Set();
                Thread.Sleep(2);
                frameStart.Reset();
            }
        }

        /// <summary>
        /// FRAMEWORK. NOT to be used in the AI. Will register as a cheat.
        /// </summary>
        public void FW_Draw(HashKey key)
        {
            if (key.Validate("ShipPlayer.Draw"))
            {
                
            }
        }

        /// <summary>
        /// FRAMEWORK. NOT to be used in the AI. Will register as a cheat.
        /// </summary>
        public TimeStats FW_GetElapsedTimes(HashKey key, bool total)
        {
            if (key.Validate("ShipPlayer.Draw"))
            {
                TimeStats returnValue;

                if (total)
                {
                    returnValue = new TimeStats()
                    {
                        time = totalTime + currentTimePeriod,
                        frameCount = totalFrameCount + currentFrameCount
                    };
                }
                else
                {
                    totalTime += currentTimePeriod;
                    totalFrameCount += currentFrameCount;

                    returnValue = new TimeStats()
                    {
                        time = currentTimePeriod,
                        frameCount = currentFrameCount
                    };

                    currentTimePeriod = 0.0;
                    currentFrameCount = 0;
                }

                return returnValue;
            }

            return new TimeStats();
        }

        private void RunUpdate()
        {
            Stopwatch 
                speedTimer = new Stopwatch(),
                deltaTimeTimer = null;

            while (run)
            {
                frameStart.WaitOne(1);

                speedTimer.Restart();

                Player.Update(GetTime(deltaTimeTimer));

                ++currentFrameCount;

                double elapsed = speedTimer.Elapsed.TotalMilliseconds;
            }

        }

        private static float GetTime(Stopwatch stopwatch)
        {
            if (stopwatch != null)
            {
                float time = (float)stopwatch.Elapsed.TotalSeconds;
                stopwatch.Restart();
                return time;
            }

            stopwatch = Stopwatch.StartNew();
            return 0.0f;
        }

        private void CreateThread()
        {
            playerThread = new Thread(RunUpdate)
            {
                Name = PlayerName
            };
        }

        private Texture2D NewTexture(Texture2D sprite, Color hullColor, Color decalColor)
        {
            Texture2D newTexture = new Texture2D(controller.GraphicsDevice, sprite.Width, sprite.Height);

            Color[] colors = new Color[sprite.Height * sprite.Width];
            sprite.GetData(colors);

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

                //colors[i] = Color.White;
            }

            newTexture.SetData(colors);

            return newTexture;
        }

        public struct TimeStats
        {
            public double time;
            public int frameCount;
        }
    }

    enum Powerup
    {
        /// <summary>Thruster speed multiplier</summary>
        Speed,

        /// <summary>Direct damage reduction</summary>
        Armor,

        /// <summary>Shield strength multiplier</summary>
        Shield,

        /// <summary>Steering and acceleration buff</summary>
        Maneuverability,

        /// <summary>Maximum power increase</summary>
        Power,

        /// <summary>Improved power redistribution speed</summary>
        PowerManagement
    }

    public struct Configuration
    {
        public int targetRotation, thrusterPower, rotationPower, shieldPower;
        public int[] weaponPower;
        public bool[] weaponFire;
    }
}
