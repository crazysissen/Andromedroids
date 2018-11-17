using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Andromedroids
{
    class DirectInput
    {
        static KeyboardState previous;
        static bool previousLeftMouse, previousRightMouse;

        public static void UpdateMethods()
        {
            previous = Keyboard.GetState();

            MouseState state = Mouse.GetState();
            previousLeftMouse = state.LeftButton == ButtonState.Pressed;
            previousRightMouse = state.RightButton == ButtonState.Pressed;
        }

        public static bool KeyDown(Keys key) => (!previous.IsKeyDown(key) && Keyboard.GetState().IsKeyDown(key));

        public static bool LeftMouseDown => Mouse.GetState().LeftButton == ButtonState.Pressed && !previousLeftMouse;

        public static bool RightMouseDown => Mouse.GetState().RightButton == ButtonState.Pressed && !previousRightMouse;
    }
}
