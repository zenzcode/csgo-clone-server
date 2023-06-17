using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using Helper;
using UnityEngine;

namespace Manager
{
    public class EventHandler : SingletonMonoBehavior<EventHandler>
    {

        public Action<ushort, string> PlayerSetupReceived;
        public void CallPlayerSetupReceived(ushort clientId, string username)
        {
            PlayerSetupReceived?.Invoke(clientId, username);
        }

        public Action<ushort> ClientDisconnected;

        public void CallClientDisconnected(ushort clientId)
        {
            ClientDisconnected?.Invoke(clientId);
        }

        public Action<Enums.Timer> TimerFinished;

        public void CallTimerFinished(Enums.Timer timer)
        {
            TimerFinished?.Invoke(timer);
        }
    }
}

