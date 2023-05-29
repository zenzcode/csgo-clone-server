using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        return _players.First(pair => pair.Value.PlayerId == clientId).Value;
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
        player.Username = username;
        newPlayer.name = $"{player.Username} ({player.PlayerId})";
        _players.Add(clientId, player);
        SendSpawnMessage(player);
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
        return message;
    }

    private void EventHandler_ClientDisconnected(ushort clientId)
    {
        if (!_players.ContainsKey(clientId))
        {
            return;
        }
        Destroy(_players[clientId].gameObject);
        _players.Remove(clientId);
    }
}
