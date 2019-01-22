using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andromedroids
{
    class TournamentBracket
    {
        public Slot[][] Bracket { get; private set; }
        public Queue<Match> Matches { get; private set; }
        public Match CurrentMatch { get; private set; }

        public void GetNextMatch(int winner)
        {
            Bracket[CurrentMatch.row][CurrentMatch.slot].state = winner == 0 ? Slot.State.Won : Slot.State.Lost;
            Bracket[CurrentMatch.row][CurrentMatch.slot + 1].state = winner == 1 ? Slot.State.Won : Slot.State.Lost;
            Bracket[CurrentMatch.row - 1][CurrentMatch.slot / 2] = new Slot(Bracket[CurrentMatch.row][CurrentMatch.slot + winner].player);

            CurrentMatch = Matches.Dequeue();
        }

        public TournamentBracket(PlayerManager[] players)
        {
            int playerCount = players.Length;
            int currentPlayer = -1;
            int lowestPower = playerCount.LowestPowerMoreThanOrEqual(out int lPower);
            int highestPower = playerCount.HighestPowerLessThanOrEqual(out int hPower);
            int excess = playerCount - highestPower;

            Bracket = new Slot[lPower + 1][];
            Bracket[Bracket.Length - 1] = new Slot[lowestPower];

            bool[] nestedPlayers = new bool[highestPower];
            int excessRemaining = excess, currentPower = 1, currentStep = 1, currentStepSize = 0;

            // 

            while (excessRemaining > 0)
            {
                if (currentStep == currentPower)
                {
                    currentPower *= 2;
                    currentStep = 0;
                    currentStepSize = highestPower / currentPower;
                }

                if (nestedPlayers[currentStep * currentStepSize])
                {
                    ++currentStep;
                    continue;
                }

                nestedPlayers[currentStep * currentStepSize] = true;

                ++currentStep;
                --excessRemaining;
            }

            // Filling the bracket array with the most fair arrangment of players

            for (int i = 0; i <= hPower; ++i)
            {
                Bracket[i] = new Slot[(int)Math.Pow(2, i)];

                for (int j = 0; j < Bracket[i].Length; j++)
                {
                    if (i < hPower)
                    {
                        Bracket[i][j] = new Slot(null);
                        continue;
                    }

                    if (i == hPower)
                    {
                        if (nestedPlayers[j])
                        {
                            Bracket[i][j] = new Slot(null);
                            Bracket[i + 1][j * 2] = new Slot(players[++currentPlayer]);
                            Bracket[i + 1][j * 2 + 1] = new Slot(players[++currentPlayer]);

                            continue;
                        }

                        Bracket[i][j] = new Slot(players[++currentPlayer]); 
                    }
                }
            }

            // Set up a queue of matches that will be played

            Matches = new Queue<Match>();

            for (int i = lPower; i >= 0; --i)
            {
                for (int j = 0; j < Bracket[i].Length; j += 2)
                {
                    if (Bracket[i][j] == null)
                    {
                        continue;
                    }

                    Matches.Enqueue(new Match() { row = i, slot = j });
                }
            }

            CurrentMatch = Matches.Dequeue();
        }

        public struct Match
        {
            public int row, slot;
        }

        public class Slot
        {
            public enum State
            {
                Empty, Waiting, Lost, Won
            }

            public PlayerManager player;
            public State state;

            public Slot(PlayerManager player)
            {
                if (player != null)
                {
                    this.player = player;
                    state = State.Waiting;
                    return;
                }

                this.player = null;
                state = State.Empty;
            }
        }
    }
}
