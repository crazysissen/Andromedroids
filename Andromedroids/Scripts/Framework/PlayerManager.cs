using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Andromedroids
{
    public sealed class PlayerManager
    {
        const float
            BYTETOFLOAT = 1.0f / 255,
            HITDURATION = 0.35f,
            MAXACCELERATION = 0.8f,
            MAXSPEED = 1.0f,
            MAXROTATIONSPEED = 8.0f * MathA.DEGTORAD,
            ACCELERATIONPERPOWER = MAXACCELERATION / MAXTHRUSTERPOWER,
            DECELLERATIONPERSPEED = MAXACCELERATION / MAXSPEED,
            SHIELDREGENMULTUIPLIER = 1.0f / 5; // 1/X where X is how much power is needed for each shield/second

        const int
            HITALPHA = 150,
            MAXPOWER = 160,
            MAXSHIELD = 100,
            STARTSHIELD = 50,
            STARTHEALTH = 100,
            POWERBOOST = 8,
            MAXTHRUSTERPOWER = 6,
            MAXROTATIONPOWER = 4;

        readonly Color
            shieldMaxColor = new Color(160, 245, 255, 80),
            shieldMinColor = new Color(220, 220, 220, 25);

        public ShipPlayer Player { get; private set; }

        public int PlayerNumber { get; private set; }

        public int Health { get; private set; }
        public int Shield { get; private set; }

        public string PlayerName { get; private set; }
        public string ShortName { get; private set; }
        public string PlayerDescription { get; private set; }
        public Color PlayerHullColor { get; private set; }
        public Color PlayerDecalColor { get; private set; }
        public Texture2D PlayerTexture { get; private set; }

        Weapon.StartType[] _weaponTypes;
        private Weapon[] _weapons;
        private PlayerManager _opponent;
        private ManualResetEvent _frameStart = new ManualResetEvent(false);
        private XNAController _controller;
        private GameController _gameController;
        private Thread _playerThread, _startThread;
        private Renderer.Sprite _renderer, _shieldRenderer;
        private HashKey _key;
        private float _shieldRegenProgress, _hitEffectCooldown;
        private double _currentTimePeriod, _totalTime;
        private int _currentFrameCount, _totalFrameCount;
        private bool _inUpdate, _run, _firstFrame;
        private float[] _remainingPowerupTime;

        private Configuration latestConfig = Configuration.Empty;

        public PlayerManager(HashKey key, XNAController controller, ShipPlayer player)
        {
            if (key.Validate("PlayerManager Constructor"))
            {
                this._controller = controller;
                this._key = key;

                Player = player;
            }
        }

        public PlayerManager New(HashKey key)
        {
            if (key.Validate("PlayerManager.New"))
            {
                PlayerManager newPlayer = (PlayerManager)MemberwiseClone();

                newPlayer.Player = (ShipPlayer)Activator.CreateInstance(Player.GetType());

                return newPlayer;
            }

            return null;
        }

        /// <summary>
        /// FRAMEWORK. NOT to be used in the AI. Will register as a cheat.
        /// </summary>
        public void FW_Setup(HashKey key)
        {
            if (key.Validate("ShipPlayer.Setup"))
            {
                StartupConfig config = Player.GetConfig();

                _weaponTypes = config.Weapons;

                PlayerName = config.Name.Length <= 16 ? config.Name : config.Name.Substring(0, 16);
                ShortName = config.ShortName.Length <= 5 ? config.ShortName : config.ShortName.Substring(0, 5);
                PlayerDescription = config.Description;
                PlayerHullColor = config.HullColor;
                PlayerDecalColor = config.DecalColor;

                Texture2D texture = ContentController.Get<Texture2D>(config.Class.ToString());
                PlayerTexture = NewTexture(texture, PlayerHullColor, PlayerDecalColor);

                Debug.WriteLine(PlayerName + ": SETUP");
            }
        }

        public void FW_Initialize(HashKey key, Vector2 position, GameController gameController, PlayerManager opponentManager, float rotation, int playerNumber)
        {
            if (key.Validate("ShipPlayer.Initialize [Player:" + ShortName + "]"))
            {
                Debug.WriteLine(PlayerName + ": INITIALIZATION");

                _opponent = opponentManager;
                _gameController = gameController;

                CreateThread();

                PlayerNumber = playerNumber;

                Health = STARTHEALTH;
                Shield = STARTSHIELD;

                Player.SetRotation(key, rotation);
                Player.SetPosition(key, position);

                _weapons = new Weapon[6];
                for (int i = 0; i < 6; i++)
                {
                    _weapons[i] = new Weapon(key, (WeaponType)_weaponTypes[i], position, i, rotation, PlayerNumber, gameController);
                }

                _totalFrameCount = 0;
                _currentFrameCount = 0;
                _currentTimePeriod = 0;
                _totalTime = 0;
                _remainingPowerupTime = new float[4];
                _firstFrame = true;
                _renderer = new Renderer.Sprite(Layer.Default, PlayerTexture, position, Vector2.One, Color.White, rotation, new Vector2(0.5f, 0.5f), SpriteEffects.None);
                _shieldRenderer = new Renderer.Sprite(new Layer(MainLayer.Main, -1), ContentController.Get<Texture2D>("HitRadius"), Player.Position, Vector2.One * (Camera.WORLDUNITPIXELS / 300.0f), shieldMinColor, 0);

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

                // There should be no config for frame 1
                Configuration config = _firstFrame ? Configuration.Empty : latestConfig;

                // - Reading and verifying the values returned

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

                // - Updating weapons

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

                // - Updating velocity

                // Resistance evens out with acceleration at MAXSPEED if thrusters are at max power, otherwise the maximum speed will be lower
                Vector2 resistance = Player.Velocity * DECELLERATIONPERSPEED * scaledDeltaTime;
                Vector2 acceleration = ACCELERATIONPERPOWER * config.thrusterPower * (new Vector2(0, -1)).Rotate(Player.Rotation) * scaledDeltaTime;

                Player.SetVelocity(key, Player.Velocity + acceleration - resistance);

                // - Updating rotation

                float multiplier = config.targetRotation < 0 ? -1 : 1;
                float targetRotation = multiplier * (Math.Abs(config.targetRotation) % ((float)Math.PI * 2));

                if (targetRotation < 0)
                {
                    targetRotation += (float)Math.PI * 2;
                }

                float effectiveRotation = targetRotation - Player.Rotation;

                if (effectiveRotation > Math.PI)
                {
                    effectiveRotation -= (float)Math.PI * 2; 
                }

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

                Player.SetRotation(key, newRotation);

                // - Updating shields

                if (Shield >= MAXSHIELD)
                {
                    // Shield should not progress towards regen at 100 power
                    _shieldRegenProgress = 0;
                }
                else
                {
                    _shieldRegenProgress += scaledDeltaTime * SHIELDREGENMULTUIPLIER * shieldPower;

                    if (_shieldRegenProgress > 1)
                    {
                        --_shieldRegenProgress;
                        ++Shield;
                    }
                }

                // - Updating renderers

                _renderer.Position = Player.Position;
                _renderer.Rotation = Player.Rotation;

                float alphaLerp = 0;

                if (_hitEffectCooldown > 0)
                {
                    _hitEffectCooldown -= scaledDeltaTime;

                    alphaLerp = _hitEffectCooldown / HITDURATION;
                }

                if (_hitEffectCooldown < 0)
                {
                    _hitEffectCooldown = 0;

                    alphaLerp = 0;
                }

                float colorLerp = (float)Shield / MAXSHIELD;

                float colorAlpha = (shieldMinColor.A * BYTETOFLOAT).Lerp(shieldMaxColor.A * BYTETOFLOAT, colorLerp).Lerp(HITALPHA * BYTETOFLOAT, alphaLerp);

                _shieldRenderer.Position = Player.Position;
                _shieldRenderer.Color = new Color(Color.Lerp(shieldMinColor, shieldMaxColor, (float)Shield / MAXSHIELD), colorAlpha);

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

        public PlayerInfo FW_GetPlayerInfo(HashKey key)
        {
            if (key.Validate("Player.GetPlayerInfo [Player:" + ShortName + "]"))
            {
                return new PlayerInfo()
                {
                    config = latestConfig,
                    time = FW_GetElapsedTimes(key, false),
                    health = Health,
                    shield = Shield
                };
            }

            return new PlayerInfo();
        }

        public void FW_End(HashKey key)
        {
            if (key.Validate("PlayerManager.End"))
            {
                _run = false;
            }
        }

        public void Damage(HashKey key, int damage)
        {
            if (key.Validate("PlayerManager.Damage [Player:" + ShortName + "]"))
            {
                _hitEffectCooldown = HITDURATION;

                if (Shield > damage)
                {
                    Shield -= damage;
                    return;
                }

                Health -= damage - Shield;
            }
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

                _currentTimePeriod += elapsed;
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

        public struct PlayerInfo
        {
            public TimeStats time;
            public int shield, health;
            public Configuration config;
        }
    }
}
