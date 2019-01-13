using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Andromedroids
{
    public enum GameState { QuickMenu, MainMenu, InGame, Tournament, StartGame, End, Exit, Transition }

    class StateManager
    {
        public GameState GameState { get; private set; }
        public Stack<int> SubStateStack { get; private set; }

        public StateManager(GameState gameState, int subState)
        {
            GameState = gameState;

            SubStateStack = new Stack<int>();
            SubStateStack.Push(subState);
        }

        public void SetGameState(GameState gameState, int subState)
        {
            Debug.WriteLine("GameState set to: " + gameState.ToString());

            GameState = gameState;
            Replace(subState);
        }

        public void StackSubState(int state)
        {
            Debug.WriteLine("SubState set to: " + state);

            SubStateStack.Push(state);
        }

        public int Peek => SubStateStack.Peek();

        public int Pop => SubStateStack.Pop();

        public void Replace(int state)
        {
            SubStateStack = new Stack<int>();
            SubStateStack.Push(state);
        }
    }
}
