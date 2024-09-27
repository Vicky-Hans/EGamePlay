using System.Collections.Generic;
namespace EGamePlay.Combat
{
    public class AbilityComponent : Component
    {
        private Dictionary<long, Ability> IdAbilities { get; set; } = new ();

        /// <summary>
        /// 挂载能力，技能、被动、buff等都通过这个接口挂载
        /// </summary>
        public Ability AttachAbility(object configObject)
        {
            var ability = Entity.AddChild<Ability>(configObject);
            ability.AddComponent<AbilityLevelComponent>();
            IdAbilities.Add(ability.Id, ability);
            return ability;
        }
        public void RemoveAbility(Ability ability)
        {
            IdAbilities.Remove(ability.Id);
            ability.EndAbility();
        }
    }
}
