using Maps.SpawnPoint;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maps
{
    [CreateAssetMenu(fileName = "MapName_SO", menuName = "Scriptable Objects/Maps")]
    public class MapSO : ScriptableObject
    {
        [Header("General Information")]
        public string Name;
        public string PathToMap;
        [Space(10)]
        [Header("Settings")]
        public int RoundLengthMinutes = 5;
        public int WarmupTimeSeconds = 20;
    }

}
