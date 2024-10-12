using System;
namespace EGamePlay.Combat
{
    public class ActionComponent : Component
    {
        private Type actionType;
        public override void Awake(object initData)
        {
            actionType = initData as Type;
        }
    }
}