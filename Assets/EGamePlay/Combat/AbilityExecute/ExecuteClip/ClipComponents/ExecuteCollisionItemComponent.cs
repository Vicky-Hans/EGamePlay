namespace EGamePlay.Combat
{
    public class ExecuteCollisionItemComponent : Component
    {
        public ItemExecute CollisionExecuteData { get; set; }
        public override void Awake()
        {
            Entity.OnEvent(nameof(ExecuteClip.TriggerEffect), OnTriggerExecutionEffect);
            Entity.OnEvent(nameof(ExecuteClip.EndEffect), OnTriggerEnd);
        }
        private void OnTriggerExecutionEffect(Entity entity)
        {
            SpawnCollisionItem(GetEntity<ExecuteClip>().ExecutionEffectConfig);
        }
        //技能碰撞体生成
        private void SpawnCollisionItem(ExecuteClipData clipData)
        {
            var skillExecution = Entity.GetParent<AbilityExecution>();
            var abilityItem = Entity.Create<AbilityItem>(skillExecution);
            abilityItem.AddComponent<AbilityItemCollisionExecuteComponent>(clipData);

            var moveType = clipData.ItemData.MoveType;
            if (moveType == CollisionMoveType.PathFly) abilityItem.PathFlyProcess(skillExecution.InputPoint);
            if (moveType == CollisionMoveType.SelectedDirectionPathFly) abilityItem.DirectionPathFlyProcess(skillExecution.InputPoint, skillExecution.InputRadian);
            if (moveType == CollisionMoveType.TargetFly) abilityItem.TargetFlyProcess(skillExecution.InputTarget);
            if (moveType == CollisionMoveType.ForwardFly) abilityItem.ForwardFlyProcess(skillExecution.InputRadian);
            if (moveType == CollisionMoveType.SelectedPosition) abilityItem.SelectedPositionProcess(skillExecution.InputPoint);
            if (moveType == CollisionMoveType.FixedPosition) abilityItem.FixedPositionProcess();
            if (moveType == CollisionMoveType.SelectedDirection) abilityItem.SelectedDirectionProcess();
#if EGAMEPLAY_ET
            abilityItem.AddCollisionComponent();
#else
#if UNITY
            abilityItem.CreateAbilityItemProxyObj();
#endif
#endif
        }
        private void OnTriggerEnd(Entity entity) { }
    }
}