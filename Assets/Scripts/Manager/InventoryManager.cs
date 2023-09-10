using Helper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class InventoryManager : SingletonMonoBehavior<InventoryManager>
    {
        private Dictionary<ushort, PlayerInventory.PlayerInventory> _playerItems;

        protected override void Awake()
        {
            base.Awake();
            _playerItems = new Dictionary<ushort, PlayerInventory.PlayerInventory>();
        }
    }
}