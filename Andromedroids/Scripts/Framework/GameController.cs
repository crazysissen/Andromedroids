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

namespace Andromedroids
{
    class GameController
    {
        const float
            INTROTIME = 5.0f,
            ZOOMOUTTIME = 0.5f,
            CAMERAOFFSETPROPORTION = 7.0f / 18.0f,
            STARTZOOM = 0.1f,
            DEFAULTZOOM = 1 / 0.78f,
            ZOOMMULTIPLIER = 0.7071f;

        private XNAController controller;
        private MainController mainController;

        private int frameCount, state;
        private bool aiRunning;
        private float startCountdown;
        private PlayerManager[] players;
        private StatWindow[] statWindows;
        private Renderer.Sprite backgroundSquare;
        private GUI.Collection[] playerStatWindows;
        private Random r;
        private HashKey key;
        private System.Timers.Timer timer;
        private Song song;

        public GameController(HashKey key)
        {
            if (key.Validate("GameController Constructor"))
            {

                this.key = key;
            }
        }

        public void Initialize(XNAController controller, Song song, PlayerManager[] players, float wuRadius, float wuMinDistance, float wuMaxDistance)
        {
            this.song = song;
            this.controller = controller;
            this.players = players;
            r = new Random();

            MediaPlayer.Play(song);

            //Texture2D square = ContentController.Get<Texture2D>("Square");

            //for (float x = -wuRadius + 0.5f; x < wuRadius; ++x)
            //{
            //    for (float y = -wuRadius + 0.5f; y < wuRadius; ++y)
            //    {
            //        float aX = x + wuRadius, aY = y + wuRadius;

            //        if (((int)aX % 2 == 0 && (int)aY % 2 == 0) || ((int)aX % 2 == 1 && (int)aY % 2 == 1))
            //        {
            //            tiles.Add(new Renderer.Sprite(new Layer(MainLayer.Background, 0), square, new Vector2(x, y), Vector2.One * Camera.WORLDUNITPIXELS, new Color(0, 0, 15, 255), 0));
            //        }
            //    }
            //}

            float startRotation = (float)r.NextDouble() * (float)Math.PI * 2;
            Vector2 startPosition;

            do
            {
                startPosition = new Vector2((float)r.NextDouble() * wuMaxDistance * 2 - wuMaxDistance, (float)r.NextDouble() * wuMaxDistance * 2 - wuMaxDistance);
            }
            while (Vector2.Distance(Vector2.Zero, startPosition) < wuMinDistance);

            Debug.Write(startPosition);

            statWindows = new StatWindow[2];

            for (int i = 0; i < players.Length; ++i)
            {
                players[i].FW_Setup(key, i == 0 ? startPosition : -startPosition, i == 0 ? startRotation : startRotation - (float)Math.PI);
                statWindows[i] = new StatWindow(key, players[i], players[i].PlayerDecalColor, controller, new Rectangle(30, 30 + 440 * i, 240, 430));
            }

            backgroundSquare = new Renderer.Sprite(Layer.Default, ContentController.Get<Texture2D>("SquareMask"), Vector2.Zero, Camera.WORLDUNITPIXELS * Vector2.One * wuRadius, new Color(0, 0, 15, 100), 0, new Vector2(0.5f, 0.5f), SpriteEffects.None);
            aiRunning = true;

            state = 0;
        }

        public void StartGame()
        {
            timer.Elapsed += UpdateStatistics;
            timer = new System.Timers.Timer(500);
        }

        public void Update(MainController controller, XNAController game, GameTime gameTime, float deltaTimeScaled)
        {
            Vector2 distance = players[1].Player.Position - players[0].Player.Position;
            float playerDistance = distance.Length();

            Vector2 averagePosition = 0.5f * (players[0].Player.Position + players[1].Player.Position);
            float targetScale = /*(playerDistance / (distance.X > distance.Y ? distance.X : distance.Y)) * ZOOMMULTIPLIER * DEFAULTZOOM * playerDistance;*/
                DEFAULTZOOM * playerDistance * ZOOMMULTIPLIER;

            Camera camera = RendererController.GetCamera(key);
            camera.Position = averagePosition;

            switch (state)
            {
                // Intro screen
                case 0:

                    startCountdown += deltaTimeScaled;

                    if (startCountdown < INTROTIME)
                    {
                        float progress = startCountdown / INTROTIME;


                    }

                    if (startCountdown > INTROTIME && startCountdown < INTROTIME + ZOOMOUTTIME)
                    {
                        float
                            progress = (startCountdown - INTROTIME) / ZOOMOUTTIME,
                            sine = MathA.SineA(progress);

                        camera.Scale = 1 / STARTZOOM.Lerp(targetScale, progress) ;
                    }

                    if (startCountdown > INTROTIME + ZOOMOUTTIME)
                    {
                        camera.Scale = 1 / targetScale;
                        state = 1;
                    }

                    break;

                // Actual ingame loop
                case 1:

                    if (aiRunning)
                    {
                        ++frameCount;

                        foreach (PlayerManager player in players)
                        {
                            player.FW_Update(key, gameTime, deltaTimeScaled);
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

        void UpdateStatistics(object sender, System.Timers.ElapsedEventArgs e)
        {
            PlayerManager.TimeStats[] times = { players[0].FW_GetElapsedTimes(key, false), players[1].FW_GetElapsedTimes(key, false) };
        }
    }
}
