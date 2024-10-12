using UnityEngine;
using GameUtils;
namespace EGamePlay.Combat
{
    /// <summary>
    /// 时间触发组件
    /// </summary>
    public class ExecuteTimeTriggerComponent : Component
    {
        public override bool DefaultEnable { get; set; } = false;
        public float StartTime { get; set; }
        public float EndTime { get; set; }
        public string TimeValueExpression { get; set; }
        private GameTimer StartTimer { get; set; }
        private GameTimer EndTimer { get; set; }
        public override void Update()
        {
            if (StartTimer is { IsFinished: false })
            {
                StartTimer.UpdateAsFinish(Time.deltaTime, GetEntity<ExecuteClip>().TriggerEffect);
            }
            if (EndTimer is { IsFinished: false })
            {
                EndTimer.UpdateAsFinish(Time.deltaTime, GetEntity<ExecuteClip>().EndEffect);
            }
        }
        protected override void OnEnable()
        {
            if (!string.IsNullOrEmpty(TimeValueExpression))
            {
                var expression = ExpressionHelper.TryEvaluate(TimeValueExpression);
                StartTime = (int)expression.Value / 1000f;
                StartTimer = new GameTimer(StartTime);
            }
            else if (StartTime > 0)
            {
                StartTimer = new GameTimer(StartTime);
            }
            else
            {
                GetEntity<ExecuteClip>().TriggerEffect();
            }
            if (EndTime > 0) EndTimer = new GameTimer(EndTime);
        }
    }
}