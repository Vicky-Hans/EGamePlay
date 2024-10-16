﻿using System.Collections.Generic;
namespace EGamePlay.Combat
{
    /// <summary>
    /// 能力效果组件，一个能力可以包含多个效果
    /// </summary>
    public class AbilityEffectComponent : Component
    {
        public override bool DefaultEnable { get; set; } = false;
        public List<AbilityEffect> AbilityEffects { get; private set; } = new ();
        public AbilityEffect DamageAbilityEffect { get; set; }
        public AbilityEffect CureAbilityEffect { get; set; }
        public override void Awake(object initData)
        {
            if (initData == null) return;
            var effects = initData as List<Effect>;
            foreach (var item in effects)
            {
                var abilityEffect = Entity.AddChild<AbilityEffect>(item);
                AddEffect(abilityEffect);

                if (abilityEffect.EffectConfig is DamageEffect)
                {
                    DamageAbilityEffect = abilityEffect;
                }
                if (abilityEffect.EffectConfig is CureEffect)
                {
                    CureAbilityEffect = abilityEffect;
                }
            }
        }
        protected override void OnEnable()
        {
            foreach (var item in AbilityEffects)
                item.EnableEffect();
        }
        protected override void OnDisable()
        {
            foreach (var item in AbilityEffects)
                item.DisableEffect();
        }
        private void AddEffect(AbilityEffect abilityEffect)
        {
            AbilityEffects.Add(abilityEffect);
        }
        public AbilityEffect GetEffect(int index = 0)
        {
            return AbilityEffects[index];
        }
    }
}