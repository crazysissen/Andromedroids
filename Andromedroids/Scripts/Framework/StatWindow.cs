using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Andromedroids
{
    class StatWindow
    {
        public Rectangle Rectangle { get; set; }

        // Unused, Thruster, Maneuvering, Shield, Weapon (1-6), 
        readonly Color[] powerColors =
        {
            new Color()
        };

        PlayerManager player;

        float textSize;

        Texture2D panelSprite;
        SpriteFont boldFont, thinFont;

        GUI.Collection guiCollection;
        GUI.MaskedCollection healthBar, shieldBar;

        Renderer.SpriteScreen panel, miniature;
        Renderer.Text name, skippedFrames, skippedFramesPercentage, averageTime, targetRotation;
        Renderer.SpriteScreen[] powerBars, powerups;

        public StatWindow(HashKey key, PlayerManager player, Color color, XNAController controller, Rectangle rectangle)
        {
            if (key.Validate("StatWindow Constructor"))
            {
                guiCollection = new GUI.Collection();

                textSize = rectangle.Height / 18.0f;

                this.player = player;
                Rectangle = rectangle;

                boldFont = ContentController.Get<SpriteFont>("Bold");
                thinFont = ContentController.Get<SpriteFont>("Thin");

                Color modifiedColor = new Color(color * 0.5f, 0.3f);

                Texture2D defaultPanel = ContentController.Get<Texture2D>("StatWindow");
                Color[] colors = new Color[defaultPanel.Width * defaultPanel.Height];
                defaultPanel.GetData(colors);

                for (int i = 0; i < colors.Length; i++)
                {
                    if (colors[i] == Color.Red)
                    {
                        colors[i] = modifiedColor;
                    }
                }

                panelSprite = new Texture2D(controller.GraphicsDevice, defaultPanel.Width, defaultPanel.Height);

                panel = new Renderer.SpriteScreen(new Layer(MainLayer.GUI, 1), panelSprite, rectangle);

                name = new Renderer.Text(new Layer(MainLayer.GUI, 2), boldFont, player.PlayerName, textSize, 0, TextPosition(0));
            }
        }

        public void Update(int skippedFrames, float skippedFramePercentage, float averageTime, float targetRotation, float[] powerDistribution, Powerup[] powerups, float health, float shield)
        {
            
        }

        private Vector2 TextPosition(int order)
        {
            Vector2 origin = Rectangle.Location.ToVector2() + new Vector2(Rectangle.Width * 0.08f, Rectangle.Height * 0.35f);

            return origin + new Vector2(0, -textSize * 1.1f) * order;
        }
    }
}
