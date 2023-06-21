using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Assets;
using Enums;
using Helper;
using Manager;
using Player;
using Riptide;
using UnityEngine;

public class PlayerManager : SingletonMonoBehavior<PlayerManager>
{
    private Dictionary<ushort, Player.Player> _players;
    [HideInInspector] public Dictionary<ushort, Player.Player> Players => _players;
    private Dictionary<string, Dictionary<int, Player.Player>> _usedNicks;

    protected override void Awake()
    {
        base.Awake();
        _players = new Dictionary<ushort, Player.Player>();
        _usedNicks = new Dictionary<string, Dictionary<int, Player.Player>>();
    }

    private void OnEnable()
    {
        EventHandler.Instance.PlayerSetupReceived += EventHandler_PlayerSetup;
        EventHandler.Instance.ClientDisconnected += EventHandler_ClientDisconnected;
    }

    private void OnDisable()
    {
        EventHandler.Instance.PlayerSetupReceived -= EventHandler_PlayerSetup;
        EventHandler.Instance.ClientDisconnected -= EventHandler_ClientDisconnected;
    }

    public Player.Player GetPlayer(ushort clientId)
    {
        return _players.Values.FirstOrDefault(p => p.PlayerId == clientId);
    }

    public int GetPlayerCount()
    {
        return _players.Count;
    }

    public Player.Player GetCurrentLeader()
    {
        return _players.FirstOrDefault(player => player.Value.IsLeader).Value;
    }

    public void RemoveVisualRepresentations()
    {
        foreach (var player in _players.Values)
        {
            var controller = player.gameObject.GetComponentInChildren<PlayerController>();

            if (!controller)
                continue;

            player.gameObject.transform.position = Vector3.zero;
            
            Destroy(controller.gameObject);
        }
    }
    
    private void EventHandler_PlayerSetup(ushort clientId, string username)
    {
        if (_players.ContainsKey(clientId))
        {
            //Client is already in list
            return;
        }

        foreach(var p in _players.Values)
        {
            SendSpawnMessage(clientId, p);
        }

        var newPlayer = Instantiate(AssetManager.Instance.LobbyPlayer);

        if (!newPlayer.TryGetComponent<Player.Player>(out var player))
        {
            return;
        }

        player.PlayerId = clientId;
        player.Username = GetUniqueUsername(username, player);
        player.InitialUsername = username;
        player.IsLeader = !_players.Values.Any(p => p.IsLeader);
        newPlayer.name = $"{player.Username} ({player.PlayerId})";
        _players.Add(clientId, player);
        SendSpawnMessage(player);
        
        if (GameManager.Instance.State != GameState.Lobby)
        {
            var connectedId = clientId;
            NetworkManager.Instance.Server.DisconnectClient(clientId);
        }
    }

    private string GetUniqueUsername(string username, Player.Player player)
    {
        var numOfOccurences = _players.Values.Count(player => player.InitialUsername.Equals(username));
        Debug.Log($"{numOfOccurences} occurences from {_players.Count} players");
        if (numOfOccurences > 0)
        {
            var num = numOfOccurences;
            if (_usedNicks.ContainsKey(username))
            {
                foreach (var keyvalue in _usedNicks[username])
                {
                    if (keyvalue.Value == null)
                    {
                        num = keyvalue.Key;
                    }
                }
            }
            AddToUsedNicks(num, player, username);
            return $"{username} ({num})";
        }

        return username;
    }

    private void AddToUsedNicks(int numOfOccurences, Player.Player player, string username)
    {
        if(_usedNicks.ContainsKey(username))
        {
            if (!_usedNicks[username].ContainsKey(numOfOccurences))
            {
                _usedNicks[username].Add(numOfOccurences, player);
            }
            else
            {
                _usedNicks[username][numOfOccurences] = player;
            }
        }
        else
        {
            _usedNicks.Add(username, new Dictionary<int, Player.Player>());
            _usedNicks[username].Add(numOfOccurences, player);
        }
    }

    private void SendSpawnMessage(Player.Player player)
    {
        NetworkManager.Instance.Server.SendToAll(SpawnMessage(player));
    }

    private void SendSpawnMessage(ushort receiver, Player.Player player)
    {

        NetworkManager.Instance.Server.Send(SpawnMessage(player), receiver);
    }

    private Message SpawnMessage(Player.Player player)
    {
        var message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientMessages.SpawnLobbyClient);
        message.AddUShort(player.PlayerId);
        message.AddString(player.Username);
        message.AddBool(player.IsLeader);
        message.AddFloat(player.LastKnownRtt);
        return message;
    }

    private void SetNewLeader(ushort oldLeaderId)
    {
        var newLeader = _players.Values.FirstOrDefault(p => !p.IsLeader);
        if (!newLeader)
        {
            return;
        }

        newLeader.IsLeader = true;

        var message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientMessages.LeaderChanged);
        message.AddUShort(oldLeaderId); 
        message.AddUShort(newLeader.PlayerId);
        NetworkManager.Instance.Server.SendToAll(message);
    }

    private void EventHandler_ClientDisconnected(ushort clientId)
    {
        if (!_players.ContainsKey(clientId))
        {
            return;
        }
        var player = _players[clientId];

        if(_usedNicks.ContainsKey(player.InitialUsername))
        {
            var kvp = _usedNicks[player.InitialUsername].FirstOrDefault(keyvalue => keyvalue.Value == player);
            if(kvp.Value == null)
            {
                return;
            }
            var num = kvp.Key;
            _usedNicks[player.InitialUsername][num] = null;

            //remove if no more duplicates
            if (!_usedNicks[player.InitialUsername].Any(kvp => kvp.Value != null))
            {
                _usedNicks.Remove(player.InitialUsername);
            }
        }

        if (player.IsLeader)
        {
            SetNewLeader(clientId);
            player.IsLeader = false;
        }
        
        Destroy(player.gameObject);
        _players.Remove(clientId);
    }


    [MessageHandler((ushort)ClientToServerMessages.TravelFinished)]
    private static void TravelFinished(ushort sender, Message message)
    {
        foreach (var player in PlayerManager.Instance.Players.Values)
        {
            if (player.ConnectedInMap)
            {
                Instance.SendMapSpawnMessage(sender, player);
            }
        }
        Instance.SendMapSpawnMessage(sender);
        Instance.GetPlayer(sender).ConnectedInMap = true;
    }

    private void SendMapSpawnMessage(ushort receiver, Player.Player player)
    {
        var message = GetSpawnMessage(player.PlayerId);
        NetworkManager.Instance.Server.Send(message, receiver);
    }

    private void SendMapSpawnMessage(ushort sender)
    {
        NetworkManager.Instance.Server.SendToAll(GetSpawnMessage(sender));
    }

    private Message GetSpawnMessage(ushort clientToSpawn)
    {
        var message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientMessages.SpawnInMap);
        var player = GetPlayer(clientToSpawn);
        message.AddUShort(clientToSpawn);
        message.AddVector3(player.gameObject.transform.position);
        message.AddQuaternion(player.gameObject.transform.rotation);
        return message;

    }
}
