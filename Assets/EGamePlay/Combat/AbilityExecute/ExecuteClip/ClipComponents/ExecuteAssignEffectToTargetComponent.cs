namespace EGamePlay.Combat
{
    /// <summary>
    /// 执行体应用目标效果组件
    /// </summary>
    public class ExecuteAssignEffectToTargetComponent : Component
    {
        public override bool DefaultEnable { get; set; } = false;
        public ExecuteTriggerType ExecuteTriggerType { get; set; }
        public override void Awake()
        {
            Entity.Subscribe<ExecuteEffectEvent>(OnTriggerExecuteEffect);
        }
        private void OnTriggerExecuteEffect(ExecuteEffectEvent evnt)
        {
            var skillExecution = Entity.GetParent<AbilityExecution>();
            if (skillExecution.InputTarget == null) return;
            var abilityTriggerComp = skillExecution.AbilityEntity.GetComponent<AbilityTriggerComponent>();
            var effects = abilityTriggerComp.AbilityTriggers;
            for (var i = 0; i < effects.Count; i++)
            {
                if (i != (int)ExecuteTriggerType - 1 && ExecuteTriggerType != ExecuteTriggerType.AllTriggers) continue;
                var effect = effects[i];
                effect.OnTrigger(new TriggerContext
                {
                    AbilityTrigger = effect,
                    TriggerSource = skillExecution,
                    Target = skillExecution.InputTarget,
                });
            }
        }
    }
}