namespace EGamePlay.Combat
{
    public class MotionActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }
        public bool Enable { get; set; }
        public bool TryMakeAction(out MotionAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChild<MotionAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }
    /// <summary>
    /// 动作行动
    /// </summary>
    public class MotionAction : Entity, IActionExecute
    {
        public int MotionType { get; set; }
        public Entity ActionAbility { get; set; }//行动能力
        public EffectAssignAction SourceAssignAction { get; set; }//效果赋给行动源
        public CombatEntity Creator { get; set; }//行动实体
        public Entity Target { get; set; }//目标对象
        public void FinishAction()
        {
            Destroy(this);
        }
        //前置处理
        private void PreProcess() { }
        public void ApplyMotion()
        {
            PreProcess();
            PostProcess();
        }
        //后置处理
        private void PostProcess() { }
    }
}