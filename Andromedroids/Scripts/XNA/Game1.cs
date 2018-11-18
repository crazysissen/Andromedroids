using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Andromedroids
{
    public class XNAController : Game
    {
        public static float TimeScale { get; set; } = 1;
        public static Point DisplayResolution => new Point(singleton.Graphics.PreferredBackBufferWidth, singleton.Graphics.PreferredBackBufferHeight);

        public GraphicsDeviceManager Graphics { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }
        public MainController MainController { get; private set; }

        private static XNAController singleton;

        public XNAController()
        {
            singleton = this;
            Graphics = new GraphicsDeviceManager(this);
            MainController = new MainController(this);

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();

            Mouse.SetCursor(MouseCursor.Crosshair);

            //Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            //Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            Graphics.PreferredBackBufferWidth = 1920;
            Graphics.PreferredBackBufferHeight = 1080;
            Graphics.ApplyChanges();

            //Graphics.ToggleFullScreen();

            IsMouseVisible = true;

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
            GraphicsDevice.Clear(new Color(30, 0, 60));

            base.Draw(gameTime);

            MainController.Draw(this, Graphics, SpriteBatch, gameTime, deltaTimeScaled: (float)gameTime.ElapsedGameTime.TotalSeconds * TimeScale);
        }
    }
}
