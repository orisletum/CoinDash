using CoinDash.Coin;
using CoinDash.Slime;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace CoinDash.Connection
{
    public enum InputSystem
    {
        Auto,
        Mobile,
        Keyboard
    }
    public class WebSocketConnectionService
    {
        private InputSystem                 _inputSystem = InputSystem.Auto;
        private float                       _lastPingTime;
        private ClientWebSocket             _webSocket;
        private string                      _serverUrl = "ws://localhost:8080/ws?sessionId=";
        private string                      _sessionId;
        private UpdateSlimeService          _updateSlimeService;
        private SpawnSlimeService           _spawnSlimeService;
        private UpdateCoinService           _updateCoinService;
        private JoystiñkForMovement         _joystiñkForMovement;

        public bool                         IsConnected = false;
        [Inject]
        public void Construct(JoystiñkForMovement joystiñkForMovement, UpdateSlimeService updateSlimeService, SpawnSlimeService spawnSlimeService, UpdateCoinService updateCoinService)
        {
            _joystiñkForMovement = joystiñkForMovement;
            _updateSlimeService = updateSlimeService;
            _spawnSlimeService = spawnSlimeService;
            _updateCoinService = updateCoinService;
            Initialize();
        }

        public void Initialize()
        {
            bool isMobile;
            switch (_inputSystem)
            {
                case InputSystem.Auto:
                    isMobile = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
                    break;
                case InputSystem.Mobile:
                    isMobile = true;
                    break;
                case InputSystem.Keyboard:
                    isMobile = false;
                    break;
                default:
                    isMobile = true;
                    break;
            }
            
            Observable.EveryUpdate()
                .Where(_ => IsConnected)
                .ThrottleFirst(TimeSpan.FromSeconds(0.1f)) 
                .Subscribe(_ =>
                {
                    if (isMobile)
                        UpdateMobileMovement();
                    else
                        UpdateMovement();
                });
        }

        private void UpdateMobileMovement()
        {
            _joystiñkForMovement.gameObject.SetActive(true);
            if(_joystiñkForMovement.InputVector.x>0.25f) SendDirection("right");
            else if (_joystiñkForMovement.InputVector.x < -0.25f) SendDirection("left");
            if (_joystiñkForMovement.InputVector.y > 0.25f) SendDirection("down");
            else if (_joystiñkForMovement.InputVector.y < -0.25f) SendDirection("up");
          
        }

        public void UpdateMovement()
        {
            if (Input.GetKey(KeyCode.W)) SendDirection("up");
            if (Input.GetKey(KeyCode.S)) SendDirection("down");
            if (Input.GetKey(KeyCode.A)) SendDirection("left");
            if (Input.GetKey(KeyCode.D)) SendDirection("right");

            // Ïèíã êàæäûå 5 ñåê
            if (Time.time - _lastPingTime > 5f)
            {
                SendDirection("ping");
                _lastPingTime = Time.time;
            }

        }
      
        public async Task ConnectToWebSocket(string sessionId)
        {
            _sessionId = sessionId;
            _webSocket = new ClientWebSocket();
            _webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(5);

            try
            {
                await _webSocket.ConnectAsync(new Uri(_serverUrl + _sessionId), CancellationToken.None);
                IsConnected = true;

                Debug.Log("WebSocket connected!");

               
                await ReceiveMessages();
            }
            catch (Exception e)
            {
                Debug.LogError($"Connection error: {e.Message}");
            }
        }

        private async Task ReceiveMessages()
        {

            var buffer = new byte[1024 * 64];
            while (IsConnected)
            {
                try
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Debug.Log($"GameState: {json}");
                        GameState state = JsonUtility.FromJson<GameState>(json);

                        if (state == null)
                        {
                            Debug.LogError($"Failed: {json}");
                            continue;
                        }
                        var player = state.Players.FirstOrDefault(p => p.SessionId == _sessionId);
                        _spawnSlimeService.RemoveNonExSlimes(state.Players);
                        _spawnSlimeService.CreateNewSlimes(state.Players);
                        _updateCoinService.SpawnCoins(state.Coins);
                        _updateSlimeService.UpdatePlayers(state, _spawnSlimeService.Slimes);
                        GameActions.UpdateScoreAction.Invoke(player.Score);
                        GameActions.UpdateLeaderboardAction.Invoke(state.Players);
                    }
                }
                catch (Exception e)
                {
                    if (IsConnected)
                    {
                        IsConnected = false;
                    }
                    break;
                }
            }

        }
        private void SendDirection(string direction)
        {
            //Debug.LogError($"direction: {direction}");
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var bytes = Encoding.UTF8.GetBytes(direction);
                    _webSocket.SendAsync(
                        new ArraySegment<byte>(bytes),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                }
                catch (Exception e)
                {
                    Debug.LogError($"Send error: {e.Message}");
                }
            }
            else
            {
                Debug.LogError("WebSocket closed");
            }
        }


    }
}