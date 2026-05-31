using System;

namespace CoinDash.Connection
{
    [Serializable]
    public class BossState
    {
        public string SessionId;
        public string Name;
        public float X;
        public float Y;
        public int Score;
        public string State;
        //DateTime LastActivity;
    }
}