using UnityEngine;
#if UNITY
namespace EGamePlay.Combat
{
    public class ExecuteAnimationComponent : Component
    {
        public AnimationClip AnimationClip { get; set; }
        public override void Awake()
        {
            Entity.OnEvent(nameof(ExecuteClip.TriggerEffect), OnTriggerExecutionEffect);
        }
        private void OnTriggerExecutionEffect(Entity entity)
        {
            Entity.GetParent<AbilityExecution>().OwnerEntity.Publish(AnimationClip);
        }
    }
}
#endif