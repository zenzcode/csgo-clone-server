using Helper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class RoundManager : SingletonMonoBehavior<RoundManager>
    {
        private List<ushort> _playersDiedThisRound;

        protected override void Awake()
        {
            base.Awake();
            _playersDiedThisRound = new List<ushort>();
        }

        public void NewRoundStarted()
        {
            TimerManager.Instance.StartServerTimer(Enums.Timer.WarmupTimer);
        }
    }

}