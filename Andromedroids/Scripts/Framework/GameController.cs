using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace Andromedroids
{
    public class GameController
    {
        const float
            INTROTIME = 2.0f,
            ZOOMOUTTIME = 0.5f,
            EXTRAWAITTIME = 0.4F,
            STARTZOOM = 0.1f,
            DEFAULTZOOM = 1 / 0.85f,
            ZOOMMULTIPLIER = 0.7071f;

        const int
            CAMERAOFFSET = -10;

        private XNAController controller;
        private MainController mainController;

        private int frameCount, state, timeScale = 1;
        private bool running;
        private float startCountdown;
        private PlayerManager[] players;
        private List<Bullet>[] 
            bullets = new List<Bullet>[] { new List<Bullet>(), new List<Bullet>() },
            removeQueue = new List<Bullet>[] { new List<Bullet>(), new List<Bullet>() };
        private Random r;
        private HashKey key;
        private System.Timers.Timer timer;
        private Texture2D[] speedControlSprites;
        private SoundEffect song;
        private GUI.Button speedControl;

        private Renderer.Sprite backgroundSquare;
        private Renderer.SpriteScreen background;

        // Stat windows
        private GUI.Collection[] playerStatWindows;
        private StatWindow[] statWindows;
 
        // Small names
        private Renderer.SpriteScreen[] nameConnectors;
        private Renderer.Text[] nameTexts;

        public GameController(HashKey key)
        {
            if (key.Validate("GameController Constructor"))
            {
                this.key = key;
            }
        }

        public void Initialize(XNAController controller, Renderer.SpriteScreen backgroundRenderer, SoundEffect song, Texture2D background, PlayerManager[] players, float wuRadius, float wuMinDistance, float wuMaxDistance, bool quickStart)
        {
            TournamentBracket bracket = new TournamentBracket(new PlayerManager[] { players [0], players[0], players[0], players[0], players[0], players[0], players[0], players[0], players[0], players[0]} );

            this.song = song;
            this.controller = controller;
            this.players = players;
            this.background = backgroundRenderer;
            speedControlSprites = ContentController.GetRange<Texture2D>("Speed0", "Speed1", "Speed2", "Speed3");
            r = new Random();

            Point res = XNAController.DisplayResolution;

            // Play song
            Sound.PlaySong(song);

            // Set the background
            backgroundRenderer.Texture = background;

            // Setup suitable start position/rotation

            float startRotation = (float)r.NextDouble() * (float)Math.PI * 2;
            Vector2 startPosition;

            do
            {
                startPosition = new Vector2((float)r.NextDouble() * wuMaxDistance * 2 - wuMaxDistance, (float)r.NextDouble() * wuMaxDistance * 2 - wuMaxDistance);
            }
            while (Vector2.Distance(Vector2.Zero, startPosition) < wuMinDistance);

            Debug.WriteLine(startPosition);

            // Setup the stat windows visible in-game
            statWindows = new StatWindow[2];

            // Setup the speed control switch
            speedControl = new GUI.Button(new Rectangle(res.X - 130, 20, 110, 14), speedControlSprites[1], GUI.Button.DefaultColors(), GUI.Button.Transition.Switch, 0.05f);
            speedControl.OnClick += SpeedControlClick; 
            RendererController.GUI += speedControl;

            // Setup the out-of-bounds box
            backgroundSquare = new Renderer.Sprite(Layer.Default, ContentController.Get<Texture2D>("SquareMask"), Vector2.Zero, Camera.WORLDUNITPIXELS * Vector2.One * wuRadius, new Color(0, 0, 15, 100), 0, new Vector2(0.5f, 0.5f), SpriteEffects.None);

            running = true;
            state = 0;

            for (int i = 0; i < players.Length; i++)
            {
                // Initialize the players
                players[i].FW_Initialize(key, i == 0 ? startPosition : -startPosition, this, players[(i + 1) % 2], i == 0 ? startRotation : (startRotation + (float)Math.PI) % ((float)Math.PI * 2), i);

                // Initialize stat windows
                statWindows[i] = new StatWindow(key, players[i], players[i].PlayerDecalColor, controller, new Rectangle(30, 30 + 440 * i, 240, 430));
            }

            // Set up the text connectors
            nameTexts = new Renderer.Text[players.Length];
            nameConnectors = new Renderer.SpriteScreen[players.Length];
            for (int i = 0; i < players.Length; ++i)
            {
                nameConnectors[i] = new Renderer.SpriteScreen(new Layer(MainLayer.GUI, 0), ContentController.Get<Texture2D>("Square"), new Rectangle());
                nameTexts[i] = new Renderer.Text(new Layer(MainLayer.GUI, 0), ContentController.Get<SpriteFont>("Thin"), players[i].ShortName, 20, 0, Vector2.Zero);
            }

            DrawAbbreviations(RendererController.Camera);
        }

        public void StartGame()
        {
            timer = new System.Timers.Timer(500);
            timer.Elapsed += UpdateStatistics;

            foreach (PlayerManager player in players)
            {
                player.FW_Start(key);
            }
        }

        public void EndGame()
        {

        }

        public void Update(MainController controller, XNAController game, GameTime gameTime, float deltaTimeScaled)
        {
            Vector2 distance = players[1].Player.Position - players[0].Player.Position;
            float playerDistance = distance.Length();

            Vector2 averagePosition = 0.5f * (players[0].Player.Position + players[1].Player.Position);

            float targetScale = 1 / (ZOOMMULTIPLIER * DEFAULTZOOM * playerDistance);

            Camera camera = RendererController.GetCamera(key);
            camera.Position = averagePosition + camera.ScreenToWorldSize(new Vector2(CAMERAOFFSET, 0));

            switch (state)
            {
                // Intro screen
                case 0:

                    startCountdown += deltaTimeScaled;

                    DrawAbbreviations(RendererController.Camera);

                    if (startCountdown < INTROTIME)
                    {
                        float progress = startCountdown / INTROTIME;


                    }

                    if (startCountdown > INTROTIME && startCountdown < INTROTIME + ZOOMOUTTIME)
                    {
                        float
                            progress = (startCountdown - INTROTIME) / ZOOMOUTTIME,
                            sine = MathA.SineA(progress);

                        camera.Scale = STARTZOOM.Lerp(targetScale, progress); 
                    }

                    if (startCountdown > INTROTIME + ZOOMOUTTIME && startCountdown < INTROTIME + ZOOMOUTTIME + EXTRAWAITTIME)
                    {
                        camera.Scale = targetScale; 
                    }

                    if (startCountdown > INTROTIME + ZOOMOUTTIME + EXTRAWAITTIME)
                    {
                        camera.Scale = targetScale;
                        state = 1;

                        StartGame();
                    }

                    break;

                // Ingame loop
                case 1:

                    if (running)
                    {
                        ++frameCount;

                        DrawAbbreviations(RendererController.Camera);

                        camera.Scale = targetScale;

                        ManagedWorldObject.UpdateAll(key, deltaTimeScaled);

                        for (int i = 0; i < 2; i++)
                        {
                            foreach (Bullet bullet in bullets[i])
                            {
                                bullet.Update(deltaTimeScaled, players[(i + 1) % 2]);
                            }

                            foreach (Bullet bullet in removeQueue[i])
                            {
                                bullets[i].Remove(bullet);
                            }

                            removeQueue[i].Clear();
                        }

                        foreach (PlayerManager player in players)
                        {
                            if (player.Health <= 0 || player)
                            {
                                player.FW_End(key);
                                break;
                            }

                            player.FW_Update(key, gameTime, deltaTimeScaled, bullets);
                        }
                    }

                    break;

                // Pause menu
                case 2:
                    break;

                // Disqualification
                case 3:
                    break;
            }
        }

        public void AddBullet(Bullet bullet, int team)
        {
            bullets[team].Add(bullet);
        }

        public void RemoveBullet(Bullet bullet, int team)
        {
            removeQueue[team].Add(bullet);
        }

        private void DrawAbbreviations(Camera camera)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 sPosition = camera.WorldToScreenPosition(players[i].Player.Position);

                nameConnectors[i].Transform = new Rectangle((int)sPosition.X, (int)sPosition.Y - 85, 2, 50);
                nameTexts[i].Position = new Vector2((int)sPosition.X + 5, (int)sPosition.Y - 85);
            }
        }

        private void SpeedControlClick()
        {
            timeScale = (timeScale + 1) % 4;

            controller.SetSpeed(key, (TimeScale)(timeScale));

            speedControl.Texture = speedControlSprites[timeScale];
        }

        private void UpdateStatistics(object sender, System.Timers.ElapsedEventArgs e)
        {
            PlayerManager.TimeStats[] times = { players[0].FW_GetElapsedTimes(key, false), players[1].FW_GetElapsedTimes(key, false) };
        }
    }
}