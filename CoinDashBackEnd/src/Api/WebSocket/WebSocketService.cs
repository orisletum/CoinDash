using System.Data;
using System.Net.WebSockets;

public class WebSocketService 
{
    private readonly ILogger<WebSocketService>          _logger;
    private readonly Dictionary<string, WebSocket>      _connections = new();
    private readonly GameState _gameState = new();
    public GameState                                    GameState => _gameState;
    private readonly System.Timers.Timer                _gameTimer;
    private DateTime                                    _lastCoinSpawn = DateTime.UtcNow;
    public WebSocketService(ILogger<WebSocketService> logger)
    {
        _logger = logger;
        _gameTimer = new System.Timers.Timer(100); 
        _gameTimer.Elapsed += (s, e) => UpdateGameState();
        _gameTimer.Start();
    }
    public void SendMessage(string message) => _logger.LogInformation($"message: {message}");

    public (float X, float Y) GenerateRandomPosition() => (Random.Shared.Next(0, 16), Random.Shared.Next(0, 9));

    private float CalculateDistance(float x, float y) => (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

    public void AddConnection(string sessionId, WebSocket webSocket) => _connections[sessionId] = webSocket;
   
    public void RemoveConnection(string sessionId)
    {
        if (_connections.TryGetValue(sessionId, out var webSocket))
        {
            try
            {
                if (webSocket.State == WebSocketState.Open ||
                    webSocket.State == WebSocketState.CloseReceived ||
                    webSocket.State == WebSocketState.CloseSent)
                {
                    webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected", CancellationToken.None).Wait();
                }
                else
                {
                    SendMessage($"WebSocket state {webSocket.State}");
                }
            }
            catch (Exception ex)
            {
                SendMessage($"Error: {ex.Message}");
            }
            finally
            {
                _connections.Remove(sessionId);
                _gameState.Players.RemoveAll(p => p.SessionId == sessionId);
            }
        }
    }


    private void UpdateGameState()
    {
        float spawnCoinTime = 3;
        float maxCoinValue = 10;
        var playerList = _gameState.Players.ToList();
        if (playerList.Count == 0) return;
        var playersToRemove = playerList
        .Where(p => DateTime.UtcNow - p.LastActivity > TimeSpan.FromSeconds(20))
        .ToList();

        foreach (var player in playersToRemove)
        {
            _gameState.Players.Remove(player);
        }

        if (playerList.Count > 0 &&
           DateTime.UtcNow - _lastCoinSpawn > TimeSpan.FromSeconds(spawnCoinTime) &&
           _gameState.Coins.Count < maxCoinValue)
        {
            var newPosition = GenerateRandomPosition();

            var coin = new CoinState
            {
                Id = Guid.NewGuid(),
                X = newPosition.X,
                Y = newPosition.Y
            };
            _gameState.Coins.Add(coin);
            _lastCoinSpawn = DateTime.UtcNow;
        }

        var coinsToRemove = new List<CoinState>();
        foreach (var player in playerList)
        {
            foreach (var coin in _gameState.Coins)
            {
                if (CalculateDistance(player.X - coin.X, player.Y - coin.Y) < 0.5f)
                {
                    coinsToRemove.Add(coin);
                    player.Score++;
                    _logger.LogInformation($"lucky Player: {player.Name} {player.Score}");
                    break; 
                }
            }
        }

        foreach (var coin in coinsToRemove)
        {
            _gameState.Coins.Remove(coin);
            _logger.LogInformation($"Remove Coins: {coin.Id}");
        }

        BroadcastGameState().Wait();
    }

    public async Task BroadcastGameState()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(_gameState);

        foreach (var connection in _connections.Values)
        {
            try
            {
                await connection.SendAsync(
                    System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(_gameState),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                SendMessage($"Error: {ex.Message}");
            }
        }
    }

    public void UpdatePlayerDirection(string sessionId, string direction)
    {
        var player = _gameState.Players.FirstOrDefault(p => p.SessionId == sessionId);
        SendMessage($"direction: {direction}");

        if (player != null)
        {
            switch (direction)
            {
                case "up":  player.Y = Math.Clamp(player.Y + 0.5f, 0, 9); break;
                case "down": player.Y = Math.Clamp(player.Y - 0.5f, 0, 9); break;
                case "left": player.X = Math.Clamp(player.X - 0.5f, 0, 16); break;
                case "right": player.X = Math.Clamp(player.X + 0.5f, 0, 16); break;
            }
            player.LastActivity = DateTime.UtcNow;
        }
    }
}