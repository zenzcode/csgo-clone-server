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

        private float _startupTime;

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

        private void Start()
        {
            _startupTime = Time.timeSinceLevelLoad;
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

        private void SendRttUpdateMessage(ushort clientId, float rtt)
        {
            var message = Message.Create(MessageSendMode.Unreliable, (ushort)ServerToClientMessages.RTTUpdate);
            message.AddUShort(clientId);
            message.AddFloat(rtt);
            Server.SendToAll(message);
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
            response.AddFloat(message.GetFloat());
            response.AddFloat(Instance._startupTime);
            response.AddFloat(Time.timeSinceLevelLoad);
            Instance.Server.Send(response, sender);
        }

        [MessageHandler((ushort)ClientToServerMessages.RTTUpdate)]
        private static void RttUpdated(ushort senderId, Message message)
        {
            var newRtt = message.GetFloat();
            var player = PlayerManager.Instance.GetPlayer(senderId);
            if (!player)
                return;
            
            player.LastKnownRtt = newRtt;
            Instance.SendRttUpdateMessage(senderId, newRtt);
        }

        [MessageHandler((ushort)ClientToServerMessages.KickRequest)]
        private static void KickRequest(ushort sender, Message message)
        {
            var kickedId = message.GetUShort();
            var currentLeader = PlayerManager.Instance.GetCurrentLeader();

            if (!currentLeader)
            {
                Debug.Log("There is no current leader");
                return;
            }

            if (sender != currentLeader.PlayerId && sender != kickedId)
            {
                Debug.Log($"Either {sender} is not the current leader ({currentLeader.PlayerId}) or we try to kick ourself ({kickedId})");
                return;
            }

            Instance.Server.DisconnectClient(kickedId);
        }
    }  
}

