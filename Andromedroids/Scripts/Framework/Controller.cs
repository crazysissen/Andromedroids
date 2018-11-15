using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

namespace Andromedroids
{
    public class MainController
    {
        private static CheatDetection cheat;

        private bool running;
        private StateManager stateManager;
        private Renderer.Sprite backgroundSquare;
        private ShipPlayer[] players;
        private Random r;
        private HashKey key;
        private List<MenuPlayer> menuPlayers;
        private System.Timers.Timer timer;

        public MainController(XNAController systemController)
        {

        }

        public void Initialize(XNAController systemController)
        {
            r = new Random();
            cheat = new CheatDetection();
            key = new HashKey();
            stateManager = new StateManager(GameState.QuickMenu, 0);
            timer = new System.Timers.Timer(500);
            timer.Elapsed += UpdateStatistics;

            RendererController.Initialize(systemController.Graphics, new Vector2(0, 0), 0.4f);

            Scoreboard board = Scoreboard.ImportFromFile("Hellothere");

            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();
            menuPlayers = new List<MenuPlayer>();

            foreach (Type type in allTypes)
            {
                Attribute[] attributes = type.GetCustomAttributes() as Attribute[];

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is ShipAI)
                    {
                        if (type.IsSubclassOf(typeof(ShipPlayer)))
                        {
                            string menuName = (attribute as ShipAI).MenuName;

                            menuPlayers.Add(
                                new MenuPlayer()
                                {
                                    menuName = menuName,
                                    playerType = type
                                });
                        }
                    }
                }
            }

            Setup(new MenuPlayer[] { menuPlayers[0], menuPlayers[0] });
        }

        public void LateInitialize(XNAController systemController)
        {
            //ship = new Renderer.Sprite(ContentController.Get<Texture2D>("Hammerhead"), new Vector2(5, 0), new Vector2(1, 1), 0);

        }

        public void Update(XNAController systemController, GameTime gameTime, float deltaTimeScaled)
        {
            //ship.Position += Vector2.One * deltaTimeScaled;
            //ship.Rotation += deltaTimeScaled;

            if (running)
            {
                foreach (ShipPlayer player in players)
                {
                    player.FW_Update(key, gameTime, deltaTimeScaled);
                }
            }

            cheat.Execute();
        }

        public void Draw(XNAController systemController, GraphicsDeviceManager graphics, SpriteBatch spriteBatch, GameTime gameTime, float deltaTimeScaled)
        {
            RendererController.Draw(graphics, spriteBatch, gameTime);
        }

        void Setup(MenuPlayer[] menuPlayers)
        {
            players = new ShipPlayer[2] 
            {
                (ShipPlayer)Activator.CreateInstance(menuPlayers[0].playerType),
                (ShipPlayer)Activator.CreateInstance(menuPlayers[1].playerType)
            };

            string[] names = new string[]
            {
                menuPlayers[0].menuName,
                menuPlayers[1].menuName
            };

            StartGame(ref players, names, 10, 2, 9);
        }

        void UpdateStatistics(object sender, System.Timers.ElapsedEventArgs e)
        {
            double[] times = { players[0].FW_GetElapsedTimes(), players[1].FW_GetElapsedTimes() };
        }

        void StartGame(ref ShipPlayer[] players, string[] playerNames, float wuRadius, float wuMinDistance, float wuMaxDistance)
        {
            if (wuRadius < wuMaxDistance)
            {
                return;
            }

            backgroundSquare = new Renderer.Sprite(ContentController.Get<Texture2D>("Square"), Vector2.Zero, RendererController.GetCamera(key).WorldUnitDiameter * Vector2.One * 20, new Color(0, 0, 20, 120), 0, new Vector2(0.5f, 0.5f), SpriteEffects.None);
            running = true;

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
                players[i].FW_Setup(key, i == 0 ? startPosition : -startPosition, i == 0 ? startRotation : startRotation - (float)Math.PI);
            }
        }

        struct MenuPlayer
        {
            public string menuName;
            public Type playerType;
        }
    }
}