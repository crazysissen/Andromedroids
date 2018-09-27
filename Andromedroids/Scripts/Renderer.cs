using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Andromedroids
{
    static class RendererController
    {
        public static Camera Camera { get; private set; }

        static List<Renderer> renderers = new List<Renderer>();

        public static void Render(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, GameTime gameTime)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.Draw(spriteBatch, (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        public static void AddRenderer(Renderer renderer) 
            => renderers.Add(renderer);

        public static void RemoveRenderer(Renderer renderer) 
            => renderers.Remove(renderer);
    }

    abstract class Renderer
    {
        public abstract void Draw(SpriteBatch spriteBatch, float deltaTime);

        public Renderer()
        {
            RendererController.AddRenderer(this);
        }

        public void Destroy()
        {
            RendererController.RemoveRenderer(this);
        }

        public class Sprite : Renderer
        {
            public override void Draw(SpriteBatch spriteBatch, float deltaTime)
            {
                
            }
        }

        public class SpriteRectangle : Renderer
        {
            /// <summary>
            /// The texture of the object
            /// </summary>
            public Texture2D Texture { get; private set; }
            /// <summary>
            /// The dimensions and location of the object in screen space (pixel coordinates)
            /// </summary>
            public Rectangle Rectangle { get; private set; }
            /// <summary>
            /// The color multiplier of the object
            /// </summary>
            public Color Color { get; private set; }

            public SpriteRectangle(Texture2D texture, Rectangle rectangle)
            {
                Texture = texture;
                Rectangle = rectangle;
                Color = Color.White;
            }

            public SpriteRectangle(Texture2D texture, Rectangle rectangle, Color color) : this(texture, rectangle)
            {
                Color = color;
            }

            public override void Draw(SpriteBatch spriteBatch, float deltaTime)
            {
                spriteBatch.Draw(Texture, Rectangle, Color);
            }
        }

        public class Text : Renderer
        {
            public override void Draw(SpriteBatch spriteBatch, float deltaTime)
            {
                
            }
        }

        public class Custom : Renderer
        {
            public DrawCommand Command { get; private set; }

            public Custom(DrawCommand drawCommand)
            {
                Command = drawCommand;
            }

            public override void Draw(SpriteBatch spriteBatch, float deltaTime)
            {
                Command.Invoke(spriteBatch, deltaTime);
            }

            public void SetCommand(DrawCommand drawCommand) => Command = drawCommand;
        }

        public class Animator : Renderer
        {
            public override void Draw(SpriteBatch spriteBatch, float deltaTime)
            {

            }
        }

        public delegate void DrawCommand(SpriteBatch spriteBatch, float deltaTime);
    }

    public class Camera
    {
        const float WIDTHDIVISIONS = 16;

        public Vector2 ScreenWorldDimensions 
            => new Vector2(WIDTHDIVISIONS / Scale, (((float)XNAController.Graphics.PreferredBackBufferHeight / XNAController.Graphics.PreferredBackBufferWidth) * WIDTHDIVISIONS) / Scale);

        public float WorldUnitDiameter
            => (XNAController.Graphics.PreferredBackBufferWidth * Scale) / WIDTHDIVISIONS;

        public Vector2 Position { get; private set; }
        public float Scale { get; private set; }
    }
}
