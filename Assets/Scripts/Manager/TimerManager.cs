using Enums;
using Helper;
using Manager;
using Riptide;
using System.Collections.Generic;
using System.Linq;
using Misc;
using TimerManagement;
using UnityEngine;

namespace TimerManagement
{
    struct RunningTimer
    {
        public Enums.Timer Timer;
        public float RemainingSeconds;
    }
}

namespace Managers
{
    [DisallowMultipleComponent]
    public class TimerManager : SingletonMonoBehavior<TimerManager>
    {
        private List<RunningTimer> _runningTimer;

        protected override void Awake()
        {
            base.Awake();
            _runningTimer = new List<RunningTimer>();
        }

        [MessageHandler((ushort)ClientToServerMessages.StartTimerRequest)]
        private static void StartTimerRequest(ushort senderId, Message message)
        {
            var timer = (Enums.Timer)message.GetUShort();
            Instance.TryStartTimer(senderId, timer);
        }

        private void TryStartTimer(ushort playerId, Enums.Timer timer)
        {
            if (IsTimerRunning(timer))
            {
                return;
            }

            var auth = PlayerManager.Instance.GetCurrentLeader();
            if (!auth)
            {
                return;
            }

            if (auth.PlayerId != playerId)
            {
                return;
            }

            SendTimerStartMessage(timer);
        }

        private void SendTimerStartMessage(Enums.Timer timer)
        {
            var message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientMessages.TimerStarted);
            message.AddUShort((ushort)timer);

            var newRunningTimer = new RunningTimer()
            {
                Timer = timer
            };

            switch (timer)
            {
                case Enums.Timer.LobbyTimer:
                    //Dont start timer when we are less than min players
                    if (PlayerManager.Instance.GetPlayerCount() < Statics.MinPlayerCount)
                    {
                        return;
                    }
                    message.AddInt(Statics.LobbyTime);
                    newRunningTimer.RemainingSeconds = Statics.LobbyTime;
                    GameManager.Instance.SetGameState(GameState.PreparingGame);
                    GameManager.Instance.LoadSelectedMap();
                    break;
            }

            message.AddFloat(Time.timeSinceLevelLoad);

            NetworkManager.Instance.Server.SendToAll(message);
            _runningTimer.Add(newRunningTimer);
        }

        private void Update()
        {
            if (!IsAnyTimerActive())
            {
                return;
            }

            for(int i = _runningTimer.Count - 1; i >= 0; --i)
            {
                var runningTimer = _runningTimer[i];
                runningTimer.RemainingSeconds -= (1 * Time.deltaTime);
                Debug.Log($"REMAINING FOR {runningTimer.Timer} = {runningTimer.RemainingSeconds}");

                if (runningTimer.RemainingSeconds <= 0)
                {
                    HandleTimerComplete(runningTimer.Timer);
                    EventHandler.Instance.CallTimerFinished(runningTimer.Timer);
                    _runningTimer.RemoveAt(i);
                    continue;
                }

                _runningTimer[i] = runningTimer;
            }
        }

        private void HandleTimerComplete(Enums.Timer timer)
        {
            //On Lobby Timer Complete
            if (timer == Enums.Timer.LobbyTimer)
            {
                
                GameManager.Instance.SetGameState(GameState.Warmup);
                GameManager.Instance.SendTravelSignal();
            }
        }

        private bool IsTimerRunning(Enums.Timer timer)
        {
            return _runningTimer.Any(t => t.Timer == timer);
        }

        private bool IsAnyTimerActive()
        {
            return _runningTimer.Count > 0;
        }
    }
}