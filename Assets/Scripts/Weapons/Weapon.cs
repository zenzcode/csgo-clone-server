using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapons;

namespace Weapons
{
    public class Weapon : MonoBehaviour
    {
        #region Vars
        [SerializeField] private WeaponSO WeaponSO;
        public GameObject GrabPointRight;
        public GameObject GrabPointLeft;
        public int Ammo { get; private set; }
        public int ReserveAmmo { get; private set; }
        #endregion

        private void Awake()
        {
            Ammo = WeaponSO.StartAmmo;
            ReserveAmmo = WeaponSO.ReserveAmmo;
        }

        //returns true if reload succeeded and false if there is not enough ammo left
        public bool TryReload()
        {
            if (ReserveAmmo <= 0)
            {
                return false;
            }

            int actualReloadAmount = 0;

            //there is still ammo in the clip
            if (Ammo >= 0)
            {
                actualReloadAmount = Mathf.Min(MagazineCapacity - Ammo, ReserveAmmo);
            }
            else
            {
                actualReloadAmount = Mathf.Min(MagazineCapacity, ReserveAmmo);
            }

            ReserveAmmo -= actualReloadAmount;
            Ammo += actualReloadAmount;

            //TODO: send to server as update with tick, to tell client reloading finished

            return true;
        }

        public string DisplayName => WeaponSO.DisplayName;

        public int StartAmmo => WeaponSO.StartAmmo;

        public int MagazineCapacity => WeaponSO.MagazineCapacity;

        public AnimationCurve RecoilCurve => WeaponSO.Recoil;
    }
}