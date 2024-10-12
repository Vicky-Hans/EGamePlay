using UnityEngine;
#if EGAMEPLAY_ET
using Unity.Mathematics;
#endif

#if UNITY
namespace EGamePlay.Combat
{
    public class ExecuteParticleEffectComponent : Component
    {
        public GameObject ParticleEffectPrefab { get; set; }
        private GameObject ParticleEffectObj { get; set; }
        public override void Awake()
        {
            Entity.OnEvent(nameof(ExecuteClip.TriggerEffect), OnTriggerStart);
            Entity.OnEvent(nameof(ExecuteClip.EndEffect), OnTriggerEnd);
        }

        private void OnTriggerStart(Entity entity)
        {
#if EGAMEPLAY_ET
            ParticleEffectObj = GameObject.Instantiate(ParticleEffectPrefab, Entity.GetParent<SkillExecution>().OwnerEntity.Position, Entity.GetParent<SkillExecution>().OwnerEntity.Rotation);
#else
            ParticleEffectObj = Object.Instantiate(ParticleEffectPrefab, Entity.GetParent<AbilityExecution>().OwnerEntity.Position, Entity.GetParent<AbilityExecution>().OwnerEntity.Rotation);
#endif
        }
        private void OnTriggerEnd(Entity entity)
        {
            Object.Destroy(ParticleEffectObj);
        }
    }
}
#endif