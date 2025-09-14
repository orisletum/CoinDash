using CoinDash.Connection;
using System.Collections.Generic;
using System.Linq;

namespace CoinDash.Slime
{
    public class UpdateSlimeService
    {
        public void UpdatePlayers(GameState state, List<SlimeView> slimeViews)
        {
            var players = state.Players.ToList();

            for (int p = 0; p < players.Count; p++)
            {
                var slime = slimeViews.Where(sl => sl.SessionID == players[p].SessionId).FirstOrDefault();
                if (slime != null)
                {
                    slime.SetNewPosition(players[p].X, players[p].Y);
                    slime.SetCurrentScore(players[p].Score);
                }
            }

        }


    }
}