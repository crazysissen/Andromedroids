using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Andromedroids
{
    sealed class PlayerManager
    {
        const float
            SPEEDDIMINISH = 0.85f;

        const int
            MAXPOWER = 16,
            POWERBOOST = 8;

        public ShipPlayer Player { get; private set; }

        public int PlayerNumber { get; private set; }

        public int Health { get; private set; }
        public float Shield { get; private set; }

        public string PlayerName { get; private set; }
        public string ShortName { get; private set; }
        public string PlayerDescription { get; private set; }
        public Color PlayerHullColor { get; private set; }
        public Color PlayerDecalColor { get; private set; }
        public Texture2D PlayerTexture { get; private set; }

        private Weapon[] weapons;
        private PlayerManager opponent;
        private ManualResetEvent frameStart = new ManualResetEvent(false);
        private XNAController controller;
        private Thread playerThread, startThread;
        private Renderer.Sprite renderer;
        private HashKey key;
        private double currentTimePeriod, totalTime;
        private int currentFrameCount, totalFrameCount;
        private bool inUpdate, run, firstFrame;
        private float[] remainingPowerupTime;

        private Configuration latestConfig;

        public PlayerManager(HashKey key, XNAController controller, ShipPlayer player, int playerNumber)
        {
            if (key.Validate("PlayerManager Constructor"))
            {
                this.controller = controller;
                this.key = key;

                PlayerNumber = playerNumber;
                Player = player;
            }
        }

        /// <summary>
        /// FRAMEWORK. NOT to be used in the AI. Will register as a cheat.
        /// </summary>
        public void FW_Setup(HashKey key, PlayerManager opponentManager, Vector2 position, float rotation)
        {
            if (key.Validate("ShipPlayer.Setup"))
            {
                opponent = opponentManager;

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

                weapons = new Weapon[6];
                for (int i = 0; i < 6; i++)
                {
                    weapons[i] = new Weapon(key, (Weapon.Type)weaponTypes[i], position, i, rotation, PlayerNumber);
                }

                remainingPowerupTime = new float[4];
                firstFrame = true;
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

                return playerThread.ManagedThreadId;
            }

            return 0;
        }

        /// <summary>
        /// FRAMEWORK. NOT to be used in the AI. Will register as a cheat.
        /// </summary>
        public void FW_Update(HashKey key, GameTime gameTime, float scaledDeltaTime, List<Bullet>[] allBullets)
        {
            if (key.Validate("ShipPlayer.Update"))
            {
                List<PowerupInfo> powerupInfo = new List<PowerupInfo>();
                for (int i = 0; i < remainingPowerupTime.Length; i++)
                {
                    remainingPowerupTime[i] -= scaledDeltaTime;

                    if (remainingPowerupTime[i] < 0)
                    {
                        remainingPowerupTime[i] = 0;
                    }

                    powerupInfo.Add(new PowerupInfo()
                    {
                        Powerup = (Powerup)i,
                        RemainingTime = remainingPowerupTime[i]
                    });
                }

                List<BulletInfo>[] bulletInfos = new List<BulletInfo>[2];
                for (int i = 0; i < 2; i++)
                {
                    bulletInfos[i] = new List<BulletInfo>();

                    foreach (Bullet bullet in allBullets[i])
                    {
                        bulletInfos[i].Add(new BulletInfo()
                        {
                            Type = bullet.Type,
                            Position = bullet.Position,
                            Velocity = bullet.Velocity,
                            Damage = bullet.Damage,
                            BlastRadius = bullet.BlastRadius
                            //Guided = bullet.Guided
                        });
                    }
                }

                Configuration config = latestConfig;
                int totalPower = PowerupActive(Powerup.Power) ? MAXPOWER + POWERBOOST : MAXPOWER;
                Weapon.Type[] opponentWeapons = new Weapon.Type[6];
                int[] weaponPowerMin = new int[6], weaponPowerMax = new int[6];
                float[] weaponCooldown = new float[6], weaponCooldownRemaining = new float[6];
                bool[] weaponReady = new bool[6];
                int unusedPower = totalPower;

                for (int i = 0; i < 6; i++)
                {
                    weapons[i].SetPosition(Player.Position, i, Player.Rotation);

                    if (opponent.weapons[i] != null)
                    {
                        opponentWeapons[i] = opponent.weapons[i].WeaponType;
                    }
                }

                Player.OpponentHealth = opponent.Health;
                Player.OpponentShield = opponent.Shield;
                Player.OpponentPosition = opponent.Player.Position;
                Player.OpponentRotation = opponent.Player.Rotation;
                Player.OpponentVelocity = opponent.Player.Velocity;
                Player.OpponentWeapons = opponentWeapons;

                Player.FriendlyBullets = bulletInfos[PlayerNumber].ToArray();
                Player.OpponentBullets = bulletInfos[(PlayerNumber + 1) % 2].ToArray();

                Player.FirstFrame = firstFrame;
                Player.TotalPower = totalPower;

                renderer.Position = Player.Position;
                renderer.Rotation = Player.Rotation;

                if (inUpdate)
                {


                    return;
                }

                frameStart.Set();
                Thread.Sleep(2);
                frameStart.Reset();
                firstFrame = false;
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
                inUpdate = false;

                frameStart.WaitOne(3);

                inUpdate = true;
                speedTimer.Restart();

                latestConfig = Player.Update(GetTime(deltaTimeTimer));

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

        public bool PowerupActive(Powerup type)
            => remainingPowerupTime[(int)type] > 0;

        public struct TimeStats
        {
            public double time;
            public int frameCount;
        }
    }
}
