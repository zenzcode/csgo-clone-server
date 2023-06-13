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
                    var availableSpawnPoints = AttackerSpawnPoints.Where(spawnpoint => !spawnpoint.IsOccupied).ToList();
                    var spawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count - 1)];
                    var spawnPos = spawnPoint.gameObject.transform.position;
                    player.gameObject.transform.position = spawnPos;
                    spawnPoint.SetOccupied(true);
                }
                else if(player.Team == Enums.Team.Defender)
                {
                    var availableSpawnPoints = DefenderSpawnPoints.Where(spawnpoint => !spawnpoint.IsOccupied).ToList();
                    var spawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count - 1)];
                    var spawnPos = spawnPoint.gameObject.transform.position;
                    player.gameObject.transform.position = spawnPos;
                    spawnPoint.SetOccupied(true);
                }

                Instantiate(AssetManager.Instance.GamePlayer, player.transform);
            }
        }
    }

}