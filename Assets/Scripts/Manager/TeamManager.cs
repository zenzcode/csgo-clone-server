using System.Collections;
using System.Collections.Generic;
using Enums;
using Helper;
using Riptide;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Manager
{
    public class TeamManager : SingletonMonoBehavior<TeamManager>
    {
        private Dictionary<Team, List<ushort>> _teamMembers;

        protected override void Awake()
        {
            base.Awake();
            _teamMembers = new Dictionary<Team, List<ushort>>
            {
                { Team.Attacker, new List<ushort>() },
                { Team.Defender, new List<ushort>() }
            };
        }

        private void OnEnable()
        {
            EventHandler.Instance.ClientDisconnected += EventHandler_ClientDisconnected;
        }

        private void OnDisable()
        {
            EventHandler.Instance.ClientDisconnected -= EventHandler_ClientDisconnected;
        }

        public void AddPlayer(ushort clientId)
        {
            var playerComp = PlayerManager.Instance.GetPlayer(clientId);
            if (!playerComp)
            {
                return;
            }

            foreach (var team in _teamMembers.Keys)
            {
                foreach (var member in _teamMembers[team])
                {
                    SendTeamSetMessage(clientId, member, team);
                }
            }

            var joinedTeam = Team.None;

            if (GetPlayerCount(Team.Defender) >= GetPlayerCount(Team.Attacker))
            {
                joinedTeam = Team.Attacker;
                _teamMembers[joinedTeam].Add(clientId);
                playerComp.Team = joinedTeam;
            }
            else
            {
                joinedTeam = Team.Defender;
                _teamMembers[joinedTeam].Add(clientId);
                playerComp.Team = joinedTeam;
            }

            SendTeamSetMessage(clientId, joinedTeam);
        }

        private void EventHandler_ClientDisconnected(ushort clientId)
        {
            //Inform clients about player leaving
            PlayerLeft(clientId);
        }

        private void PlayerLeft(ushort playerId)
        {
            var team = GetTeam(playerId);
            RemoveFromAllTeams(playerId);
            SendTeamSetMessage(playerId, Team.None);

            if (GetPlayerCount(team) > 0)
            {
                return;
            }

            var otherTeam = team == Team.Attacker ? Team.Defender : Team.Attacker;

            if ((team == Team.Attacker && GetPlayerCount(otherTeam) > 1) ||
                (team == Team.Defender && GetPlayerCount(otherTeam) > 1))
            {
                var randomPlayerToMove = _teamMembers[otherTeam][Random.Range(0, _teamMembers[otherTeam].Count)];
                RemoveFromAllTeams(randomPlayerToMove);
                SetTeam(randomPlayerToMove, team);
            }
        }

        private void RemoveFromAllTeams(ushort playerId)
        {
            foreach(var team in _teamMembers.Keys)
            {
                if (_teamMembers[team].Remove(playerId))
                {
                    var player = PlayerManager.Instance.GetPlayer(playerId);
                    if (player)
                    {
                        player.Team = Team.None;
                    }
                }
            }
        }

        public Team GetTeam(ushort clientId)
        {
            foreach (var team in _teamMembers.Keys)
            {
                if (_teamMembers[team].Contains(clientId))
                {
                    return team;
                }
            }

            return Team.None;
        }

        private void SendTeamSetMessage(ushort receiver, ushort client, Team team)
        {
            Debug.Log("SEND MESSAGE TO RECEIVER");
            NetworkManager.Instance.Server.Send(TeamSetMessage(client, team), receiver);
        }

        private void SendTeamSetMessage(ushort clientId, Team team)
        {
            Debug.Log("SEND MESSAGE TO EVERYONE");
            NetworkManager.Instance.Server.SendToAll(TeamSetMessage(clientId, team));
        }

        private Message TeamSetMessage(ushort id, Team team)
        {
            var message = Message.Create(MessageSendMode.Reliable, (ushort)Enums.ServerToClientMessages.TeamSet);
            message.AddUShort(id);
            message.AddUShort((ushort)team);
            return message;
        }

        private void SetTeam(ushort playerId, Team team)
        {
            _teamMembers[team].Add(playerId);
            var player = PlayerManager.Instance.GetPlayer(playerId);
            if (!player)
            {
                return;
            }
            player.Team = team;
            SendTeamSetMessage(playerId, team);
        }

        private int GetPlayerCount(Team team)
        {
            return _teamMembers[team]?.Count ?? 0;
        }

        [MessageHandler((ushort)ClientToServerMessages.SwitchTeamRequest)]
        private static void SwitchTeamRequest(ushort senderId, Message message)
        {
            Instance.HandleSwitchRequest(senderId);
        }

        private void HandleSwitchRequest(ushort playerId)
        {
            var team = GetTeam(playerId);

            switch (team)
            {
                case Team.Attacker:
                case Team.Defender:
                    TrySwitchTo(playerId, team);
                    break;
                case Team.None:
                    AddPlayer(playerId);
                    break;
            }
        }

        private void TrySwitchTo(ushort playerId, Team currentTeam)
        {
            if (currentTeam == Team.Attacker && GetPlayerCount(Team.Attacker) > 1)
            {
                RemoveFromAllTeams(playerId);
                SetTeam(playerId, Team.Defender);
            }
            else if (currentTeam == Team.Defender && GetPlayerCount(Team.Defender) > 1)
            {
                RemoveFromAllTeams(playerId);
                SetTeam(playerId, Team.Attacker);
            }
        }
    }

}