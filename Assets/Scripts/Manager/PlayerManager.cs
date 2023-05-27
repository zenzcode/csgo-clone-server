using System.Collections;
using System.Collections.Generic;
using Assets;
using Helper;
using Manager;
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

    private void EventHandler_PlayerSetup(ushort clientId, string username)
    {
        if (_players.ContainsKey(clientId))
        {
            //Client is already in list
            return;
        }

        var newPlayer = Instantiate(AssetManager.Instance.LobbyPlayer);

        if (!newPlayer.TryGetComponent<Player.Player>(out var player))
        {
            return;
        }

        player.PlayerId = clientId;
        player.Username = username;
        newPlayer.name = $"{player.Username} ({player.PlayerId})";
        _players.Add(clientId, player);
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
