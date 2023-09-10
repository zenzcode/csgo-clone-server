using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using Helper;
using Riptide;
using Maps;
using UnityEngine.SceneManagement;
using Misc;

namespace Manager
{
    public class GameManager : SingletonMonoBehavior<GameManager>
    {
        public GameState State { get; private set; } = GameState.Lobby;
        

        [SerializeField] private MapSO _map;

        private void OnEnable()
        {
            EventHandler.Instance.ClientDisconnected += EventHandler_ClientDisconnected;
        }

        private void OnDisable()
        {
            EventHandler.Instance.ClientDisconnected -= EventHandler_ClientDisconnected;
        }

        public void SetGameState(GameState newState)
        {
            State = newState;
            var message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientMessages.GameStateUpdated);
            message.AddUShort((ushort)newState);
            NetworkManager.Instance.Server.SendToAll(message);
        }

        public void SendTravelSignal()
        {
            var message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientMessages.Travel);
            message.AddString(_map.PathToMap);
            NetworkManager.Instance.Server.SendToAll(message);
        }

        public void LoadSelectedMap()
        {
            SceneManager.LoadSceneAsync(_map.PathToMap, LoadSceneMode.Additive);
        }

        private void EventHandler_ClientDisconnected(ushort clientId)
        {
            if(PlayerManager.Instance.GetPlayerCount() == 0)
            {
                if(State != GameState.InGame)
                {
                    return;
                }

                //go back to lobby scene
                SceneManager.UnloadSceneAsync(GetSelectedMapSceneName());
                //Allow Joining Again
                SetGameState(GameState.Lobby);
                //TODO: Cleanup Managers
            }
        }

        public string GetSelectedMapSceneName()
        {
            return _map.PathToMap;
        }
    }

}
