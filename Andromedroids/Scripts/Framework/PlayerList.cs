using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Andromedroids
{
    class PlayerList
    {
        static Texture2D 
            checkBoxTrue,
            checkBoxFalse,
            buttonBlank;

        static SpriteFont
            nameFont = ContentController.Get<SpriteFont>("Bold");

        public GUI.Collection Collection { get; private set; }

        public GUI.Button[] Buttons { get; private set; }
        public Renderer.SpriteScreen[] Labels { get; private set; }
        public Renderer.Text[] Names { get; private set; }

        public bool[] Values { get; private set; }

        public int SelectedCount => Values.Count(o => o == true);

        MethodInstance[] instances;
        List<int> _selectionOrder;
        Point _origin;
        int _maxChoices;

        public PlayerList()
        {
            Collection = new GUI.Collection();
        }

        public void Initialize(PlayerManager[] players, Point origin, int maxChoices)
        {
            if (checkBoxTrue == null)
            {
                checkBoxTrue = ContentController.Get<Texture2D>("CheckBoxTrue");
                checkBoxFalse = ContentController.Get<Texture2D>("CheckBoxFalse");
                buttonBlank = ContentController.Get<Texture2D>("ButtonBlank");
            }

            Collection.Members.Clear();

            _origin = origin;
            _maxChoices = maxChoices;
            _selectionOrder = new List<int>();

            Values = new bool[players.Length];
            Buttons = new GUI.Button[players.Length];
            Labels = new Renderer.SpriteScreen[players.Length];
            Names = new Renderer.Text[players.Length];
            instances = new MethodInstance[players.Length];

            for (int i = 0; i < players.Length; ++i)
            {
                instances[i] = new MethodInstance(ButtonClick, i);

                Point slotOrigin = origin + new Point(0, 64 * i);

                Buttons[i] = new GUI.Button(new Rectangle(slotOrigin, new Point(48, 48)), checkBoxFalse);
                Buttons[i].AddEffect(Sound.Effect(SFX.MenuBlipNeutral));
                Buttons[i].Layer = new Layer(MainLayer.GUI, 0);
                Buttons[i].OnClick += instances[i].Activate;

                Labels[i] = new Renderer.SpriteScreen(new Layer(MainLayer.GUI, 0), buttonBlank, new Rectangle(slotOrigin.X + 54, slotOrigin.Y, 300, 48));

                Names[i] = new Renderer.Text(new Layer(MainLayer.GUI, 1), nameFont, players[i].MenuName, 20, 0, new Vector2(slotOrigin.X + 66, slotOrigin.Y + 12), new Color(255, 150, 0, 255));

                Collection.Add(Buttons[i], Labels[i], Names[i]);
            }
        }

        void ButtonClick(int index)
        {
            if (!Values[index])
            {
                ButtonActivate(index);

                if (_maxChoices > 0)
                {
                    _selectionOrder.Add(index);

                    if (_selectionOrder.Count > _maxChoices)
                    {
                        ButtonDeactivate(_selectionOrder[0]);
                        _selectionOrder.RemoveAt(0);
                    }
                }

                return;
            }

            ButtonDeactivate(index);

            _selectionOrder.Remove(index);
        }

        void ButtonDeactivate(int index)
        {
            Buttons[index].Texture = checkBoxFalse;

            Values[index] = false;
        }

        void ButtonActivate(int index)
        {
            Buttons[index].Texture = checkBoxTrue; 

            Values[index] = true;
        }

        private struct MethodInstance
        {
            Action<int> _output;
            int _index;

            public MethodInstance(Action<int> output, int number)
            {
                _output = output;
                _index = number;
            }

            public void Activate()
            {
                _output.Invoke(_index);
            }
        }
    }
}
