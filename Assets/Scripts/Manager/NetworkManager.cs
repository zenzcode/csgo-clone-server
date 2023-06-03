using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using Helper;
using Riptide;
using Riptide.Utils;
using UnityEngine;

namespace Manager
{
    public class NetworkManager : SingletonMonoBehavior<NetworkManager>
    {
        public Server Server { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
            
#if UNITY_EDITOR
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, true);
#else
            RiptideLogger.Initialize(Debug.Log, true);
#endif
            
            Server = new Server();
            Server.Start(27901, 10);
            Server.ClientDisconnected += Server_ClientDisconnected;
        }

        private void FixedUpdate()
        {
            Server.Update();
        }

        private void OnApplicationQuit()
        {
            Server.Stop();
            Server.ClientDisconnected -= Server_ClientDisconnected;
        }

        private void Server_ClientDisconnected(object o, ServerDisconnectedEventArgs eventArgs)
        {
            EventHandler.Instance.CallClientDisconnected(eventArgs.Client.Id);
        }

        [MessageHandler((ushort)ClientToServerMessages.Username)]
        private static void UsernameReceived(ushort sender, Message message)
        {
            var username = message.GetString();
            EventHandler.Instance.CallPlayerSetupReceived(sender, username);
            //Add to one of the teams
            TeamManager.Instance.AddPlayer(sender);
        }

        [MessageHandler((ushort)ClientToServerMessages.RequestRTT)]
        private static void RttRequest(ushort sender, Message message)
        {
            var response = Message.Create(MessageSendMode.Unreliable, (ushort)ServerToClientMessages.RTTAnswer);
            //TODO: Add Tick later to recognize lost package.
            //TODO: Add server time to calculate client/server time delta?
            response.AddFloat(message.GetFloat());
            Instance.Server.Send(response, sender);
        }

    }  
}

