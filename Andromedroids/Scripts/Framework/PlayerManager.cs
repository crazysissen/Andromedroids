﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Andromedroids
{
    public sealed class PlayerManager
    {
        const float
            MAXACCELERATION = 0.8f,
            MAXSPEED = 1.0f,
            MAXROTATIONSPEED = 8.0f * MathA.DEGTORAD,
            ACCELERATIONPERPOWER = MAXACCELERATION / MAXTHRUSTERPOWER,
            DECELLERATIONPERSPEED = MAXACCELERATION / MAXSPEED;

        const int
            MAXPOWER = 160,
            POWERBOOST = 8,
            MAXTHRUSTERPOWER = 6,
            MAXROTATIONPOWER = 4;

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

        private Weapon[] _weapons;
        private PlayerManager _opponent;
        private ManualResetEvent _frameStart = new ManualResetEvent(false);
        private XNAController _controller;
        private GameController _gameController;
        private Thread _playerThread, _startThread;
        private Renderer.Sprite _renderer;
        private HashKey _key;
        private double _currentTimePeriod, _totalTime;
        private int _currentFrameCount, _totalFrameCount;
        private bool _inUpdate, _run, _firstFrame;
        private float[] _remainingPowerupTime;

        private Configuration latestConfig = Configuration.Empty;

        public PlayerManager(HashKey key, XNAController controller, ShipPlayer player, int playerNumber)
        {
            if (key.Validate("PlayerManager Constructor"))
            {
                this._controller = controller;
                this._key = key;

                PlayerNumber = playerNumber;
                Player = player;
            }
        }

        /// <summary>
        /// FRAMEWORK. NOT to be used in the AI. Will register as a cheat.
        /// </summary>
        public void FW_Setup(HashKey key, GameController gameController, PlayerManager opponentManager, Vector2 position, float rotation)
        {
            if (key.Validate("ShipPlayer.Setup"))
            {
                _opponent = opponentManager;
                this._gameController = gameController;

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

                _weapons = new Weapon[6];
                for (int i = 0; i < 6; i++)
                {
                    _weapons[i] = new Weapon(key, (WeaponType)weaponTypes[i], position, i, rotation, PlayerNumber, gameController);
                }

                _remainingPowerupTime = new float[4];
                _firstFrame = true;
                _renderer = new Renderer.Sprite(Layer.Default, PlayerTexture, position, Vector2.One, Color.White, rotation, new Vector2(0.5f, 0.5f), SpriteEffects.None);

                Debug.WriteLine(PlayerName + ": SETUP");
            }
        }

        public void FW_Initialize(HashKey key)
        {
            if (key.Validate("ShipPlayer.Initialize [Player:" + ShortName + "]"))
            {
                Debug.WriteLine(PlayerName + ": INITIALIZE");

                _startThread = new Thread(Player.Initialize)
                {
                    Name = PlayerName + "[START THREAD]"
                };
                _startThread.Start();
            }
        }

        public int FW_Start(HashKey key)
        {
            if (key.Validate("ShipPlayer.Start [Player:" + ShortName + "]"))
            {
                Debug.WriteLine(PlayerName + ": START");

                CreateThread();
                _playerThread.Start();

                _run = true;

                return _playerThread.ManagedThreadId;
            }

            return 0;
        }

        /// <summary>
        /// FRAMEWORK. NOT to be used in the AI. Will register as a cheat.
        /// </summary>
        public void FW_Update(HashKey key, GameTime gameTime, float scaledDeltaTime, List<Bullet>[] allBullets)
        {
            if (key.Validate("ShipPlayer.Update [Player:" + ShortName + "]"))
            {
                // Executing the players choices first 

                #region Execution

                if (PlayerNumber == 1)
                    return;

                Configuration config = _firstFrame ? Configuration.Empty : latestConfig;

                float 
                    totalAvailablePower = PowerupActive(Powerup.Power) ? MAXPOWER + POWERBOOST : MAXPOWER, 
                    totalPower = config.rotationPower + config.shieldPower + config.thrusterPower + config.weaponPower.Sum();

                if (totalPower > totalAvailablePower)
                {
                    config = Configuration.Empty;
                }

                float
                    thrusterPower = config.thrusterPower.Clamp(0, MAXTHRUSTERPOWER),
                    shieldPower = config.shieldPower.Min(0),
                    rotationPower = config.rotationPower.Clamp(0, MAXROTATIONPOWER);

                // Updating weapons
                for (int i = 0; i < 6; ++i)
                {
                    _weapons[i].Power = config.weaponPower[i].Clamp(0, _weapons[i].PowerMaximum);

                    _weapons[i].SetPosition(Player.Position, i, Player.Rotation);
                    _weapons[i].Update(scaledDeltaTime);

                    if (config.weaponFire[i])
                    {
                        _weapons[i].TryFire(Player.Position, i, Player.Rotation);
                    }
                }

                // Resistance evens out with acceleration at MAXSPEED if thrusters are at max power, otherwise the maximum speed will be lower
                Vector2 resistance = Player.Velocity * DECELLERATIONPERSPEED * scaledDeltaTime;
                Vector2 acceleration = ACCELERATIONPERPOWER * config.thrusterPower * (new Vector2(0, -1)).Rotate(Player.Rotation) * scaledDeltaTime;

                Player.SetVelocity(key, Player.Velocity + acceleration - resistance);

                float multiplier = config.targetRotation < 0 ? -1 : 1;
                float targetRotation = multiplier * (Math.Abs(config.targetRotation) % ((float)Math.PI * 2));

                if (targetRotation < 0)
                {
                    targetRotation += (float)Math.PI * 2;
                }

                float effectiveRotation = targetRotation - Player.Rotation;
                float possibleRotation = MAXROTATIONSPEED * (config.rotationPower / MAXROTATIONPOWER) * scaledDeltaTime;
                float newRotation = Player.Rotation + effectiveRotation.Clamp(-possibleRotation, possibleRotation);

                if (newRotation > Math.PI * 2)
                {
                    newRotation -= (float)Math.PI * 2;
                }

                if (newRotation < 0)
                {
                    newRotation += (float)Math.PI * 2;
                }

                Debug.WriteLine("New Rotation: " + newRotation);
                Player.SetRotation(key, newRotation);

                #endregion

                if (_inUpdate)
                {


                    return;
                }

                // Setting the player up for a new update cycle, always happens just before a new update is issued

                #region Variable Setup

                List<PowerupInfo> powerupInfo = UpdatePowerups(scaledDeltaTime);

                List<BulletInfo>[] bulletInfos = GetBulletInfos(allBullets);

                WeaponType[] opponentWeapons = new WeaponType[6], playerWeapons = new WeaponType[6];
                float[] weaponPowerMin = new float[6], weaponPowerMax = new float[6];
                float[] weaponCooldown = new float[6], weaponCooldownRemaining = new float[6];
                bool[] weaponReady = new bool[6];

                float unusedPower = totalPower - config.thrusterPower + config.rotationPower + config.shieldPower;

                for (int i = 0; i < 6; i++)
                {
                    if (_weapons[i] != null)
                    {
                        unusedPower -= config.weaponPower[i];

                        playerWeapons[i] = _weapons[i].WeaponType;
                        weaponPowerMax[i] = _weapons[i].PowerMaximum;
                        weaponCooldown[i] = _weapons[i].MaxCooldown;
                        weaponCooldownRemaining[i] = _weapons[i].Cooldown;
                        weaponReady[i] = weaponCooldownRemaining[i] <= 0;
                    }

                    if (_opponent._weapons[i] != null)
                    {
                        opponentWeapons[i] = _opponent._weapons[i].WeaponType;
                    }
                }

                Player.OpponentHealth = _opponent.Health;
                Player.OpponentShield = _opponent.Shield;
                Player.OpponentPosition = _opponent.Player.Position;
                Player.OpponentRotation = _opponent.Player.Rotation;
                Player.OpponentVelocity = _opponent.Player.Velocity;
                Player.OpponentWeapons = opponentWeapons;

                Player.FriendlyBullets = bulletInfos[PlayerNumber].ToArray();
                Player.OpponentBullets = bulletInfos[(PlayerNumber + 1) % 2].ToArray();

                Player.FirstFrame = _firstFrame;
                Player.UnusedPower = unusedPower;
                Player.TotalPower = totalPower;

                Player.WeaponType = playerWeapons;
                Player.WeaponPowerMax = weaponPowerMax;
                Player.WeaponReady = weaponReady;

                #endregion

                _renderer.Position = Player.Position;
                _renderer.Rotation = Player.Rotation;

                // Executing update cycle in AI

                #region Frame Trigger

                _frameStart.Set();
                Thread.Sleep(1);
                _frameStart.Reset();

                #endregion

                _firstFrame = false;
            }
        }

        /// <summary>
        /// FRAMEWORK. NOT to be used in the AI. Will register as a cheat.
        /// </summary>
        public void FW_Draw(HashKey key)
        {
            if (key.Validate("ShipPlayer.Draw [Player:" + ShortName + "]"))
            {
                
            }
        }

        /// <summary>
        /// FRAMEWORK. NOT to be used in the AI. Will register as a cheat.
        /// </summary>
        public TimeStats FW_GetElapsedTimes(HashKey key, bool total)
        {
            if (key.Validate("ShipPlayer.GetElapsedTimes [Player:" + ShortName + "]"))
            {
                TimeStats returnValue;

                if (total)
                {
                    returnValue = new TimeStats()
                    {
                        time = _totalTime + _currentTimePeriod,
                        frameCount = _totalFrameCount + _currentFrameCount
                    };
                }
                else
                {
                    _totalTime += _currentTimePeriod;
                    _totalFrameCount += _currentFrameCount;

                    returnValue = new TimeStats()
                    {
                        time = _currentTimePeriod,
                        frameCount = _currentFrameCount
                    };

                    _currentTimePeriod = 0.0;
                    _currentFrameCount = 0;
                }

                return returnValue;
            }

            return new TimeStats();
        }

        public void Damage(HashKey key, int damage)
        {
            if (key.Validate("PlayerManager.Damage [Player:" + ShortName + "]"))
            Health -= damage;
        }

        private void RunUpdate()
        {
            Stopwatch 
                speedTimer = new Stopwatch(),
                deltaTimeTimer = null;

            while (_run)
            {
                _inUpdate = false;

                _frameStart.WaitOne();

                _inUpdate = true;
                speedTimer.Restart();

                latestConfig = Player.Update(GetTime(deltaTimeTimer));

                ++_currentFrameCount;

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
            _playerThread = new Thread(RunUpdate)
            {
                Name = PlayerName
            };
        }

        private List<PowerupInfo> UpdatePowerups(float scaledDeltaTime)
        {
            List<PowerupInfo> returnList = new List<PowerupInfo>();

            for (int i = 0; i < _remainingPowerupTime.Length; i++)
            {
                _remainingPowerupTime[i] -= scaledDeltaTime;

                if (_remainingPowerupTime[i] < 0)
                {
                    _remainingPowerupTime[i] = 0;
                }

                returnList.Add(new PowerupInfo()
                {
                    Powerup = (Powerup)i,
                    RemainingTime = _remainingPowerupTime[i]
                });
            }

            return returnList;
        }

        private List<BulletInfo>[] GetBulletInfos(List<Bullet>[] allBullets)
        {
            List<BulletInfo>[] returnArray = new List<BulletInfo>[2];

            for (int i = 0; i < 2; i++)
            {
                returnArray[i] = new List<BulletInfo>();

                foreach (Bullet bullet in allBullets[i])
                {
                    returnArray[i].Add(new BulletInfo()
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

            return returnArray;
        }

        private Texture2D NewTexture(Texture2D sprite, Color hullColor, Color decalColor)
        {
            Texture2D newTexture = new Texture2D(_controller.GraphicsDevice, sprite.Width, sprite.Height);

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
            => _remainingPowerupTime[(int)type] > 0;

        public struct TimeStats
        {
            public double time;
            public int frameCount;
        }
    }
}
