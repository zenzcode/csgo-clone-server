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
            DontDestroyOnLoad(this);
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
            RemoveFromAllTeams(clientId);
            SendTeamSetMessage(clientId, Team.None);
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

        private int GetPlayerCount(Team team)
        {
            return _teamMembers[team]?.Count ?? 0;
        }
    }

}