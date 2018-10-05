using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Andromedroids
{
    public class MainController
    {
        static CheatDetection cheat;
        static StateManager stateManager;

        Renderer.Sprite ship;
        Random r;
        int hashKey;

        public MainController()
        {
            cheat = new CheatDetection();

            r = new Random();
            hashKey = r.Next(0x1111, 0xFFFF);
            stateManager = new StateManager(GameState.QuickMenu, 0);
        }

        public void Initialize(XNAController systemController)
        {
            RendererController.Initialize(new Vector2(0, 0), 1f);
        }

        public void LateInitialize(XNAController systemController)
        {
            ship = new Renderer.Sprite(ContentController.Get<Texture2D>("Freighter"), new Vector2(5, 0), new Vector2(1, 1), 0);
        }

        public void Update(XNAController systemController, GameTime gameTime, float deltaTime)
        {
            ship.Position += Vector2.One * deltaTime;
            ship.Rotation += deltaTime;
        }

        public void Draw(XNAController systemController, GraphicsDeviceManager graphics, SpriteBatch spriteBatch, GameTime gameTime, float deltaTime)
        {
            RendererController.Draw(graphics, spriteBatch, gameTime);
        }

        public static void ReportCheat(string message)
        {
            stateManager.SetGameState(GameState.Cheat);
        }
    }
}
