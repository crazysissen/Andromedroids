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
        public string PlayerName { get; private set; } = "Player Name";
        public Color PlayerColor { get; private set; } = Color.Gray;

        private int hashKey;

        public ShipPlayer(int hashKey)
        {
            this.hashKey = hashKey;
        }

        public void Setup(int hash)
        {
            if (hash == hashKey)
            {


                return;
            }

            MainController.ReportCheat(PlayerName + " tried to re-configure the base class ShipPlayer");
        }

        public abstract StartupConfig GetConfig();
    }
}
