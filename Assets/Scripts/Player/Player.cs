using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Player
{
    public class Player : MonoBehaviour
    {
        [HideInInspector] public ushort PlayerId;
        [HideInInspector] public string Username;
        [HideInInspector] public Team Team = Enums.Team.None;
        [HideInInspector] public bool IsLeader = false;
        [HideInInspector] public float LastKnownRtt = 1;
    }
}
