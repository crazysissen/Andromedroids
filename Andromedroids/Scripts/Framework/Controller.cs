using Microsoft.Xna.Framework;
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

        private static CheatDetection cheat;

        private StateManager stateManager;
        private Renderer.SpriteScreen menuBackground;
        private Random r;
        private HashKey key;
        private List<MenuPlayer> menuPlayers, quickstartPlayers;
        private System.Timers.Timer timer;
        private XNAController controller;

        private GUI.Collection quickStart, mainMenu;

        // Ingame variables

        private int frameCount;
        private bool aiRunning;
        private float startCountdown;
        private PlayerManager[] players;
        private Renderer.Sprite backgroundSquare;


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

            Scoreboard board = Scoreboard.ImportFromFile("Hellothere");

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

            menuBackground = new Renderer.SpriteScreen(new Layer(MainLayer.Background, -1), ContentController.Get<Texture2D>("Space3"), new Rectangle(0, res.Y / 2 - res.X / 2, res.X, res.X));

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

                    switch (stateManager.Peek())
                    {
                        // Intro screen
                        case 0:
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
                    }



                    break;

                case GameState.GameIntro:



                    break;

                case GameState.Tournament:



                    break;

                case GameState.StartGame:



                    break;

                case GameState.Cheat:



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

            stateManager.SetGameState(GameState.InGame, 0);

            backgroundSquare = new Renderer.Sprite(Layer.Default, ContentController.Get<Texture2D>("Square"), Vector2.Zero, RendererController.GetCamera(key).WorldUnitDiameter * Vector2.One * 20, new Color(0, 0, 20, 120), 0, new Vector2(0.5f, 0.5f), SpriteEffects.None);
            aiRunning = true;

            float startRotation = (float)r.NextDouble() * (float)Math.PI * 2;
            Vector2 startPosition;

            do
            {
                startPosition = new Vector2((float)r.NextDouble() * wuMaxDistance * 2 - wuMaxDistance, (float)r.NextDouble() * wuMaxDistance * 2 - wuMaxDistance);
            }
            while (Vector2.Distance(Vector2.Zero, startPosition) < wuMinDistance);

            Debug.Write(startPosition);

            for (int i = 0; i < players.Length; i++)
            {
                players[i].FW_Setup(key,  i == 0 ? startPosition : -startPosition, i == 0 ? startRotation : startRotation - (float)Math.PI);
            }
        }

        private void Quickstart()
        {
            Debug.WriteLine("Quickstart");

            quickStart.Active = false;

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

        struct MenuPlayer
        {
            public string menuName;
            public Type playerType;
        }

    }
}