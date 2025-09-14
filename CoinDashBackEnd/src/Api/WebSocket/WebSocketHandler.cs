using System.Net.WebSockets;
using System.Text;
public static class WebSocketHandler
{
    public static async Task HandleWebSocketMessages(WebSocket webSocket, string sessionId, WebSocketService manager)
    {
        var buffer = new byte[1024 * 64];
        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var direction = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    manager.UpdatePlayerDirection(sessionId, direction);
                    manager.GameState.Players.First(p => p.SessionId == sessionId).LastActivity = DateTime.UtcNow;
                }
            }
        }
        catch (WebSocketException ex)
        {
            if (ex.ErrorCode == 1000) 
            {
                manager.SendMessage("WebSocket closed normally (code 1000)");
            }
            else
            {
                manager.SendMessage($"WebSocket error (code: {ex.ErrorCode}): {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            manager.SendMessage($"WebSocket error: {ex.Message}");
        }
        finally
        {
            manager.SendMessage($"WebSocketHandler finally removeConnection " + sessionId);
            manager.RemoveConnection(sessionId);
        }
    }
}
