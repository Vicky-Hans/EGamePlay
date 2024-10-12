namespace EGamePlay.Combat
{
    public class EffectAssignAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }
        public bool Enable { get; set; }
        public bool TryMakeAction(out EffectAssignAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChild<EffectAssignAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }
    public class EffectAssignAction : Entity, IActionExecute
    {
        public Entity SourceAbility { get; set; }
        public IActionExecute TargetAction { get; set; }
        public AbilityEffect AbilityEffect { get; set; }
        public Effect EffectConfig => AbilityEffect.EffectConfig;
        public Entity ActionAbility { get; set; }
        public EffectAssignAction SourceAssignAction { get; set; }
        public CombatEntity Creator { get; set; }
        public Entity Target { get; set; }
        public Entity AssignTarget { get; set; }
        public TriggerContext TriggerContext { get; set; }
        private void PreProcess()
        {
            if (Target != null) return;
            Target = AssignTarget;
            if (AssignTarget is IActionExecute actionExecute) Target = actionExecute.Target;
            if (AssignTarget is AbilityExecution skillExecution) Target = skillExecution.InputTarget;
        }
        public void AssignEffect()
        {
            PreProcess();
            foreach (var item in AbilityEffect.Components.Values)
            {
                if (item is IEffectTriggerSystem effectTriggerSystem)
                {
                    effectTriggerSystem.OnTriggerApplyEffect(this);
                }
            }
            PostProcess();
            FinishAction();
        }
        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.AssignEffect, this);
            if (!Target.IsDisposed)
            {
                Target.GetComponent<ActionPointComponent>().TriggerActionPoint(ActionPointType.ReceiveEffect, this);
            }
            var decorators = AbilityEffect.EffectConfig.Decorators;
            if (decorators == null) return;
            foreach (var item in decorators)
            {
                if (item is not TriggerNewEffectWhenAssignEffectDecorator effectDecorator) continue;
                var abilityTriggerComp = AbilityEffect.OwnerAbility.GetComponent<AbilityTriggerComponent>();
                var effects = abilityTriggerComp.AbilityTriggers;
                var executeTriggerType = effectDecorator.ExecuteTriggerType;
                for (var i = 0; i < effects.Count; i++)
                {
                    if (i != (int)executeTriggerType - 1 && executeTriggerType != ExecuteTriggerType.AllTriggers) continue;
                    var effect = effects[i];
                    effect.OnTrigger(new TriggerContext() { Target = Target });
                }
            }
        }
        private void FinishAction()
        {
            Destroy(this);
        }
    }
}