using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using Helper;
using Riptide;

namespace Manager
{
    public class GameManager : SingletonMonoBehavior<GameManager>
    {
        public GameState State { get; private set; } = GameState.Lobby;

        private bool _isTimerRunning = false;
        [SerializeField] private int WarmupTime = 10;
        private float _remainingSeconds;

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
                Debug.Log("TIMER COMPLETE");
            }
        }
    }

}
