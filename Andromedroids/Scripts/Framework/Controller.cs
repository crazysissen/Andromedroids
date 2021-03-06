﻿using Microsoft.Xna.Framework;
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
        private int currentWinner;
        private float transitionCountdown, transitionTarget;
        private bool returnToTournament, updatingTournament;
        private SoundEffect menuMusic, ingameMusic, tournamentMusic, targetSong;
        private StateManager stateManager;
        private Random r;
        private HashKey key;
        private List<PlayerManager> allPlayers, quickstartPlayers;
        private PlayerManager latestWinner;
        private XNAController controller;
        private GameState endState;

        // Main menu

        private Renderer.SpriteScreen menuBackground, transitionOverlay;
        private GUI.Collection quickStart, mainMenu;
        private PlayerList playerList;
        private PlayerManager[] transitionStartPlayers;
        private bool tournamentListActive, playerListActive;

        // Ingame variables

        GameController gameController;

        // Tournament

        Tournament tournament;

        // Win

        GUI.Collection winCollection;

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
            startButton.AddEffect(ContentController.Get<SoundEffect>("MenuBlipClick"));
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

                    if (updatingTournament)
                    {
                        tournament.Update(this, deltaTimeScaled);
                    }

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


            TransitionState(GameState.InGame, 1.0f, ActivateQuickstart);
        }

        private void ActivateQuickstart()
        {
            quickStart.Active = false;

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
                playerList.Initialize(allPlayers.ToArray(), new Point(40, 40), 2, StartConfirm);
            }
        }

        private void StartConfirm()
        {
            List<PlayerManager> startPlayers = new List<PlayerManager>();
            for (int i = 0; i < playerList.Values.Length; i++)
            {
                if (playerList.Values[i])
                {
                    startPlayers.Add(playerList.Players[i]);
                }
            }

            if (startPlayers.Count == 2)
            {
                transitionStartPlayers = startPlayers.ToArray();
                TransitionState(GameState.InGame, 1.0f, ActivateStart);
            }
        }

        private void ActivateStart()
        {
            mainMenu.Active = false;
            playerList.Collection.Active = false;

            PlayerManager[] players = new PlayerManager[2]
            {
                transitionStartPlayers[0].New(key),
                transitionStartPlayers[1].New(key)
            };

            StartGame(players, MAPRADIUS, MINSPAWNDISTANCE, MAXSPAWNDISTANCE, false);
        }

        private void Tournament()
        {
            if (playerList != null)
            {
                playerList.Initialize(allPlayers.ToArray(), new Point(XNAController.DisplayResolution.X - 394, 40), 0, TournamentConfirm);
            }
        }

        private void TournamentConfirm()
        {
            List<PlayerManager> tournamentPlayers = new List<PlayerManager>();
            for (int i = 0; i < playerList.Values.Length; i++)
            {
                if (playerList.Values[i])
                {
                    tournamentPlayers.Add(playerList.Players[i]);
                }
            }

            if (tournamentPlayers.Count >= 3)
            {
                transitionStartPlayers = tournamentPlayers.ToArray();
                TransitionState(GameState.Tournament, 1, ActivateTournament);
            }
        }

        private void ActivateTournament()
        {
            tournament = new Tournament(transitionStartPlayers, StartTournamentMatch, TournamentWin);

            mainMenu.Active = false;
            tournament.Collection.Active = true;
            updatingTournament = true;
        }

        private void ProceedTournament()
        {

        }

        public void StartTournamentMatch(PlayerManager p1, PlayerManager p2)
        {
            returnToTournament = true;

            transitionStartPlayers = new PlayerManager[] { p1.New(key), p2.New(key) };

            TransitionState(GameState.InGame, 0.5f, ActivateTournamentMatch);

            updatingTournament = false;
        }

        private void ActivateTournamentMatch()
        {
            tournament.Collection.Active = false;
            StartGame(transitionStartPlayers, MAPRADIUS, MINSPAWNDISTANCE, MAXSPAWNDISTANCE, false);
        }

        public void TournamentWin(PlayerManager winner)
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

        public void GameEnd(int winner, PlayerManager winnerManager)
        {
            currentWinner = winner;

            latestWinner = winnerManager;

            if (returnToTournament)
            {
                if (tournament.Bracket.Matches.Count == 1)
                {
                    TransitionState(GameState.End, 1, TournamentEndActivate);

                    return;
                }

                TransitionState(GameState.Tournament, 1, MatchEndActivate);

                return;
            }

            TransitionState(GameState.MainMenu, 1, MatchEndActivate);
        }

        private void TournamentEndActivate()
        {
            Sound.PlaySong(menuMusic);

            gameController.EndGame();
            gameController = null;

            Point res = XNAController.DisplayResolution;

            menuBackground.Texture = ContentController.Get<Texture2D>("Space1");

            Renderer.SpriteScreen win = new Renderer.SpriteScreen(new Layer(MainLayer.GUI, 11), ContentController.Get<Texture2D>("Win"), new Rectangle(40, res.Y / 2 - 140, 530, 120));
            Renderer.Text name = new Renderer.Text(new Layer(MainLayer.GUI, 11), ContentController.Get<SpriteFont>("Bold"), latestWinner.PlayerName, 90, 0, new Vector2(40, res.Y / 2), new Color(255, 150, 0, 255));

            winCollection = new GUI.Collection();

            winCollection.Add(win, name);
            RendererController.GUI.Add(winCollection);
        }

        public void MatchEndActivate()
        {
            gameController.EndGame();
            gameController = null;

            if (returnToTournament)
            {
                Sound.PlaySong(tournamentMusic);

                tournament.Collection.Active = true;
                tournament.MatchOver(currentWinner);
                updatingTournament = true;
            }
            else
            {
                Sound.PlaySong(menuMusic);

                mainMenu.Active = true;
                playerList.Collection.Active = true;
            }
        }
    }
}