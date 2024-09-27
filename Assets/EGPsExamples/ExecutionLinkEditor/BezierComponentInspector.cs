using UnityEngine;
using UnityEditor;
namespace EGamePlay
{
    [CustomEditor(typeof(BezierComponent))]
    public class BezierComponentInspector : Editor
    {
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
        private void OnSceneGUI()
        {
            var bezierComponent = target as BezierComponent;
            if (bezierComponent != null && bezierComponent.ctrlPoints == null) return;
            if (pickedIndex >= bezierComponent.ctrlPoints.Count) pickedIndex = -1;
            if (pickedIndex != -1)
            {
                var pickedCtrlPoint = bezierComponent.ctrlPoints[pickedIndex];
                if (pickedCtrlPoint.HandleStyle == NaughtyBezierCurves.BezierPoint3D.HandleType.Broken) pickedType = CtrlPointPickedType.position;
                if (pickedType == CtrlPointPickedType.position)
                {
                    var newPosition = Handles.PositionHandle(pickedCtrlPoint.Position, Quaternion.identity);
                    pickedCtrlPoint.Position = newPosition;
                }
                else if (pickedType == CtrlPointPickedType.inTangent)
                {
                    var position = pickedCtrlPoint.Position;
                    var newInTangent = Handles.PositionHandle(pickedCtrlPoint.InTangent + position, Quaternion.identity) - position;
                    pickedCtrlPoint.InTangent = newInTangent;
                }
                else if (pickedType == CtrlPointPickedType.outTangent)
                {
                    var position = pickedCtrlPoint.Position;
                    var newOutTangent = Handles.PositionHandle(pickedCtrlPoint.OutTangent + position, Quaternion.identity) - position;
                    pickedCtrlPoint.OutTangent = newOutTangent;
                }
            }
            for (var i = 0; i < bezierComponent.ctrlPoints.Count; i++)
            {
                var ctrlPoint = bezierComponent.ctrlPoints[i];
                var type = ctrlPoint.HandleStyle;
                var position = ctrlPoint.Position;
                var inTangentPoint = ctrlPoint.InTangent + position;
                var outTangentPoint = ctrlPoint.OutTangent + position;
                var buttonPicked = Handles.Button(position, Quaternion.identity, 0.1f, 0.1f, Handles.CubeHandleCap);
                if (buttonPicked)
                {
                    pickedIndex = i;
                    pickedType = CtrlPointPickedType.position;
                    //to-do:
                }

                if (type == NaughtyBezierCurves.BezierPoint3D.HandleType.Broken) continue;
                Handles.DrawLine(position, inTangentPoint);
                var inTangentPicked = Handles.Button(inTangentPoint, Quaternion.identity, 0.1f, 0.1f, Handles.SphereHandleCap);
                if (inTangentPicked)
                {
                    pickedIndex = i;
                    pickedType = CtrlPointPickedType.inTangent;
                    //to-do:
                }
                Handles.DrawLine(position, outTangentPoint);
                var outTangentPicked = Handles.Button(outTangentPoint, Quaternion.identity, 0.1f, 0.1f, Handles.SphereHandleCap);
                if (!outTangentPicked) continue;
                pickedIndex = i;
                pickedType = CtrlPointPickedType.outTangent;
                //to_do:
            }
        }
    }
}
