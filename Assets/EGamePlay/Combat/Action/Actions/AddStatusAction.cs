using System.Collections.Generic;
using ET;
using GameUtils;

namespace EGamePlay.Combat
{
    public class AddStatusActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }
        public bool Enable { get; set; }
        public bool TryMakeAction(out AddStatusAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChild<AddStatusAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }
    public class AddStatusAction : Entity, IActionExecute
    {
        public Entity SourceAbility { get; set; }
        public AddStatusEffect AddStatusEffect => SourceAssignAction.AbilityEffect.EffectConfig as AddStatusEffect;
        public Ability BuffAbility { get; set; }
        public Entity ActionAbility { get; set; }
        public EffectAssignAction SourceAssignAction { get; set; }
        public CombatEntity Creator { get; set; }
        public Entity Target { get; set; }
        private void FinishAction()
        {
            Destroy(this);
        }
        private void PreProcess() { }
        public void ApplyAddStatus()
        {
            PreProcess();
            var buffObject = AddStatusEffect.AddStatus;
            if (buffObject == null)
            {
                var statusId = AddStatusEffect.AddStatusId;
                buffObject = AssetUtils.LoadObject<AbilityConfigObject>($"{AbilityManagerObject.BuffResFolder}/Buff_{statusId}");
            }
            var buffConfig = AbilityConfigCategory.Instance.Get(buffObject.Id);
            var canStack = buffConfig.CanStack == "��";
            var statusComp = Target.GetComponent<StatusComponent>();
            if (canStack == false)
            {
                if (statusComp.HasStatus(buffConfig.KeyName))
                {
                    var status = statusComp.GetStatus(buffConfig.KeyName);
                    var lifeComp = status.GetComponent<AbilityLifeTimeComponent>();
                    if (lifeComp != null)
                    {
                        var statusLifeTimer = lifeComp.LifeTimer;
                        statusLifeTimer.MaxTime = AddStatusEffect.Duration;
                        statusLifeTimer.Reset();
                    }
                    FinishAction();
                    return;
                }
            }
            BuffAbility = statusComp.AttachStatus(buffObject);
            BuffAbility.OwnerEntity = Creator;
            BuffAbility.GetComponent<AbilityLevelComponent>().Level = SourceAbility.GetComponent<AbilityLevelComponent>().Level;
            ProcessInputKvParams(BuffAbility, AddStatusEffect.Params);
            if (AddStatusEffect.Duration > 0)
            {
                BuffAbility.AddComponent<AbilityLifeTimeComponent>(AddStatusEffect.Duration);
            }
            BuffAbility.TryActivateAbility();
            PostProcess();
            FinishAction();
        }
        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PostGiveStatus, this);
            Target.GetComponent<ActionPointComponent>().TriggerActionPoint(ActionPointType.PostReceiveStatus, this);
        }
        private void ProcessInputKvParams(Ability ability, Dictionary<string, string> @params)
        {
            foreach (var abilityTrigger in ability.GetComponent<AbilityTriggerComponent>().AbilityTriggers)
            {
                var effect = abilityTrigger.TriggerConfig;
                if (!string.IsNullOrEmpty(effect.ConditionParam))
                {
                    abilityTrigger.ConditionParamValue = ProcessReplaceKv(effect.ConditionParam, @params);
                }
            }

            foreach (var abilityEffect in ability.GetComponent<AbilityEffectComponent>().AbilityEffects)
            {
                var effect = abilityEffect.EffectConfig;

                if (effect is AttributeModifyEffect attributeModify && abilityEffect.TryGet(out EffectAttributeModifyComponent attributeModifyComponent))
                {
                    attributeModifyComponent.ModifyValueFormula = ProcessReplaceKv(attributeModify.NumericValue, @params);
                }
                if (effect is DamageEffect damage && abilityEffect.TryGet(out EffectDamageComponent damageComponent))
                {
                    damageComponent.DamageValueFormula = ProcessReplaceKv(damage.DamageValueFormula, @params);
                }
                if (effect is CureEffect cure && abilityEffect.TryGet(out EffectCureComponent cureComponent))
                {
                    cureComponent.CureValueProperty = ProcessReplaceKv(cure.CureValueFormula, @params);
                }
            }
        }
        private string ProcessReplaceKv(string originValue, Dictionary<string, string> @params)
        {
            foreach (var aInputKvItem in @params)
            {
                if (!string.IsNullOrEmpty(originValue))
                {
                    originValue = originValue.Replace(aInputKvItem.Key, aInputKvItem.Value);
                }
            }
            return originValue;
        }
    }
}