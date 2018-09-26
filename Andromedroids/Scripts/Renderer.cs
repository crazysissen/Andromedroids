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
        public static Vector2 CameraPosition { get; private set; }
        public static float CameraZoom { get; private set; }

        static List<Renderer> renderers = new List<Renderer>();

        public static void Render(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, GameTime gameTime)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.Draw(spriteBatch, (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        public static void AddRenderer(Renderer renderer) => renderers.Add(renderer);

        public static void RemoveRenderer(Renderer renderer) => renderers.Remove(renderer);
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

        }

        public class Sprite : Renderer
        {

        }

        public class 
    }

    class 
}
