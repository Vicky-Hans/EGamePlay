namespace EGamePlay.Combat
{
    public class CureActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }
        public bool Enable { get; set; }
        public bool TryMakeAction(out CureAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChild<CureAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }
    /// <summary>
    /// 治疗行动
    /// </summary>
    public class CureAction : Entity, IActionExecute
    {
        public CureEffect CureEffect => SourceAssignAction.AbilityEffect.EffectConfig as CureEffect;
        public int CureValue { get; set; }//治疗数值
        public Entity ActionAbility { get; set; }//行动能力
        public EffectAssignAction SourceAssignAction { get; set; }//效果赋给行动源
        public CombatEntity Creator { get; set; }//行动实体
        public Entity Target { get; set; }//目标对象
        private void FinishAction()
        {
            Destroy(this);
        }
        //前置处理
        private void PreProcess()
        {
            if (SourceAssignAction is not { AbilityEffect: not null }) return;
            CureValue = SourceAssignAction.AbilityEffect.GetComponent<EffectCureComponent>().GetCureValue();
            var healthComp = Target.GetComponent<HealthPointComponent>();
            if (CureValue + healthComp.Value > healthComp.MaxValue)
            {
                CureValue = healthComp.MaxValue - healthComp.Value;
            }
        }
        public void ApplyCure()
        {
            PreProcess();
            var healthComp = Target.GetComponent<HealthPointComponent>();
            if (healthComp.IsFull() == false) healthComp.ReceiveCure(this);
            PostProcess();
            FinishAction();
        }
        //后置处理
        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PostGiveCure, this);
            Target.GetComponent<ActionPointComponent>().TriggerActionPoint(ActionPointType.PostReceiveCure, this);
        }
    }
}