using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Andromedroids
{
    static class RendererController
    {
        static Camera camera;

        static List<Renderer> renderers = new List<Renderer>();

        public static void Initialize(GraphicsDeviceManager graphics, Vector2 cameraPosition, float cameraScale)
        {
            camera = new Camera(graphics)
            {
                Position = cameraPosition,
                Scale = cameraScale
            };
        }

        public static void Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, GameTime gameTime)
        {
            System.Diagnostics.Debug.WriteLine(camera.ScreenToWorldPosition(Mouse.GetState().Position.ToVector2()));

            //camera.Scale -= (0.2f * (float)gameTime.ElapsedGameTime.TotalSeconds);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            foreach (Renderer renderer in renderers)
            {
                renderer.Draw(spriteBatch, camera, (float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            spriteBatch.End();
        }

        public static void AddRenderer(Renderer renderer)
            => renderers.Add(renderer);

        public static void RemoveRenderer(Renderer renderer)
            => renderers.Remove(renderer);

        public static Camera GetCamera(HashKey key)
        {
            if (key.Validate("GetCamera"))
            {
                return camera;
            }

            return null;
        }
    }

    abstract class Renderer
    {
        const float
            DEGTORAD = (2 * (float)Math.PI) / 360,
            FONTSIZEMULTIPLIER = 1 / 12;

        /// <summary>Whether or not the object should be drawn automatically</summary>
        public virtual bool Automatic { get; set; } = true;

        public abstract void Draw(SpriteBatch spriteBatch, Camera camera, float deltaTime);

        /// <summary>A Layer class to represent what depth it should be drawn at</summary>
        public abstract Layer Layer { get; set; }

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
            /// <summary>The texture of the object</summary>
            public virtual Texture2D Texture { get; set; }

            /// <summary>The x & y coordinates of the object in world space</summary>
            public virtual Vector2 Position { get; set; }

            /// <summary>The width/height of the object</summary>
            public virtual Vector2 Size { get; set; }

            /// <summary>The rotation angle of the object measured in degrees (0-360)</summary>
            public virtual float Rotation { get; set; }

            /// <summary>A vector between (0,0) and (1,1) to represent the pivot around which the object rotates 
            /// and what point will line up to the Vector2 position</summary>
            public virtual Vector2 Origin { get; set; }

            /// <summary>The color multiplier of the object</summary>
            public virtual Color Color { get; set; }

            /// <summary>Wether or not the sprite is flipped somehow, stack using binary OR operator (|)</summary>
            public virtual SpriteEffects Effects { get; set; }

            public override Layer Layer { get; set; }

            public Sprite(Texture2D texture, Vector2 position, Vector2 size, Color color, float rotation, Vector2 rotationOrigin, SpriteEffects effects)
            {
                Texture = texture;
                Position = position;
                Size = size;
                Rotation = rotation;
                Origin = rotationOrigin;
                Color = color;
                Effects = effects;
            }

            public override void Draw(SpriteBatch spriteBatch, Camera camera, float deltaTime)
            {
                spriteBatch.Draw(Texture, camera.WorldToScreenPosition(Position), null, Color, Rotation, Origin, camera.WorldToScreenSize(Size), Effects, Layer.LayerDepth);
            }
        }

        public class SpriteScreen : Renderer
        {
            /// <summary>The texture of the object</summary>
            public virtual Texture2D Texture { get; set; }

            /// <summary>The x & y coordinates of the object in world spacesummary>
            public virtual Rectangle Transform { get; set; }

            /// <summary>The rotation angle of the object measured in degrees (0-360)</summary>
            public virtual float Rotation { get; set; }

            /// <summary>A vector between (0,0) and (1,1) to represent the pivot around which the object rotates 
            /// and what point will line up to the Vector2 position</summary>
            public virtual Vector2 Origin { get; set; }

            /// <summary>The color multiplier of the object</summary>
            public virtual Color Color { get; set; }

            /// <summary> Wether or not the sprite is flipped somehow, stack using binary OR operator (|)</summary>
            public virtual SpriteEffects Effects { get; set; }

            public override Layer Layer { get; set; }

            public SpriteScreen(Texture2D texture, Rectangle transform) : this(texture, transform, Color.White, 0, Vector2.Zero, SpriteEffects.None) { }

            public SpriteScreen(Texture2D texture, Rectangle transform, Color color) : this(texture, transform, color, 0, Vector2.Zero, SpriteEffects.None) { }

            public SpriteScreen(Texture2D texture, Rectangle transform, Color color, float rotation, Vector2 rotationOrigin, SpriteEffects effects)
            {
                Texture = texture;
                Transform = transform;
                Rotation = rotation;
                Origin = rotationOrigin;
                Color = color;
                Effects = effects;
            }

            public override void Draw(SpriteBatch spriteBatch, Camera camera, float deltaTime)
            {
                spriteBatch.Draw(Texture, Transform, null, Color, Rotation * DEGTORAD, Origin, Effects, Layer.LayerDepth);
            }
        }

        public class Animator : Renderer
        {
            public override Layer Layer { get; set; }

            public override void Draw(SpriteBatch spriteBatch, Camera camera, float deltaTime)
            {

            }
        }

        public class AnimatorScreen : Renderer
        {
            /// <summary>The texture of the object</summary>
            public virtual Texture2D Texture { get; set; }

            /// <summary>The x & y coordinates of the object in world space</summary>
            public virtual Rectangle Transform { get; set; }

            /// <summary>The rotation angle of the object measured in degrees (0-360)</summary>
            public virtual float Rotation { get; set; }

            /// <summary>A vector between (0,0) and (1,1) to represent the pivot around which the object rotates 
            /// and what point will line up to the Vector2 position</summary>
            public virtual Vector2 Origin { get; set; }

            /// <summary>The color multiplier of the object</summary>
            public virtual Color Color { get; set; }

            /// <summary>Wether or not the sprite is flipped somehow, stack using binary OR operator (|)</summary>
            public virtual SpriteEffects Effects { get; set; }

            public override Layer Layer { get; set; }

            public Point FrameDimensions { get; private set; }
            public Point CountDimensions { get; private set; }
            public float Time { get; set; }
            public float TimeInterval { get; set; }
            public bool Repeat { get; set; }
            public int CurrentFrame => (int)(Time / TimeInterval);
            public int FrameCount { get; private set; }

            public AnimatorScreen(Texture2D sheet, Point frameDimensions, Rectangle transform, Vector2 origin, float rotation, Color color, float interval, float startTime, bool repeat, SpriteEffects spriteEffects)
            {
                if (sheet.Width % frameDimensions.X != 0 || sheet.Height % frameDimensions.Y != 0)
                {
                    throw new Exception("Tried to create AnimatorScreen where the format image was not proportional to the frame size.");
                }

                FrameDimensions = frameDimensions;
                Time = startTime;
                TimeInterval = interval;
                Repeat = repeat;
                CountDimensions = new Point(sheet.Width / frameDimensions.X, sheet.Height / frameDimensions.Y);
                FrameCount = CountDimensions.X * CountDimensions.Y;

                Texture = sheet;
                Transform = transform;
                Rotation = rotation;
                Origin = origin;
                Color = color;
                Effects = spriteEffects;
            }

            public override void Draw(SpriteBatch spriteBatch, Camera camera, float deltaTime)
            {
                Time += deltaTime;
                if (Time > FrameCount * TimeInterval)
                {
                    Time %= (FrameCount * TimeInterval);
                }

                int currentFrame = CurrentFrame;
                Rectangle DestinationRectangle = new Rectangle()
                {
                    X = FrameDimensions.X * (currentFrame % CountDimensions.X),
                    Y = FrameDimensions.Y * (int)((float)currentFrame / CountDimensions.X),
                    Width = FrameDimensions.X,
                    Height = FrameDimensions.Y
                };

                spriteBatch.Draw(Texture, Transform, DestinationRectangle, Color, Rotation, Origin, Effects, Layer.LayerDepth);
            }
        }

        public class Text : Renderer
        {
            public SpriteFont Font { get; set; }
            /// <summary>A StringBuilder class to represent the text shown</summary>
            public StringBuilder String { get; set; }
            public Vector2 Position { get; set; }
            public Vector2 Scale { get; set; }
            /// <summary>A vector between (0,0) and (1,1) to represent the pivot around which the object rotates 
            /// and what point will line up to the Vector2 position</summary>
            public Vector2 Origin { get; set; }
            public Color Color { get; set; }
            public float Rotation { get; set; }
            public SpriteEffects SpriteEffects { get; set; }

            public override Layer Layer { get; set; }

            public Text(SpriteFont font, string text, float fontSize, float rotation, Vector2 position)
                : this(font, new StringBuilder(text), Vector2.One * fontSize * FONTSIZEMULTIPLIER, rotation, position, new Vector2(0.5f, 0.5f), Color.White, SpriteEffects.None)
            { }

            public Text(SpriteFont font, StringBuilder text, float fontSize, float rotation, Vector2 position)
                : this(font, text, Vector2.One * fontSize * FONTSIZEMULTIPLIER, rotation, position, new Vector2(0.5f, 0.5f), Color.White, SpriteEffects.None)
            { }

            public Text(SpriteFont font, string text, float fontSize, float rotation, Vector2 position, Vector2 origin)
                : this(font, new StringBuilder(text), Vector2.One * fontSize * FONTSIZEMULTIPLIER, rotation, position, origin, Color.White, SpriteEffects.None)
            { }

            public Text(SpriteFont font, StringBuilder text, float fontSize, float rotation, Vector2 position, Vector2 origin)
                : this(font, text, Vector2.One * fontSize * FONTSIZEMULTIPLIER, rotation, position, origin, Color.White, SpriteEffects.None)
            { }

            public Text(SpriteFont font, string text, float fontSize, float rotation, Vector2 position, Color color)
                : this(font, new StringBuilder(text), Vector2.One * fontSize * FONTSIZEMULTIPLIER, rotation, position, new Vector2(0.5f, 0.5f), color, SpriteEffects.None)
            { }

            public Text(SpriteFont font, StringBuilder text, float fontSize, float rotation, Vector2 position, Color color)
                : this(font, text, Vector2.One * fontSize * FONTSIZEMULTIPLIER, rotation, position, new Vector2(0.5f, 0.5f), color, SpriteEffects.None)
            { }

            public Text(SpriteFont font, string text, float fontSize, float rotation, Vector2 position, Vector2 origin, Color color)
                : this(font, new StringBuilder(text), Vector2.One * fontSize * FONTSIZEMULTIPLIER, rotation, position, origin, color, SpriteEffects.None)
            { }

            public Text(SpriteFont font, StringBuilder text, float fontSize, float rotation, Vector2 position, Vector2 origin, Color color)
                : this(font, text, Vector2.One * fontSize * FONTSIZEMULTIPLIER, rotation, position, origin, color, SpriteEffects.None)
            { }

            public Text(SpriteFont font, StringBuilder text, Vector2 scale, float rotation, Vector2 position, Vector2 origin, Color color, SpriteEffects spriteEffects)
            {
                Font = font;
                String = text;
                Scale = scale;
                Rotation = rotation;
                Position = position;
                Origin = origin;
                Color = color;
                SpriteEffects = spriteEffects;
            }

            public override void Draw(SpriteBatch spriteBatch, Camera camera, float deltaTime)
            {
                spriteBatch.DrawString(Font, String, Position, Color, Rotation * DEGTORAD, Origin, Scale, SpriteEffects, Layer.LayerDepth);
            }
        }

        public class Custom : Renderer
        {
            public DrawCommand Command { get; private set; }

            public override Layer Layer { get; set; }

            public Custom(DrawCommand drawCommand, Layer layer)
            {
                Command = drawCommand;
                Layer = layer;
            }

            public override void Draw(SpriteBatch spriteBatch, Camera camera, float deltaTime)
            {
                Command.Invoke(spriteBatch, camera, deltaTime, Layer.LayerDepth);
            }

            public void SetCommand(DrawCommand drawCommand) => Command = drawCommand;
        }

        public delegate void DrawCommand(SpriteBatch spriteBatch, Camera camera, float deltaTime, float managedLayer);
    }

    public class Camera
    {
        // A square based on the average distances to the screen edges, divided into pieces
        const float
            UNIVERSALMODIFIER = 0.1f;

        public const int
            WORLDUNITPIXELS = 1500; 

        public Vector2 Position { get; set; }
        public float Scale { get; set; }

        public Vector2 CenterCoordinate { get; private set; }

        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }

        private float _standardWUScaling, _standardSquareDiameter;

        public float WorldUnitDiameter => _standardWUScaling * _standardSquareDiameter;

        public Camera(GraphicsDeviceManager graphics)
        {
            ScreenWidth = graphics.PreferredBackBufferWidth;
            ScreenHeight = graphics.PreferredBackBufferHeight;

            _standardSquareDiameter = 0.5f * (ScreenWidth + ScreenHeight);
            
            _standardWUScaling = _standardSquareDiameter / WORLDUNITPIXELS;

            CenterCoordinate = new Vector2(ScreenWidth * 0.5f, ScreenHeight * 0.5f);
        }

        public Vector2 WorldToScreenPosition(Vector2 worldPosition)
            => CenterCoordinate + (worldPosition - Position) * _standardSquareDiameter * _standardWUScaling * Scale * UNIVERSALMODIFIER;

        public Vector2 ScreenToWorldPosition(Vector2 screenPosition)
            => (screenPosition - CenterCoordinate) / (_standardSquareDiameter * _standardWUScaling * Scale * UNIVERSALMODIFIER) + Position;

        public Vector2 WorldToScreenSize(Vector2 size)
            => size * UNIVERSALMODIFIER * Scale;

        public Vector2 ScreenToWorldSize(Vector2 size)
            => size / (UNIVERSALMODIFIER * Scale);
    }

    public struct Layer
    {
        public enum Main
        {
            AbsoluteBottom, Background, Main, Overlay, GUI, AbsoluteTop
        }

        public const int
            MAINCOUNT = 6;

        public const float
            LAYERINTERVAL = float.Epsilon,
            MAININTERVAL = 1 / MAINCOUNT,
            HALFINTERVAL = MAININTERVAL * 0.5f;

        public float LayerDepth => 1 - ((int)main * MAININTERVAL + HALFINTERVAL + LAYERINTERVAL * layer);

        public Main main;
        public int layer;

        public Layer(Main main, int layer)
        {
            this.main = main;
            this.layer = layer;
        }
    }
}
