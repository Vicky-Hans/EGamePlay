using ET;

namespace EGamePlay.Combat
{
    public class AttackActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }
        public bool Enable { get; set; }
        public bool TryMakeAction(out AttackAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChild<AttackAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }
    /// <summary>
    /// 普攻行动
    /// </summary>
    public class AttackAction : Entity, IActionExecute
    {
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
            Creator.TriggerActionPoint(ActionPointType.PreGiveAttack, this);
            Target.GetComponent<ActionPointComponent>().TriggerActionPoint(ActionPointType.PreReceiveAttack, this);
        }
        public async ETTask ApplyAttackAwait()
        {
            PreProcess();
            await TimeHelper.WaitAsync(1000);
            ApplyAttack();
            await TimeHelper.WaitAsync(300);
            PostProcess();
            FinishAction();
        }
        public void ApplyAttack() { }
        //后置处理
        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PostGiveAttack, this);
            Target.GetComponent<ActionPointComponent>().TriggerActionPoint(ActionPointType.PostReceiveAttack, this);
        }
    }
}