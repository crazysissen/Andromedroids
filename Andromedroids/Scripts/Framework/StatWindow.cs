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
        public GUI.Collection Collection { get; set; }

        public Rectangle Rectangle { get; set; }

        // Unused, Thruster, Maneuvering, Shield, Weapon (1-6), 
        readonly Color[] powerColors = 
        {
            new Color()
        };

        PlayerManager _player;

        HashKey _key;

        float _textSize, _minorTextSize;

        Texture2D _panelSprite;
        SpriteFont _boldFont, _thinFont;

        GUI.MaskedCollection _miniatureMask;

        Renderer.SpriteScreen _panel, _miniature;
        Renderer.Text _name, _health, _shield, _sf, _sfPercent, _aTime, _tRotation;

        Renderer.Text[] Texts => new Renderer.Text[] { _sf, _sfPercent, _aTime, _tRotation };

        public StatWindow(HashKey key, PlayerManager player, Color color, XNAController controller, Rectangle rectangle)
        {
            if (key.Validate("StatWindow Constructor"))
            {
                Collection = new GUI.Collection();

                _textSize = rectangle.Height / 18.0f;
                _minorTextSize = _textSize * 0.6f;

                _key = key;
                _player = player;
                Rectangle = rectangle;

                _boldFont = ContentController.Get<SpriteFont>("Bold");
                _thinFont = ContentController.Get<SpriteFont>("Thin");

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

                _panelSprite = new Texture2D(controller.GraphicsDevice, defaultPanel.Width, defaultPanel.Height);
                _panelSprite.SetData(colors);

                _panel = new Renderer.SpriteScreen(new Layer(MainLayer.GUI, 1), _panelSprite, rectangle);

                _miniatureMask = new GUI.MaskedCollection()
                {
                    Mask = new Mask(ContentController.Get<Texture2D>("Space1"), new Rectangle((int)(rectangle.X + (rectangle.Width * 5.0f) / 48.0f), (int)(rectangle.Y + (rectangle.Height * 14.0f) / 86.0f), (int)(rectangle.Width * (38.0f / 48.0f)), (int)(rectangle.Height * (26.0f / 86.0f))), Color.Black, false)
                };

                int miniatureWidth = (int)(rectangle.Width * 0.35f), miniatureHeight = (miniatureWidth * 48) / 36;

                _miniature = new Renderer.SpriteScreen(new Layer(MainLayer.GUI, 3), player.PlayerTexture, new Rectangle(rectangle.X + (rectangle.Width/* - miniatureWidth*/) / 2, rectangle.Y + (int)((27.0f / 86.0f) * rectangle.Height) /*- miniatureHeight / 2*/ , miniatureWidth, miniatureHeight), Color.White, 0, 0.5f * new Vector2(32, 48), SpriteEffects.None);

                _miniatureMask.Add(_miniature);

                _name = new Renderer.Text(new Layer(MainLayer.GUI, 2), _boldFont, player.PlayerName, _textSize, 0, Rectangle.Location.ToVector2() + new Vector2(Rectangle.Width * 0.09f, Rectangle.Height * 0.06f));

                _health =       new Renderer.Text(new Layer(MainLayer.GUI, 2), _boldFont, "", _textSize, 0, TextPosition(0));
                _shield =       new Renderer.Text(new Layer(MainLayer.GUI, 2), _boldFont, "", _textSize, 0, TextPosition(1));
                _sf =           new Renderer.Text(new Layer(MainLayer.GUI, 2), _boldFont, "", _minorTextSize, 0, TextPosition(3));
                _sfPercent =    new Renderer.Text(new Layer(MainLayer.GUI, 2), _boldFont, "", _minorTextSize, 0, TextPosition(4));
                _aTime =        new Renderer.Text(new Layer(MainLayer.GUI, 2), _boldFont, "", _minorTextSize, 0, TextPosition(5));
                _tRotation =    new Renderer.Text(new Layer(MainLayer.GUI, 2), _boldFont, "", _minorTextSize, 0, TextPosition(6));

                Collection.Add(_panel, _miniatureMask, _name, _health, _shield, _sf, _sfPercent, _aTime, _tRotation);
            }
        }

        public void Update()
        {
            PlayerManager.TimeStats timeStats = _player.LatestTimeStats;

            _miniature.Rotation = _player.Player.Rotation;

            _health.String = new StringBuilder("Health: " + _player.Health + " / " + PlayerManager.STARTHEALTH);
            _shield.String = new StringBuilder("Shield: " + _player.Shield + " / " + PlayerManager.MAXSHIELD);
            _sf.String = new StringBuilder("Skipped Frames:   " + timeStats.skippedFrames);
            _sfPercent.String = new StringBuilder("Skipped Frames %: " + (int)((float)timeStats.skippedFrames / timeStats.frameCount * 1000) * 0.001f);
            _aTime.String = new StringBuilder("Average Time: " + (int)((float)timeStats.time / timeStats.frameCount * 1000) * 0.001f);
            _tRotation.String = new StringBuilder("Target Rotation: " + (int)(_player.LatestConfig.targetRotation * MathA.RADTODEG * 100) * 0.01f + " deg");
        }

        private Vector2 TextPosition(int order)
        {
            Vector2 origin = Rectangle.Location.ToVector2() + new Vector2(Rectangle.Width * 0.09f, Rectangle.Height * 0.5f);

            return origin + new Vector2(0, _textSize * 1.1f) * order;
        }

        private void SetPositions()
        {
            _name.Position = Rectangle.Location.ToVector2() + new Vector2(Rectangle.Width * 0.09f, Rectangle.Height * 0.06f);


        }
    }
}
