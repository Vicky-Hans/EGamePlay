namespace EGamePlay.Combat
{
    /// <summary>
    /// 伤害吸血组件
    /// </summary>
    public class DamageBloodSuckComponent : Component
    {
        public override void Awake()
        {
            var combatEntity = Entity.GetParent<CombatEntity>();
            combatEntity.ListenActionPoint(ActionPointType.PostCauseDamage, OnCauseDamage);
        }
        private void OnCauseDamage(Entity action)
        {
            if (action is not DamageAction { Target: CombatEntity } damageAction) return;
            var value = damageAction.DamageValue * 0.2f;
            var combatEntity = Entity.GetParent<CombatEntity>();
            if (!combatEntity.CureAbility.TryMakeAction(out var cureAction)) return;
            cureAction.Creator = combatEntity;
            cureAction.Target = combatEntity;
            cureAction.CureValue = (int)value;
            cureAction.SourceAssignAction = null;
            cureAction.ApplyCure();
        }
    }
}