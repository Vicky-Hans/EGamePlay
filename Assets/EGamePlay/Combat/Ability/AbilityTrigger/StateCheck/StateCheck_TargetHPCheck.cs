﻿using System;
using UnityEngine;

namespace EGamePlay.Combat
{
    /// <summary>
    /// 判断目标当前生命值是否满足条件
    /// </summary>
    public class StateCheck_TargetHPCheck : Entity, IStateCheck
    {
        public CombatEntity OwnerBattler => Parent.GetParent<AbilityEffect>().OwnerEntity;
        public string AffectCheck { get; set; }
        public bool IsInvert => AffectCheck.StartsWith("!");

        protected override void Awake(object initData)
        {
            AffectCheck = initData.ToString().ToLower();
        }

        public bool CheckWith(Entity target)
        {
            if (target is IActionExecute combatAction)  target = combatAction.Target;
            if (target is CombatEntity battler)
            {
                var arr = AffectCheck.Split('<', '=', '≤');
                var formula = arr[1];
                formula = formula.Replace("%", $"*0.01");
                formula = formula.Replace("TargetHPMax".ToLower(), $"{battler.GetComponent<AttributeComponent>().HealthPointMax.Value}");
                var value = ExpressionHelper.ExpressionParser.Evaluate(formula);
                var targetHp = battler.GetComponent<AttributeComponent>().HealthPoint.Value;
                if (AffectCheck.Contains("<") || AffectCheck.Contains("≤")) return targetHp <= value;
                if (AffectCheck.Contains("=")) return Math.Abs(targetHp - value) < Mathf.Epsilon;
            }
            Log.Debug($"ConditionTargetHPCheck CheckCondition {AffectCheck} false");
            return false;
        }
    }
}
