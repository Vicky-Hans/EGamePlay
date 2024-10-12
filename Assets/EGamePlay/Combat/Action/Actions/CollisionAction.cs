namespace EGamePlay.Combat
{
    public class CollisionActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }
        public bool Enable { get; set; }
        public bool TryMakeAction(out CollisionAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChild<CollisionAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }

    /// <summary>
    /// 碰撞行动
    /// </summary>
    public class CollisionAction : Entity, IActionExecute
    {
        public Entity ActionAbility { get; set; }//行动能力
        public EffectAssignAction SourceAssignAction { get; set; }//效果赋给行动源
        public CombatEntity Creator { get; set; }//行动实体
        public Entity Target { get; set; }//目标对象
        public AbilityItem CauseItem { get; set; }
        private void FinishAction()
        {
            Destroy(this);
        }
        private void PreProcess() { }//前置处理
        public void ApplyCollision()
        {
            PreProcess();
            if (Target != null)
            {
                if (Target is CombatEntity combatEntity)
                {
                    CauseItem.OnTriggerEvent(combatEntity);
                }
                if (Target is AbilityItem abilityItem)
                {
                    var causeCollisionComp = CauseItem.GetComponent<AbilityItemCollisionExecuteComponent>();
                    if (Target.GetComponent<AbilityItemShieldComponent>() != null)
                    {
#if !EGAMEPLAY_ET
                        if (CauseItem.OwnerEntity.IsHero != abilityItem.OwnerEntity.IsHero)
#endif
                        {
                            CauseItem.OnTriggerEvent(Target);

                            if (causeCollisionComp.CollisionExecuteData.ActionData.FireType == FireType.CollisionTrigger)
                            {
#if EGAMEPLAY_ET
                                var itemUnit = CauseItem.GetComponent<CombatUnitComponent>().Unit as ItemUnit;
                                itemUnit.DestroyType = UnitDestroyType.DestroyWithExplosion;
#endif
                                CauseItem.GetComponent<HealthPointComponent>().SetDie();
                            }
                        }
                    }
                }
            }
            PostProcess();
            FinishAction();
        }
        //后置处理
        private void PostProcess() { }
    }
}