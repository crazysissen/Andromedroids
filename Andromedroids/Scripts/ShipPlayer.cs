using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Andromedroids
{
    abstract class ShipPlayer
    {
        string playerName = "NewPlayer";
        public string PlayerName
        {
            get
            {
                return playerName;
            }
            set
            {
                if (playerName == "NewPlayer")
                {
                    playerName = value;
                }
            }
        }

        Color playerColor = Color.Gray;
        public Color PlayerColor
        {
            get
            {
                return playerColor;
            }
            set
            {
                if (PlayerColor == Color.Gray)
                {
                    playerColor = value;
                }
            }
        }


    }
}
