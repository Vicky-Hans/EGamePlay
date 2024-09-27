﻿using GameUtils;
using System;
using UnityEngine;

namespace EGamePlay.Combat
{
    public sealed class TimeState_TimeIntervalObserveComponent : Component
    {
        public override bool DefaultEnable => false;
        private GameTimer IntervalTimer { get; set; }
        private Action WhenTimeIntervalAction { get; set; }
        public override void Awake(object initData)
        {
            var time = (float)initData;
            IntervalTimer = new GameTimer(time);
        }
        public void StartListen(Action whenTimeIntervalAction)
        {
            IntervalTimer.OnRepeat(OnRepeat);
            Enable = true;
        }
        private void OnRepeat()
        {
            GetEntity<AbilityTrigger>().OnTrigger(new TriggerContext());
        }
        public override void Update()
        {
            if (IntervalTimer.IsRunning) IntervalTimer.UpdateAsRepeat(Time.deltaTime);
        }
    }
}