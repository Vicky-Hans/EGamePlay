using UnityEngine;
using DG.Tweening;
using System;

namespace EGamePlay.Combat
{
    public enum MoveType
    {
        TargetMove,
        PathMove,
    }

    public enum SpeedType
    {
        Speed,
        Duration,
    }

    public class MoveWithDotweenComponent : Component
    {
        private SpeedType SpeedType { get; set; }
        private float Speed { get; set; }
        private float Duration { get; set; }
        private float ElapsedTime { get; set; }
        private IPosition PositionEntity { get; set; }
        private IPosition TargetPositionEntity { get; set; }
        private Entity TargetEntity { get; set; }
        private Vector3 Destination { get; set; }
        private Tweener MoveTweener { get; set; }
        private Action MoveFinishAction { get; set; }
        public override void Awake()
        {
            PositionEntity = (IPosition)Entity;
            ElapsedTime = 0;
        }
        public override void Update()
        {
            if (TargetPositionEntity == null) return;
            if (TargetEntity.IsDisposed)
            {
                TargetEntity = null;
                TargetPositionEntity = null;
                Entity.Destroy(Entity);
                return;
            }
            if (SpeedType == SpeedType.Speed) DoMoveToWithSpeed(TargetPositionEntity, Speed);
            if (SpeedType == SpeedType.Duration) DoTimeMove(MathF.Max(0, Duration - ElapsedTime));
            ElapsedTime += Time.deltaTime;
        }
        public MoveWithDotweenComponent DoMoveTo(Vector3 destination, float duration)
        {
            Destination = destination;
            DOTween.To(()=> PositionEntity.Position, x => PositionEntity.Position = x, Destination, duration).SetEase(Ease.Linear).OnComplete(OnMoveFinish);
            return this;
        }
        private void DoMoveToWithSpeed(IPosition targetPositionEntity, float speed = 1f)
        {
            Speed = speed;
            SpeedType = SpeedType.Speed;
            TargetPositionEntity = targetPositionEntity;
            TargetEntity = targetPositionEntity as Entity;
            MoveTweener?.Kill();
            var dist = Vector3.Distance(PositionEntity.Position, TargetPositionEntity.Position);
            var duration = dist / speed;
            MoveTweener = DOTween.To(() => PositionEntity.Position, x => PositionEntity.Position = x, TargetPositionEntity.Position, duration);
        }
        public void DoMoveToWithTime(IPosition targetPositionEntity, float time = 1f)
        {
            Duration = time;
            SpeedType = SpeedType.Duration;
            TargetPositionEntity = targetPositionEntity;
            TargetEntity = targetPositionEntity as Entity;
            DoTimeMove(time);
        }
        private void DoTimeMove(float time)
        {
            MoveTweener?.Kill();
            MoveTweener = DOTween.To(() => PositionEntity.Position, x => PositionEntity.Position = x, TargetPositionEntity.Position, time);
            MoveTweener.SetEase(Ease.Linear);
        }
        public void OnMoveFinish(Action action)
        {
            MoveFinishAction = action;
        }
        private void OnMoveFinish()
        {
            MoveFinishAction?.Invoke();
        }
    }
}