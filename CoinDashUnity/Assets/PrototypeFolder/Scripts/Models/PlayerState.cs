using System;

namespace CoinDash.Connection
{
    [Serializable]
    public class PlayerState
    {
        public string SessionId;
        public string Name;
        public float X;
        public float Y;
        public int Score;
        //DateTime LastActivity;
    }
}