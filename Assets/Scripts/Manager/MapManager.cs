using Assets;
using Helper;
using Maps.SpawnPoint;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Managers
{
    public class MapManager : SingletonMonoBehavior<MapManager>
    {
        public List<SpawnPoint> AttackerSpawnPoints;
        public List<SpawnPoint> DefenderSpawnPoints;

        private void Start()
        {
            foreach(var player in PlayerManager.Instance.Players.Values)
            {
                if(player.Team == Enums.Team.Attacker)
                {
                    var spawnpoint = AttackerSpawnPoints.FirstOrDefault(spawnpoint => !spawnpoint.IsOccupied);
                    var spawnPos = spawnpoint.gameObject.transform.position + new Vector3(0f, player.PlayerHeight, 0f);
                    player.gameObject.transform.position = spawnPos;
                }
                else if(player.Team == Enums.Team.Defender)
                {
                    var spawnpoint = DefenderSpawnPoints.FirstOrDefault(spawnpoint => !spawnpoint.IsOccupied);
                    var spawnPos = spawnpoint.gameObject.transform.position + new Vector3(0f, player.PlayerHeight, 0f);
                    player.gameObject.transform.position = spawnPos;
                }

                Instantiate(AssetManager.Instance.GamePlayer, player.transform);
            }
        }
    }

}