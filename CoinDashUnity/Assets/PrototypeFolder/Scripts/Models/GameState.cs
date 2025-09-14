using System;

namespace CoinDash.Connection
{
    [Serializable]
    public class GameState
    {
        public PlayerState[] Players;
        public CoinState[] Coins;
        public bool IsFinal;
    }
}