using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using Helper;
using Riptide;
using Maps;
using UnityEngine.SceneManagement;

namespace Manager
{
    public class GameManager : SingletonMonoBehavior<GameManager>
    {
        public GameState State { get; private set; } = GameState.Lobby;

        private bool _isTimerRunning = false;
        [SerializeField] private int WarmupTime = 10;
        private float _remainingSeconds;

        [SerializeField] private MapSO _map;

        private Timer _runningTimer = Timer.None;


        [MessageHandler((ushort)ClientToServerMessages.StartGameTimer)]
        private static void StartTimerRequest(ushort senderId, Message message)
        {
            Instance.TryStartGameTimer(senderId);
        }

        private void TryStartGameTimer(ushort playerId)
        {
            if (_isTimerRunning)
            {
                return;
            }

            var auth = PlayerManager.Instance.GetCurrentLeader();
            if(!auth)
            {
                return;
            }

            if (auth.PlayerId != playerId)
            {
                return;
            }

            SendTimerStartMessage(Timer.WarmupTimer);
        }

        private void SendTimerStartMessage(Timer timer)
        {
            var message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientMessages.TimerStarted);
            message.AddUShort((ushort)timer);
            switch(timer)
            {
                case Timer.WarmupTimer:
                    message.AddInt(WarmupTime);
                    _remainingSeconds = WarmupTime;
                    break;
            }

            message.AddFloat(Time.timeSinceLevelLoad);

            NetworkManager.Instance.Server.SendToAll(message);
            _isTimerRunning = true;
            _runningTimer = timer;
            SceneManager.LoadSceneAsync(_map.PathToMap, LoadSceneMode.Additive);
        }

        private void Update()
        {
            if(!_isTimerRunning)
            {
                return;
            }
            _remainingSeconds -= (1 * Time.deltaTime);

            if(_remainingSeconds <= 0)
            {
                _isTimerRunning = false;
                HandleTimerComplete();
                EventHandler.Instance.CallTimerFinished(_runningTimer);
                SetGameState(GameState.Warmup);
                _runningTimer = Timer.None;
            }
        }

        private void SetGameState(GameState newState)
        {
            State = newState;
            var message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientMessages.GameStateUpdated);
            message.AddUShort((ushort)newState);
            NetworkManager.Instance.Server.SendToAll(message);
        }

        private void HandleTimerComplete()
        {
            if(_runningTimer == Timer.WarmupTimer)
            {
                TravelTo(_map.PathToMap);
            }
        }

        private void TravelTo(string level)
        {
            var message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientMessages.Travel);
            message.AddString(level);
            NetworkManager.Instance.Server.SendToAll(message);
        }
    }

}
