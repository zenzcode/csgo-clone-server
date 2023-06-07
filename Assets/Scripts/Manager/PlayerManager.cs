using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Assets;
using Enums;
using Helper;
using Manager;
using Riptide;
using UnityEngine;

public class PlayerManager : SingletonMonoBehavior<PlayerManager>
{
    private Dictionary<ushort, Player.Player> _players;

    protected override void Awake()
    {
        base.Awake();
        _players = new Dictionary<ushort, Player.Player>();
        DontDestroyOnLoad(this);
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

    public Player.Player GetCurrentLeader()
    {
        return _players.FirstOrDefault(player => player.Value.IsLeader).Value;
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

        //TODO: CHECK GAME STATE (LOBBY/INGAME)
        var newPlayer = Instantiate(AssetManager.Instance.LobbyPlayer);

        if (!newPlayer.TryGetComponent<Player.Player>(out var player))
        {
            return;
        }

        player.PlayerId = clientId;
        player.InitialUsername = username;
        player.Username = GetUniqueUsername(username);
        player.IsLeader = !_players.Values.Any(p => p.IsLeader);
        newPlayer.name = $"{player.Username} ({player.PlayerId})";
        _players.Add(clientId, player);
        SendSpawnMessage(player);
    }

    private string GetUniqueUsername(string username)
    {
        var numOfOccurences = _players.Values.Count(player => player.InitialUsername.Equals(username));
        if (numOfOccurences > 0)
        {
            return $"{username} ({numOfOccurences})";
        }

        return username;
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
        var message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientMessages.SpawnClient);
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

        Debug.Log(newLeader);
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

        if (player.IsLeader)
        {
            SetNewLeader(clientId);
            player.IsLeader = false;
        }
        
        Destroy(player.gameObject);
        _players.Remove(clientId);
    }
}
