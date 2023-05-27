using System;
using System.Collections;
using System.Collections.Generic;
using Helper;
using UnityEngine;

namespace Manager
{
    public class EventHandler : SingletonMonoBehavior<EventHandler>
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
        }

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
    }
}

