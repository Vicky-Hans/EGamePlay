using GameUtils;
namespace EGamePlay.Combat
{
    public class ExecuteTriggerNewExecutionComponent : Component
    {
        public ActionEventData ActionEventData { get; set; }
        public override void Awake()
        {
            Entity.Subscribe<ExecuteEffectEvent>(OnTriggerExecutionEffect);
        }
        private void OnTriggerExecutionEffect(ExecuteEffectEvent evnt)
        {
            var executionObject = AssetUtils.LoadObject<ExecutionObject>($"{AbilityManagerObject.ExecutionResFolder}/" + ActionEventData.NewExecution);
            if (executionObject == null) return;
            var sourceExecution = Entity.GetParent<AbilityExecution>();
            var execution = sourceExecution.OwnerEntity.AddChild<AbilityExecution>(sourceExecution.SkillAbility);
            execution.ExecutionObject = executionObject;
            execution.InputTarget = sourceExecution.InputTarget;
            execution.InputPoint = sourceExecution.InputPoint;
            execution.LoadExecutionEffects();
            execution.BeginExecute();
            if (executionObject != null) execution.AddComponent<UpdateComponent>();
        }
    }
}