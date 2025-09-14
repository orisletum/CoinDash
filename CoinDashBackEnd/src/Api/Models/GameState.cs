public class GameState
{
    public List<PlayerState>    Players { get; set; } = new();
    public List<CoinState>      Coins { get; set; } = new();
}