using NaughtyBezierCurves;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EGamePlay
{
    public class BezierComponent : MonoBehaviour
    {
        public List<BezierPoint3D> ctrlPoints { get => CollisionExecuteData.BezierCurve.KeyPoints; }
        public BezierCurve3D BezierCurve { get => CollisionExecuteData.BezierCurve; }
        public ItemExecute CollisionExecuteData;
        private Vector3 lastPosition;
        private Vector3 lastOutTangent;
        int pickedIndex = -1;
        enum CtrlPointPickedType
        {
            position,
            inTangent,
            outTangent
        }
        CtrlPointPickedType pickedType = CtrlPointPickedType.position;
        float SegmentCount = 100;
        public float Progress;
        private void OnDrawGizmos()
        {
            var bezierComponent = this;
            if (bezierComponent.ctrlPoints == null) return;
            for (var i = 0; i < bezierComponent.ctrlPoints.Count; i++)
            {
                var ctrlPoint = bezierComponent.ctrlPoints[i];
                var type = ctrlPoint.HandleStyle;
                var position = ctrlPoint.Position;
                var inTangentPoint = ctrlPoint.LeftHandleLocalPosition + position;
                var outTangentPoint = ctrlPoint.RightHandleLocalPosition + position;
                if (type == BezierPoint3D.HandleType.Broken)
                {
                    inTangentPoint = position;
                    outTangentPoint = position;
                }
                if (i > 0) Handles.DrawBezier(lastPosition, position, lastOutTangent, inTangentPoint, Color.green, null, 2f);
                lastPosition = position;
                lastOutTangent = outTangentPoint;
            }
        }
    }
}
