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
            MAPRADIUS = 40,
            MINSPAWNDISTANCE = 12,
            MAXSPAWNDISTANCE = 12;

        private static CheatDetection cheat;

        private Action transitionStart;
        private float transitionCountdown, transitionTarget;
        private SoundEffect menuMusic, ingameMusic, tournamentMusic, targetSong;
        private StateManager stateManager;
        private Random r;
        private HashKey key;
        private List<PlayerManager> allPlayers, quickstartPlayers;
        private XNAController controller;
        private GameState endState;

        // Main menu

        private Renderer.SpriteScreen menuBackground, transitionOverlay;
        private GUI.Collection quickStart, mainMenu;
        private PlayerList playerList;
        private bool tournamentListActive, playerListActive;

        // Ingame variables

        GameController gameController;

        // Tournament



        public MainController(XNAController systemController)
        {

        }

        public void Initialize(XNAController systemController)
        {
            for (int i = 0; i < 65; i++)
            {
                Debug.WriteLine("[ " + i + "]");
                Debug.WriteLine("Highest: " + i.HighestPowerLessThanOrEqual(out int power1) + "\tPow: " + power1);
                Debug.WriteLine("Lowest: " + i.LowestPowerMoreThanOrEqual(out int power2) + "\tPow: " + power2);
            }

            key = new HashKey();
            r = new Random();
            cheat = new CheatDetection();
            stateManager = new StateManager(GameState.QuickMenu, 0);
            controller = systemController;

            RendererController.Initialize(key, systemController.Graphics, new Vector2(0, 0), 1f);
            Renderer.SetKey(key);

            allPlayers = new List<PlayerManager>();
            quickstartPlayers = new List<PlayerManager>();
            ingameMusic = ContentController.Get<SoundEffect>("Andromedroids");
            menuMusic = ContentController.Get<SoundEffect>("AndromedroidsMenu");
            tournamentMusic = ContentController.Get<SoundEffect>("AndromedroidsBright");

            Scoreboard board = Scoreboard.ImportFromFile("Hellothere");

            MediaPlayer.IsRepeating = true;

            Sound.Initialize();

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

                        PlayerManager current = new PlayerManager(key, controller, (ShipPlayer)Activator.CreateInstance(type));

                        current.FW_Setup(key, menuName);

                        allPlayers.Add(current);

                        if ((attribute as ShipAI).Quickstart)
                        {
                            quickstartPlayers.Add(current);
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
            quickStartButton.AddEffect(ContentController.Get<SoundEffect>("MenuBlipStart"));
            quickStartButton.OnClick += Quickstart;

            GUI.Button menuButton = new GUI.Button(new Rectangle(-72 + origin.X, 40 + origin.Y, 145, 80), ContentController.Get<Texture2D>("ButtonMenu"));
            menuButton.AddEffect(ContentController.Get<SoundEffect>("MenuBlipClick"));
            menuButton.OnClick += Menu;

            quickStart.Add(quickStartButton, menuButton);

            // Main menu

            mainMenu = new GUI.Collection()
            {
                Active = false
            };

            Renderer.SpriteScreen title = new Renderer.SpriteScreen(Layer.Default, ContentController.Get<Texture2D>("Icon"), new Rectangle(-348 + origin.X, -270 + origin.Y, 696, 120));

            GUI.Button startButton = new GUI.Button(new Rectangle(-78 + origin.X, -50 + origin.Y, 155, 80), ContentController.Get<Texture2D>("ButtonStart"));
            startButton.AddEffect(ContentController.Get<SoundEffect>("MenuBlipStart"));
            startButton.OnClick += Start;

            GUI.Button tournamentButton = new GUI.Button(new Rectangle(-163 + origin.X, 70 + origin.Y, 325, 80), ContentController.Get<Texture2D>("ButtonTournament"));
            tournamentButton.AddEffect(ContentController.Get<SoundEffect>("MenuBlipClick"));
            tournamentButton.OnClick += Tournament;

            GUI.Button exitButton = new GUI.Button(new Rectangle(-55 + origin.X, 190 + origin.Y, 110, 80), ContentController.Get<Texture2D>("ButtonExit"));
            exitButton.AddEffect(ContentController.Get<SoundEffect>("MenuBlipExit"));
            exitButton.OnClick += Exit;

            playerList = new PlayerList();

            mainMenu.Add(title, startButton, tournamentButton, exitButton, playerList.Collection);
            
            // Finalization

            RendererController.GUI.Add(quickStart, mainMenu);

            #endregion
        }

        public void LateInitialize(XNAController systemController)
        {

        }

        public void Update(XNAController systemController, GameTime gameTime, float deltaTimeScaled)
        {
            int state = stateManager.Peek;

            switch (stateManager.GameState)
            {
                case GameState.QuickMenu:



                    break;

                case GameState.MainMenu:



                    break;

                case GameState.InGame:

                    if (gameController != null)
                    {
                        gameController.Update(this, systemController, gameTime, deltaTimeScaled);
                    }

                    cheat.Execute();

                    break;

                case GameState.Tournament:



                    break;

                case GameState.StartGame:



                    break;

                case GameState.Transition:

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
                                Sound.PlaySong(targetSong);
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

                case GameState.Exit:


                    break;
            }


        }

        public void Draw(XNAController systemController, GraphicsDeviceManager graphics, SpriteBatch spriteBatch, GameTime gameTime, float deltaTimeScaled)
        {
            RendererController.Draw(key, graphics, spriteBatch, gameTime, deltaTimeScaled);
        }

        void StartGame(PlayerManager[] players, float wuRadius, float wuMinDistance, float wuMaxDistance, bool quickStart)
        {
            if (wuRadius < wuMaxDistance)
            {
                return;
            }

            gameController = new GameController(key);
            gameController.Initialize(controller, menuBackground, ingameMusic, ContentController.Get<Texture2D>("Space1"), players, wuRadius, wuMinDistance, wuMaxDistance, quickStart);
        }

        private void Quickstart()
        {
            Debug.WriteLine("Quickstart");

            quickStart.Active = false;

            TransitionState(GameState.InGame, 1f, ActivateQuickstart);
        }

        private void ActivateQuickstart()
        {
            PlayerManager[] players = new PlayerManager[2]
            {
                quickstartPlayers[0].New(key),
                quickstartPlayers[0].New(key)
            };

            StartGame(players, MAPRADIUS, MINSPAWNDISTANCE, MAXSPAWNDISTANCE, true);
        }

        private void Menu()
        {
            Debug.WriteLine("Main Menu");

            TransitionState(GameState.MainMenu, 0.35f, ActivateMenu);
        }

        private void ActivateMenu()
        {
            Sound.PlaySong(menuMusic);

            quickStart.Active = false;
            mainMenu.Active = true;
        }

        private void Start()
        {
            if (playerList != null)
            {
                playerList.Initialize(allPlayers.ToArray(), new Point(40, 40), 2);
            }
        }

        private void StartConfirm()
        {

        }

        private void ActivateStart()
        {

        }

        private void Tournament()
        {
            if (playerList != null)
            {
                playerList.Initialize(allPlayers.ToArray(), new Point(XNAController.DisplayResolution.X - 394, 40), 0);
            }
        }

        private void TournamentConfirm()
        {

        }

        private void ActivateTournament()
        {

        }

        private void Exit()
        {
            transitionOverlay.Texture = ContentController.Get<Texture2D>("Bye");

            (mainMenu.Members[3] as GUI.Button).Layer = new Layer(MainLayer.AbsoluteTop, 10);

            Sound.StopSong();

            TransitionState(GameState.Exit, 1, ActivateExit);
        }

        private void ActivateExit()
        {
            controller.Exit();
        }

        private void TransitionState(GameState endState, float transitionTime, Action stateStart)
        {
            this.endState = endState;

            transitionStart = stateStart;
            transitionTarget = transitionTime * 0.5f;
            transitionCountdown = 0;

            stateManager.SetGameState(GameState.Transition, 0);
        }

        public void GameEnd(int winner)
        {

        }

        private void CreatePlayerList()
        {

        }

        private void DestroyPlayerList()
        {

        }

        struct MenuPlayer
        {
            public string menuName;
            public Type playerType;
        }
    }
}