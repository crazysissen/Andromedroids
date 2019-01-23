using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Andromedroids
{
    class Tournament
    {
        const int
            SCREENMARGIN = 30,
            MAXIMUMVERTICALSPACING = 60,
            MAXIMUMHORIZONTALSPACING = 100,
            PLAYERHEIGHT = 32,
            PLAYERWIDTH = 200,
            LINKWIDTH = 6;

        readonly Color
            wonColor = new Color(255, 255, 255, 180),
            inactiveColor = new Color(255, 255, 255, 180),
            deadColor = new Color(255, 255, 255, 40),
            blinkColor = new Color(180, 180, 150, 255),
            textColor = new Color(255, 150, 0, 255),
            neutralLinkColor = new Color(180, 180, 180, 255),
            wonLinkColor = new Color(255, 150, 0, 255),
            deadLinkColor = new Color(150, 150, 150, 180);

        public GUI.Collection Collection { get; private set; }
        public PlayerManager[] Players { get; private set; }
        public TournamentBracket Bracket { get; private set; }

        private Texture2D _square = ContentController.Get<Texture2D>("Square");
        private Renderer.SpriteScreen[] _blinkSprites;
        private Renderer.SpriteScreen[][] _sortedRenderers;
        private List<Renderer.SpriteScreen> playerLabels, links;
        private List<Renderer.Text> playerTexts;
        private Action<PlayerManager, PlayerManager> _startMatchAction;
        private Action<PlayerManager> _endTournamentAction;
        private float _countdown;
        private bool _playedSound;

        public Tournament(PlayerManager[] players, Action<PlayerManager, PlayerManager> startMatchAction, Action<PlayerManager> endTournamentAction)
        {
            _countdown = 3.0f;
            _startMatchAction = startMatchAction;
            _endTournamentAction = endTournamentAction;

            _blinkSprites = new Renderer.SpriteScreen[2];

            Players = new PlayerManager[players.Length];
            Random r = new Random();
            List<PlayerManager> dList = new List<PlayerManager>(players);

            for (int i = 0; i < players.Length; ++i)
            {
                int slot = r.Next(dList.Count);

                Players[i] = dList[slot];
                dList.RemoveAt(slot);
            }

            // Generating the actual tournament bracket is completely automatic
            Bracket = new TournamentBracket(Players);

            GenerateRenderers();

            _blinkSprites = new Renderer.SpriteScreen[]
            {
                _sortedRenderers[Bracket.CurrentMatch.row][Bracket.CurrentMatch.slot],
                _sortedRenderers[Bracket.CurrentMatch.row][Bracket.CurrentMatch.slot + 1]
            };
        }

        public void MatchOver(int winner)
        {
            _countdown = 3.0f;
            Bracket.GetNextMatch(winner);
            _playedSound = false;

            GenerateRenderers();

            _blinkSprites = new Renderer.SpriteScreen[]
            {
                _sortedRenderers[Bracket.CurrentMatch.row][Bracket.CurrentMatch.slot],
                _sortedRenderers[Bracket.CurrentMatch.row][Bracket.CurrentMatch.slot + 1]
            };
        }

        // Expected to simply stop updating while a match is started
        public void Update(MainController controller, float deltaTime)
        {
            _countdown -= deltaTime;

            if (_countdown <= 0)
            {
                controller.StartTournamentMatch(Bracket.Bracket[Bracket.CurrentMatch.row][Bracket.CurrentMatch.slot].player, Bracket.Bracket[Bracket.CurrentMatch.row][Bracket.CurrentMatch.slot + 1].player);

                _blinkSprites[0].Color = Color.White;
                _blinkSprites[1].Color = Color.White;

                return;
            }

            if (_countdown <= 1)
            {
                if (!_playedSound)
                {
                    Sound.PlayEffect(SFX.Success);
                }

                bool blink = (int)(_countdown * 20) % 2 == 0;

                _blinkSprites[0].Color = blink ? blinkColor : Color.White;
                _blinkSprites[1].Color = blink ? blinkColor : Color.White;
            }
        }

        private void GenerateRenderers()
        {
            if (playerLabels != null)
            {
                foreach (Renderer.SpriteScreen label in playerLabels)
                {
                    label.Destroy();
                }

                foreach (Renderer.Text text in playerTexts)
                {
                    text.Destroy();
                }

                foreach (Renderer.SpriteScreen link in links)
                {
                    link.Destroy();
                }
            }

            if (Collection == null)
            {
                Collection = new GUI.Collection();
                RendererController.GUI.Add(Collection);
            }

            _sortedRenderers = new Renderer.SpriteScreen[Bracket.Bracket.Length][];

            Point res = XNAController.DisplayResolution;

            int 
                layers = Bracket.Bracket.Length, 
                maxHeightLayers = Bracket.Bracket[layers - 1].Length,

                horizontalSpace = MAXIMUMHORIZONTALSPACING,
                verticalSpace = MAXIMUMVERTICALSPACING,

                totalWidth = MAXIMUMHORIZONTALSPACING * (layers - 1) + PLAYERWIDTH * layers,
                totalHeight = MAXIMUMVERTICALSPACING * (maxHeightLayers - 1) + PLAYERHEIGHT * maxHeightLayers;

            if (totalWidth > res.X - SCREENMARGIN * 2)
            {
                horizontalSpace = (horizontalSpace * (layers - 1) - (totalWidth - (res.X - SCREENMARGIN * 2))) / (layers - 1);
                totalWidth = res.X - SCREENMARGIN * 2;
            }

            if (totalHeight > res.Y - SCREENMARGIN * 2)
            {
                verticalSpace = (verticalSpace * (maxHeightLayers - 1) - (totalHeight - (res.Y - SCREENMARGIN * 2))) / (maxHeightLayers - 1);
                totalHeight = res.Y - SCREENMARGIN * 2;
            }

            int
                currentX = (res.X - totalWidth) / 2,
                startY = (res.Y - totalHeight) / 2;

            Point[,] positions = new Point[layers, maxHeightLayers];

            for (int i = layers - 1; i >= 0; --i)
            {
                Point previousPosition = new Point();

                _sortedRenderers[i] = new Renderer.SpriteScreen[Bracket.Bracket[i].Length];

                for (int j = 0; j < Bracket.Bracket[i].Length; ++j)
                {
                    TournamentBracket.Slot slot = Bracket.Bracket[i][j];

                    Point position = new Point(currentX, i == layers - 1 ? (startY + (PLAYERHEIGHT + verticalSpace) * j) : ((positions[i + 1, j * 2].Y + positions[i + 1, j * 2 + 1].Y) / 2));

                    positions[i, j] = position;

                    if (slot != null)
                    {
                        Color[] colors = { inactiveColor, Color.White, deadColor, wonColor };

                        Renderer.SpriteScreen label = new Renderer.SpriteScreen(Layer.Default, PlayerList.buttonBlank, new Rectangle(position, new Point(PLAYERWIDTH, PLAYERHEIGHT)), slot == null ? inactiveColor : colors[(int)slot.state]);

                        _sortedRenderers[i][j] = label;

                        if (slot.player != null)
                        {
                            Renderer.Text playerText = new Renderer.Text(new Layer(MainLayer.Main, 1), PlayerList.nameFont, slot.player.MenuName, 20, 0, (position + new Point(12, 4)).ToVector2(), textColor);

                            Collection.Add(playerText);
                        }

                        Collection.Add(label);

                        if (j % 2 == 1)
                        {
                            Color[] linkColors = { neutralLinkColor, neutralLinkColor, deadLinkColor, wonLinkColor };

                            Renderer.SpriteScreen tLink, tBridge, bLink, bBridge, connector;
                            TournamentBracket.Slot firstSlot = Bracket.Bracket[i][j - 1];

                            Point averageMiddle = new Point(position.X + PLAYERWIDTH, (position.Y + previousPosition.Y + PLAYERHEIGHT) / 2);

                            int slotVerticalSpace = position.Y - previousPosition.Y - PLAYERHEIGHT;

                            tLink = new Renderer.SpriteScreen(Layer.Default, _square, new Rectangle(previousPosition.X + PLAYERWIDTH, previousPosition.Y + (PLAYERHEIGHT - LINKWIDTH) / 2, (horizontalSpace + LINKWIDTH) / 2, LINKWIDTH), linkColors[(int)firstSlot.state]);
                            bLink = new Renderer.SpriteScreen(Layer.Default, _square, new Rectangle(position.X + PLAYERWIDTH, position.Y + (PLAYERHEIGHT - LINKWIDTH) / 2, (horizontalSpace + LINKWIDTH) / 2, LINKWIDTH), linkColors[(int)slot.state]);

                            bBridge = new Renderer.SpriteScreen(Layer.Default, _square,
                                new Rectangle(averageMiddle.X + (horizontalSpace - LINKWIDTH) / 2, averageMiddle.Y - LINKWIDTH / 2, LINKWIDTH, (slotVerticalSpace + PLAYERHEIGHT) / 2 + LINKWIDTH),
                                linkColors[(int)slot.state]);

                            tBridge = new Renderer.SpriteScreen(Layer.Default, _square,
                                new Rectangle(averageMiddle.X + (horizontalSpace - LINKWIDTH) / 2, previousPosition.Y + (PLAYERHEIGHT - LINKWIDTH) / 2, LINKWIDTH, (slotVerticalSpace + PLAYERHEIGHT) / 2 + LINKWIDTH),
                                linkColors[(int)firstSlot.state]);

                            connector = new Renderer.SpriteScreen(new Layer(MainLayer.Main, 1), _square, 
                                new Rectangle(averageMiddle.X + (horizontalSpace - LINKWIDTH) / 2, averageMiddle.Y - LINKWIDTH / 2, (horizontalSpace + LINKWIDTH) / 2, LINKWIDTH), 
                                slot.state == TournamentBracket.Slot.State.Empty || slot.state == TournamentBracket.Slot.State.Waiting ? linkColors[0] : wonLinkColor);

                            Collection.Add(tLink, bLink, tBridge, bBridge, connector);
                        }
                    }

                    previousPosition = position;
                }

                currentX += PLAYERWIDTH + horizontalSpace;
            }
        }
    }
}
