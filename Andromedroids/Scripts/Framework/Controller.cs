using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

namespace Andromedroids
{
    public class MainController
    {
        const int
            MAPRADIUS = 16,
            MINSPAWNDISTANCE = 6,
            MAXSPAWNDISTANCE = 12;

        const float
            INTROTIME = 5.0f,
            ZOOMOUTTIME = 0.5f,
            CAMERAOFFSETPROPORTION = 7.0f / 18.0f;

        private static CheatDetection cheat;

        private Action transitionStart;
        private float transitionCountdown, transitionTarget;
        private Song menuMusic, ingameMusic, tournamentMusic, targetSong;
        private StateManager stateManager;
        private Random r;
        private HashKey key;
        private List<MenuPlayer> menuPlayers, quickstartPlayers;
        private System.Timers.Timer timer;
        private XNAController controller;
        private GameState endState;

        // Main menu

        private Renderer.SpriteScreen menuBackground, transitionOverlay;
        private GUI.Collection quickStart, mainMenu;

        // Ingame variables

        private int frameCount;
        private bool aiRunning;
        private float startCountdown;
        private PlayerManager[] players;
        private StatWindow[] statWindows;
        private Renderer.Sprite backgroundSquare;
        private GUI.Collection[] playerStatWindows;

        public MainController(XNAController systemController)
        {

        }

        public void Initialize(XNAController systemController)
        {
            key = new HashKey();

            r = new Random();
            cheat = new CheatDetection();
            stateManager = new StateManager(GameState.QuickMenu, 0);
            timer = new System.Timers.Timer(500);
            controller = systemController;
            timer.Elapsed += UpdateStatistics;

            RendererController.Initialize(key, systemController.Graphics, new Vector2(0, 0), 1f);
            Renderer.SetKey(key);

            menuPlayers = new List<MenuPlayer>();
            quickstartPlayers = new List<MenuPlayer>();
            ingameMusic = ContentController.Get<Song>("Andromedroids");
            menuMusic = ContentController.Get<Song>("AndromedroidsMenu");
            tournamentMusic = ContentController.Get<Song>("AndromedroidsBright");

            Scoreboard board = Scoreboard.ImportFromFile("Hellothere");

            MediaPlayer.IsRepeating = true;

            // Importing all players using the ShipAI attribute
            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();

            foreach (Type type in allTypes)
            {
                Attribute[] attributes = type.GetCustomAttributes() as Attribute[];

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is ShipAI && type.IsSubclassOf(typeof(ShipPlayer)))
                    {
                        string menuName = (attribute as ShipAI).MenuName;

                        MenuPlayer menuPlayer = new MenuPlayer()
                        {
                            menuName = menuName,
                            playerType = type
                        };

                        menuPlayers.Add(menuPlayer);

                        if ((attribute as ShipAI).Quickstart)
                        {
                            quickstartPlayers.Add(menuPlayer);
                        }
                    }
                }
            }

            #region GUI Initialization

            Point res = XNAController.DisplayResolution;
            Point origin = (res.ToVector2() * 0.5f).ToPoint();
            Rectangle screenCover = new Rectangle(0, res.Y / 2 - res.X / 2, res.X, res.X);

            transitionOverlay = new Renderer.SpriteScreen(new Layer(MainLayer.AbsoluteTop, 0), ContentController.Get<Texture2D>("Space2"), screenCover, new Color(20, 20, 20, 0));

            menuBackground = new Renderer.SpriteScreen(new Layer(MainLayer.Background, -1), ContentController.Get<Texture2D>("Space3"), screenCover, new Color(0.8f, 0.8f, 0.8f, 1.0f));

            // Quickstart menu

            quickStart = new GUI.Collection();

            GUI.Button quickStartButton = new GUI.Button(new Rectangle(-145 + origin.X, -120 + origin.Y, 290, 80), ContentController.Get<Texture2D>("ButtonQuickstart"));
            quickStartButton.AddEffect(ContentController.Get<SoundEffect>("Menu Blip Start"));
            quickStartButton.OnClick += Quickstart;

            GUI.Button menuButton = new GUI.Button(new Rectangle(-72 + origin.X, 40 + origin.Y, 145, 80), ContentController.Get<Texture2D>("ButtonMenu"));
            menuButton.AddEffect(ContentController.Get<SoundEffect>("Menu Blip Click"));
            menuButton.OnClick += Menu;

            quickStart.Add(quickStartButton, menuButton);
            RendererController.GUI.Add(quickStart);

            // Main menu

            mainMenu = new GUI.Collection()
            {
                Origin = (XNAController.DisplayResolution.ToVector2() * 0.5f).ToPoint(),
                Active = false
            };

            #endregion
        }

        public void LateInitialize(XNAController systemController)
        {

        }

        public void Update(XNAController systemController, GameTime gameTime, float deltaTimeScaled)
        {
            switch (stateManager.GameState)
            {
                case GameState.QuickMenu:



                    break;

                case GameState.MainMenu:



                    break;

                case GameState.InGame:

                    RendererController.GetCamera(key).Scale *= (1 - deltaTimeScaled  * 0.3f);

                    switch (stateManager.Peek())
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

                            cheat.Execute();

                            break;

                        // Pause menu
                        case 2:
                            break;

                        // Disqualification
                        case 3:
                            break;
                    }



                    break;

                case GameState.Tournament:



                    break;

                case GameState.StartGame:



                    break;

                case GameState.Transition:

                    int state = stateManager.Peek();
                    transitionCountdown += deltaTimeScaled;

                    if (state == 0)
                    {
                        float sine = MathA.SineA(transitionCountdown / transitionTarget);

                        transitionOverlay.Color = new Color(transitionOverlay.Color, sine);

                        if (MediaPlayer.State == MediaState.Playing)
                        {
                            MediaPlayer.Volume = 1 - sine;
                        }

                        if (transitionCountdown > transitionTarget)
                        {
                            transitionStart?.Invoke();
                            stateManager.StackSubState(1);
                            transitionCountdown -= transitionTarget;

                            MediaPlayer.Volume = 1;

                            if (targetSong != null)
                            {
                                MediaPlayer.Play(targetSong);
                            }
                        }
                    }

                    if (state == 1)
                    {
                        float sine = 1 - MathA.SineD(transitionCountdown / transitionTarget);

                        transitionOverlay.Color = new Color(transitionOverlay.Color, sine);

                        if (transitionCountdown > transitionTarget)
                        {
                            stateManager.SetGameState(endState, 0);
                            transitionCountdown = 0;

                            transitionOverlay.Color = new Color(transitionOverlay.Color, 0.0f);
                        }
                    }

                    break;

                case GameState.End:



                    break;
            }


        }

        public void Draw(XNAController systemController, GraphicsDeviceManager graphics, SpriteBatch spriteBatch, GameTime gameTime, float deltaTimeScaled)
        {
            RendererController.Draw(key, graphics, spriteBatch, gameTime);
        }

        void Setup(XNAController systemController, MenuPlayer[] menuPlayers)
        {
            players = new PlayerManager[2] 
            {
                new PlayerManager(key, systemController, (ShipPlayer)Activator.CreateInstance(menuPlayers[0].playerType)),
                new PlayerManager(key, systemController, (ShipPlayer)Activator.CreateInstance(menuPlayers[1].playerType))
            };

            StartGame(ref players, MAPRADIUS, MINSPAWNDISTANCE, MAXSPAWNDISTANCE);
        }

        void UpdateStatistics(object sender, System.Timers.ElapsedEventArgs e)
        {
            PlayerManager.TimeStats[] times = { players[0].FW_GetElapsedTimes(key, false), players[1].FW_GetElapsedTimes(key, false) };
        }

        void StartGame(ref PlayerManager[] players, float wuRadius, float wuMinDistance, float wuMaxDistance)
        {
            if (wuRadius < wuMaxDistance)
            {
                return;
            }

            MediaPlayer.Play(ingameMusic);

            backgroundSquare = new Renderer.Sprite(Layer.Default, ContentController.Get<Texture2D>("SquareMask"), Vector2.Zero, Camera.WORLDUNITPIXELS * Vector2.One * wuRadius, new Color(0, 0, 15, 100), 0, new Vector2(0.5f, 0.5f), SpriteEffects.None);
            aiRunning = true;

            Texture2D square = ContentController.Get<Texture2D>("Square");

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
        }

        private void Quickstart()
        {
            Debug.WriteLine("Quickstart");

            quickStart.Active = false;

            TransitionState(GameState.InGame, 0.5f, ActivateQuickstart);
        }

        private void ActivateQuickstart()
        {
            players = new PlayerManager[2]
            {
                new PlayerManager(key, controller, (ShipPlayer)Activator.CreateInstance(quickstartPlayers[0].playerType)),
                new PlayerManager(key, controller, (ShipPlayer)Activator.CreateInstance(quickstartPlayers[0].playerType))
            };

            StartGame(ref players, MAPRADIUS, MINSPAWNDISTANCE, MAXSPAWNDISTANCE);
        }

        private void Menu()
        {
            Debug.WriteLine("Main Menu");

            quickStart.Active = false;
        }

        private void TransitionState(GameState endState, float transitionTime, Action stateStart)
        {
            this.endState = endState;

            transitionStart = stateStart;
            transitionTarget = transitionTime * 0.5f;
            transitionCountdown = 0;

            stateManager.SetGameState(GameState.Transition, 0);
        }

        struct MenuPlayer
        {
            public string menuName;
            public Type playerType;
        }

    }
}