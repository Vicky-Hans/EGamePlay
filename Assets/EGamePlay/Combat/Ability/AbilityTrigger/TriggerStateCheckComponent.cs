using System.Collections.Generic;

namespace EGamePlay.Combat
{
    /// <summary>
    /// 状态判断组件
    /// </summary>
    public class TriggerStateCheckComponent : Component
    {
        public override bool DefaultEnable { get; set; } = false;
        private List<IStateCheck> StateChecks { get; set; } = new ();
        public override void Awake()
        {
            var effectConfig = Entity.As<AbilityTrigger>().TriggerConfig;
            if (effectConfig.StateCheckList == null)  return;
            foreach (var item in effectConfig.StateCheckList)
            {
                var conditionStr = item;
                if (string.IsNullOrEmpty(conditionStr)) continue;
                if (conditionStr.StartsWith("#")) continue;
                var condition = conditionStr;
                if (conditionStr.StartsWith("!")) condition = conditionStr.TrimStart('!');
                var arr2 = condition.Split('<', '=', '≤');
                var conditionType = arr2[0];
                var scriptType = $"EGamePlay.Combat.StateCheck_{conditionType}Check";
                var typeClass = System.Type.GetType(scriptType);
                if (typeClass != null)
                {
                    StateChecks.Add(Entity.AddChild(typeClass, conditionStr) as IStateCheck);
                }
                else
                {
                    Log.Error($"Condition class not found: {scriptType}");
                }
            }
        }
        protected override void OnDestroy()
        {
            foreach (var item in StateChecks)
                Entity.Destroy(item as Entity);
            StateChecks.Clear();
        }
        protected override void OnEnable() { }
        protected override void OnDisable() { }
        public bool CheckTargetState(Entity target)
        {
            //这里是状态判断，状态判断是判断目标的状态是否满足条件，满足则触发效果
            var conditionCheckResult = true;
            foreach (var item in StateChecks)
            {
                //条件取反
                if (item.IsInvert)
                {
                    if (!item.CheckWith(target)) continue;
                    conditionCheckResult = false;
                    break;
                }
                if (item.CheckWith(target)) continue;
                conditionCheckResult = false;
                break;
            }
            return conditionCheckResult;
        }
    }
}