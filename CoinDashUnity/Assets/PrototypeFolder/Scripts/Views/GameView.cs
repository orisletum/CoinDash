using UnityEngine;
using System.Linq;
using TMPro;
using CoinDash.Connection;
using System.Text;

namespace CoinDash.UI
{
    public class GameView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private LoginView  _loginView;
        public LoginView                    LoginView => _loginView;

        [SerializeField] private TMP_Text   _scoreText;
        [SerializeField] private TMP_Text   _leaderboardText;

        private void OnEnable()
        {
            GameActions.UpdateScoreAction += UpdateScoreText;
            GameActions.UpdateLeaderboardAction += UpdateLeaderboard;
        }

        private void OnDisable()
        {
            GameActions.UpdateScoreAction -= UpdateScoreText;
            GameActions.UpdateLeaderboardAction -= UpdateLeaderboard;
        }

        private void UpdateScoreText(int value) => _scoreText.text = $"Score: {value}";

        private void UpdateLeaderboard(PlayerState[] players)
        {
            StringBuilder leaderboardMessage = new StringBuilder();
            var sortedPlayers = players.OrderByDescending(playerState => playerState.Score).Take(3);

            foreach (var playerData in sortedPlayers)
                leaderboardMessage.AppendLine($"{playerData.Name}: {playerData.Score}\n");

            leaderboardMessage.ToString().Trim();
            _leaderboardText.text = leaderboardMessage.ToString();
        }
    }
}