using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using Helper;


namespace Manager
{
    public class GameManager : SingletonMonoBehavior<GameManager>
    {
        public GameState State { get; private set; } = GameState.Lobby;
    }

}
