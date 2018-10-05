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

        public static void Initialize(Vector2 cameraPosition, float cameraScale)
        {
            Camera = new Camera()
            {
                Position = cameraPosition,
                Scale = cameraScale
            };
        }

        public static void Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            foreach (Renderer renderer in renderers)
            {
                renderer.Draw(spriteBatch, Camera, (float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            spriteBatch.End();
        }

        public static void AddRenderer(Renderer renderer)
            => renderers.Add(renderer);

        public static void RemoveRenderer(Renderer renderer)
            => renderers.Remove(renderer);
    }

    abstract class Renderer
    {
        public abstract void Draw(SpriteBatch spriteBatch, Camera camera, float deltaTime);

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
            /// <summary>
            /// The texture of the object
            /// </summary>
            public virtual Texture2D Texture { get; set; }

            /// <summary>
            /// The x & y coordinates of the object in world space
            /// </summary>
            public virtual Vector2 Position { get; set; }

            /// <summary>
            /// The width/height of the object
            /// </summary>
            public virtual Vector2 Size { get; set; }

            /// <summary>
            /// The rotation angle of the object measured in degrees (0-360)
            /// </summary>
            public virtual float Rotation { get; set; }

            /// <summary>
            /// The point on the object around which it rotates
            /// </summary>
            public virtual Vector2 Origin { get; set; }

            /// <summary>
            /// The color multiplier of the object
            /// </summary>
            public virtual Color Color { get; set; }

            /// <summary>
            /// Wether or not the sprite is flipped somehow, stack using binary OR operator (|)
            /// </summary>
            public virtual SpriteEffects Effects { get; set; }

            public Sprite(Texture2D texture, Vector2 position, Vector2 size, Color color, float rotation, Vector2 origin, SpriteEffects effects)
            {
                Texture = texture;
                Position = position;
                Size = size;
                Rotation = rotation;
                Color = color;
                Origin = origin;
                Effects = effects;
            }

            public Sprite(Texture2D texture, Vector2 position, Vector2 size, float rotation)
                : this(texture, position, size, Color.White, rotation, new Vector2(texture.Width, texture.Height) * 0.5f, SpriteEffects.None) { }
            

            public override void Draw(SpriteBatch spriteBatch, Camera camera, float deltaTime)
            {
                spriteBatch.Draw(Texture, camera.WorldToScreenPosition(Position), null, Color, Rotation, Origin, camera.WorldToScreenSize(Size), Effects, 0);
            }
        }

        //public class SpriteAuto : Sprite
        //{
        //    Func<Texture2D> getTexture;
        //    Func<Vector2> getPosition, getSize;
        //    Func<Color> getColor;

        //    public override Texture2D Texture => getTexture.Invoke();
        //    public override Vector2 Position => getPosition.Invoke();
        //    public override Vector2 Size => getSize.Invoke();
        //    public override Color Color => getColor.Invoke();
        //    public override SpriteEffects Effects => SpriteEffects.None;

        //    public SpriteAuto()
        //    {

        //    }
        //}

        public class SpriteScreen : Renderer
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

            public SpriteScreen(Texture2D texture, Rectangle rectangle)
            {
                Texture = texture;
                Rectangle = rectangle;
                Color = Color.White;
            }

            public SpriteScreen(Texture2D texture, Rectangle rectangle, Color color) : this(texture, rectangle)
            {
                Color = color;
            }

            public override void Draw(SpriteBatch spriteBatch, Camera camera, float deltaTime)
            {
                spriteBatch.Draw(Texture, Rectangle, Color);
            }
        }

        public class Text : Renderer
        {
            public override void Draw(SpriteBatch spriteBatch, Camera camera, float deltaTime)
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

            public override void Draw(SpriteBatch spriteBatch, Camera camera, float deltaTime)
            {
                Command.Invoke(spriteBatch, camera, deltaTime);
            }

            public void SetCommand(DrawCommand drawCommand) => Command = drawCommand;
        }

        public class Animator : Renderer
        {
            public override void Draw(SpriteBatch spriteBatch, Camera camera, float deltaTime)
            {

            }
        }

        public delegate void DrawCommand(SpriteBatch spriteBatch, Camera camera, float deltaTime);
    }

    public class Camera
    {
        const float 
            WIDTHDIVISIONS = 16.0f,
            DISTANCEMODIFIER = 20.0f;

        public Vector2 ScreenWorldDimensions
            => new Vector2(WIDTHDIVISIONS / Scale, (((float)XNAController.Graphics.PreferredBackBufferHeight / XNAController.Graphics.PreferredBackBufferWidth) * WIDTHDIVISIONS) / Scale);

        public float WorldUnitDiameter
            => (XNAController.Graphics.PreferredBackBufferWidth) / (WIDTHDIVISIONS * Scale);

        public Vector2 Position { get; set; }
        public float Scale { get; set; }

        public Vector2 WorldToScreenPosition(Vector2 worldPosition)
        {
            return (worldPosition - Position) * Scale * DISTANCEMODIFIER + new Vector2(XNAController.Graphics.PreferredBackBufferWidth, XNAController.Graphics.PreferredBackBufferHeight) * 0.5f;
        }

        public Vector2 WorldToScreenSize(Vector2 size)
        {
            return size * Scale;
        }
    }
}
