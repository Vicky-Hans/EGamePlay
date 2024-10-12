﻿using ET;
namespace EGamePlay.Combat
{
    public class RoundActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }
        public bool Enable { get; set; }
        public bool TryMakeAction(out RoundAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChild<RoundAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }
    /// <summary>
    /// 回合行动
    /// </summary>
    public class RoundAction : Entity, IActionExecute
    {
        public int RoundActionType { get; set; }
        public Entity ActionAbility { get; set; }//行动能力
        public EffectAssignAction SourceAssignAction { get; set; }//效果赋给行动源
        public CombatEntity Creator { get; set; }//行动实体
        /// 目标对象
        public Entity Target { get; set; }
        public void FinishAction()
        {
            Destroy(this);
        }
        //前置处理
        private void PreProcess() { }
        public async ETTask ApplyRound()
        {
            PreProcess();
            if (Creator.JumpToAbility.TryMakeAction(out var jumpToAction))
            {
                jumpToAction.Target = Target;
                await jumpToAction.ApplyJumpTo();
            }
            PostProcess();
        }
        //后置处理
        private void PostProcess() { }
    }
}