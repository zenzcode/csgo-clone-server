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
        

        [SerializeField] private MapSO _map;

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

        public string GetSelectedMapSceneName()
        {
            return _map.PathToMap;
        }
    }

}
