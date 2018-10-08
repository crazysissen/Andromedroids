﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Andromedroids
{
    public class XNAController : Game
    {
        public static float TimeScale { get; set; }

        public static XNAController Singleton { get; private set; }
        public static GraphicsDeviceManager Graphics { get; private set; }
        public static SpriteBatch SpriteBatch { get; private set; }
        public static MainController MainController { get; private set; }

        public XNAController()
        {
            Singleton = this;
            Graphics = new GraphicsDeviceManager(this);
            MainController = new MainController();

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();

            MainController.Initialize(systemController: this);
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            ContentController.Initialize(Content, true);

            MainController.LateInitialize(this);
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            MainController.Update(this, gameTime, (float)gameTime.ElapsedGameTime.TotalSeconds * TimeScale);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(20, 20, 60));

            base.Draw(gameTime);

            MainController.Draw(this, Graphics, SpriteBatch, gameTime, deltaTimeScaled: (float)gameTime.ElapsedGameTime.TotalSeconds * TimeScale);
        }
    }
}
