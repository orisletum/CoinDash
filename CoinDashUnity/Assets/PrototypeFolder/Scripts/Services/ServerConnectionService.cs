using CoinDash.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace CoinDash.Connection
{
    public class ServerConnectionService
    {
        private IDisposable                 _subscription;
        private GameView                    _serverConnection;
        private WebSocketConnectionService  _webSocketConnection;

        [Inject]
        public void Construct(GameView serverConnection, WebSocketConnectionService webSocketConnection)
        {
            _serverConnection = serverConnection;
            _webSocketConnection = webSocketConnection;
            Initialize();
        }

        public void Initialize()
        {
            _subscription = _serverConnection.LoginView.ConnectButton
                .OnClickAsObservable()
                .Subscribe(_ => SendName());
        }

        public void Dispose() => _subscription?.Dispose();
      
        private void SendName()
        {
            var playerName = _serverConnection.LoginView.NameInputField.text;
            Debug.Log($"playerName: {playerName}");
            if (string.IsNullOrEmpty(playerName)) return;
            
            StartSession(playerName);
        }
        SessionResponse sessionResponse;
        private async void StartSession(string playerName)
        {
            
            try 
            {
                sessionResponse = await SendSessionRequest(playerName);
                
            }
            catch (Exception ex)
            {
                Debug.Log($"Failed: {ex.Message}");
            }
            finally 
            {
                if (sessionResponse != null) 
                {
                    _serverConnection.LoginView.gameObject.SetActive(false);
                    string sessionId = sessionResponse.sessionId;
                    Debug.Log($"Session started: {sessionId}");

                    await _webSocketConnection.ConnectToWebSocket(sessionId);
                }
            }
            
        }

        private async Task<SessionResponse> SendSessionRequest(string playerName)
        {
            var request = new SessionRequest { playerName = playerName };
            var json = JsonUtility.ToJson(request);

            using (var handler = new UnityWebRequest("http://localhost:8080/session", "POST"))
            {
                handler.SetRequestHeader("Content-Type", "application/json");
                handler.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                handler.downloadHandler = new DownloadHandlerBuffer();

                await handler.SendWebRequest();

                if (handler.result == UnityWebRequest.Result.Success)
                {
                    return JsonUtility.FromJson<SessionResponse>(handler.downloadHandler.text);
                }
            }
            return null;
        }
    }
}