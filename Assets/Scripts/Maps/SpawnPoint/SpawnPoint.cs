using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maps.SpawnPoint
{
    public class SpawnPoint : MonoBehaviour
    {
        private bool _isOccupied;
        [SerializeField] private Team _team;

        public bool IsOccupied => _isOccupied;
        public Team Team => _team;

        public void SetOccupied(bool occupied)
        {
            _isOccupied = occupied;
        }
    }

}