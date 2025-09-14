using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Debug);
});

builder.Services.AddSingleton<WebSocketService>();
var app = builder.Build();
app.UseWebSockets();
var playerNames = new Dictionary<string, string>();

app.MapPost("/session", (SessionRequest request) => {
    var sessionId = Guid.NewGuid().ToString();
    playerNames[sessionId] = request.playerName;
    return Results.Ok(new { sessionId });
});

app.MapGet("/ws", async (string sessionId, HttpContext context, WebSocketService webSocketService) => {
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        if (playerNames.TryGetValue(sessionId, out var playerName))
        {
            var position = webSocketService.GenerateRandomPosition();
            var player = new PlayerState
            {
                SessionId = sessionId,
                Name = playerName,
                X = position.X,
                Y = position.Y,
                LastActivity = DateTime.UtcNow
            };

            webSocketService.AddConnection(sessionId, webSocket);
            webSocketService.GameState.Players.Add(player);

            
            await webSocket.SendAsync(
                System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(webSocketService.GameState),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);

            await WebSocketHandler.HandleWebSocketMessages(webSocket, sessionId, webSocketService);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();
