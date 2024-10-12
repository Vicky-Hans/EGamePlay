using UnityEngine;
using GameUtils;

namespace EGamePlay
{
    //生命周期组件
    public class LifeTimeComponent : Component
    {
        public override bool DefaultEnable { get; set; } = true;
        private GameTimer LifeTimer { get; set; }
        public override void Awake(object initData)
        {
            LifeTimer = new GameTimer((float)initData);
        }
        public override void Update()
        {
            if (LifeTimer.IsRunning)
            {
                LifeTimer.UpdateAsFinish(Time.deltaTime, DestroyEntity);
            }
        }
        private void DestroyEntity()
        {
            Entity.Destroy(Entity);
        }
    }
}