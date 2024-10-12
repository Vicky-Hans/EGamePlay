using UnityEngine;
using NaughtyBezierCurves;
using float3 = UnityEngine.Vector3;
using DG.Tweening;

namespace EGamePlay.Combat
{
    public class AbilityItemPathMoveComponent : Component
    {
        public IPosition PositionEntity { get; set; }
        public bool Rotate { get; set; }
        public float RotateRadian { get; set; }
        public float Duration { get; set; }
        private float Speed { get; set; } = 0.05f;
        private float Progress { get; set; }
        public BezierCurve3D BezierCurve { get; set; }
        public IPosition OriginEntity { get; set; }
        public Vector3 ExecutePoint { get; set; }
        public Vector3 OriginPoint => OriginEntity?.Position ?? ExecutePoint;
        public override void Update()
        {
            var abilityItem = GetEntity<AbilityItem>();
#if EGAMEPLAY_ET
            var itemUnit = abilityItem.GetComponent<CombatUnitComponent>().Unit;
            if (itemUnit != null && !PositionEntity.Position.Equals(itemUnit.MapUnit().Position))
            {
                PositionEntity.Position = itemUnit.MapUnit().Position;
            }
#endif
        }
        public float3[] GetPathPoints()
        {
            var points = GetPathLocalPoints();
            for (var i = 0; i < points.Length; i++)
            {
                var point = points[i];
                points[i] = point + OriginPoint;
            }
            return points;
        }
        public float3[] GetPathLocalPoints()
        {
            var abilityItem = GetEntity<AbilityItem>();
            var pathPoints = new float3[BezierCurve.Sampling];
            var perc = 1f / BezierCurve.Sampling;
            for (var i = 1; i <= BezierCurve.Sampling; i++)
            {
                var progress = perc * i;
                var endValue = BezierCurve.GetPoint(progress);
                var v = endValue;
                if (Rotate)
                {
                    var a = RotateRadian;

                    var x = v.x;
                    var y = v.y;
                    
                    var x1 = x * math.cos(a) - y * math.sin(a);
                    var y1 = (y * math.cos(a) + x * math.sin(a));

                    v = new float3(x1, y1, v.z);
                }
                pathPoints[i - 1] = v;
            }

            var duration = Duration;
            var length = math.distance(pathPoints[0], abilityItem.LocalPosition);
            for (var i = 0; i < pathPoints.Length; i++)
            {
                if (i == pathPoints.Length - 1)  break;
                var dist = math.distance(pathPoints[i + 1], pathPoints[i]);
                length += dist;
            }
            var speed = length / duration;
            Speed = speed;
            return pathPoints;
        }
        public void FollowMove()
        {
#if !EGAMEPLAY_ET
            var localPos = GetEntity<AbilityItem>().LocalPosition;
            var endValue = OriginPoint + localPos;
            PositionEntity.Position = endValue;
#endif
        }
        public void DOMove()
        {
            Progress = 1f / BezierCurve.Sampling;
            var localPos = BezierCurve.GetPoint(Progress);
            GetEntity<AbilityItem>().LocalPosition = localPos;
            var endValue = OriginPoint + localPos;
            var startPos = PositionEntity.Position;
            var duration = math.distance(endValue, startPos) / Speed;
            DOTween.To(() => startPos, (x) => PositionEntity.Position = x, endValue, duration).SetEase(Ease.Linear).OnComplete(DOMoveNext);
        }
        private void DOMoveNext()
        {
            if (Progress >= 1f) return;
            Progress += 1f / BezierCurve.Sampling;
            Progress = System.Math.Min(1f, Progress);
            var endValue = BezierCurve.GetPoint(Progress);
            var startPos = PositionEntity.Position;
#if UNITY
            var duration = math.distance(endValue, startPos) / Speed;
            DOTween.To(() => startPos, x => PositionEntity.Position = x, endValue, duration).SetEase(Ease.Linear).OnComplete(DOMoveNext);
#endif
        }
    }
}