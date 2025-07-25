using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [AddComponentMenu("Modern UI Pack/Layout Group/Radial Layout Group")]
    public class RadialLayoutGroup : LayoutGroup
    {
        public enum ConstraintMode
        {
            Interval = 0,
            Range = 1
        }

        public enum Direction
        {
            Clockwise = 0,
            Counterclockwise = 1,
            Bidirectional = 2
        }

        private static readonly Vector2 center = new(0.5f, 0.5f);

        [SerializeField]
        private Direction refLayoutDir;

        [SerializeField]
        private float refRadiusStart = 200;

        [SerializeField]
        private float refRadiusDelta;

        [SerializeField]
        private float refRadiusRange;

        [SerializeField]
        private float refAngleDelta;

        [SerializeField]
        private float refAngleStart;

        [SerializeField]
        private float refAngleCenter;

        [SerializeField]
        private float refAngleRange = 200;

        [SerializeField]
        private bool refChildRotate;

        private readonly List<RectTransform> childList = new();
        private readonly List<ILayoutIgnorer> ignoreList = new();

        public Direction layoutDir
        {
            get => refLayoutDir;
            set => SetProperty(ref refLayoutDir, value);
        }

        public float radiusStart
        {
            get => refRadiusStart;
            set => SetProperty(ref refRadiusStart, value);
        }

        public float radiusDelta
        {
            get => refRadiusDelta;
            set => SetProperty(ref refRadiusDelta, value);
        }

        public float radiusRange
        {
            get => refRadiusRange;
            set => SetProperty(ref refRadiusRange, value);
        }

        public float angleDelta
        {
            get => refAngleDelta;
            set => SetProperty(ref refAngleDelta, value);
        }

        public float angleStart
        {
            get => refAngleStart;
            set => SetProperty(ref refAngleStart, value);
        }

        public float angleCenter
        {
            get => refAngleCenter;
            set => SetProperty(ref refAngleCenter, value);
        }

        public float angleRange
        {
            get => refAngleRange;
            set => SetProperty(ref refAngleRange, value);
        }

        public bool childRotate
        {
            get => refChildRotate;
            set => SetProperty(ref refChildRotate, value);
        }

        public override void CalculateLayoutInputVertical() { }
        public override void CalculateLayoutInputHorizontal() { }

        public override void SetLayoutHorizontal()
        {
            CalculateChildrenPositions();
        }

        public override void SetLayoutVertical()
        {
            CalculateChildrenPositions();
        }

        private void CalculateChildrenPositions()
        {
            m_Tracker.Clear();
            childList.Clear();

            for (var i = 0; i < transform.childCount; ++i)
            {
                var rect = transform.GetChild(i) as RectTransform;

                if (!rect.gameObject.activeSelf)
                    continue;

                ignoreList.Clear();
                rect.GetComponents(ignoreList);

                if (ignoreList.Count == 0)
                {
                    childList.Add(rect);
                    continue;
                }

                for (var j = 0; j < ignoreList.Count; j++)
                    if (!ignoreList[j].ignoreLayout)
                    {
                        childList.Add(rect);
                        break;
                    }

                ignoreList.Clear();
            }

            EnsureParameters(childList.Count);

            for (var i = 0; i < childList.Count; ++i)
            {
                var child = childList[i];
                var delta = i * angleDelta;
                var angle = layoutDir == Direction.Clockwise ? angleStart - delta : angleStart + delta;
                ProcessOneChild(child, angle, radiusStart + i * radiusDelta);
            }

            childList.Clear();
        }

        private void EnsureParameters (int childCount)
        {
            EnsureAngleParameters(childCount);
            EnsureRadiusParameters(childCount);
        }

        private void EnsureAngleParameters (int childCount)
        {
            var intervalCount = childCount - 1;

            switch (layoutDir)
            {
                case Direction.Clockwise:
                    if (intervalCount > 0)
                        angleDelta = angleRange / intervalCount;
                    else
                        angleDelta = 0;
                    break;

                case Direction.Counterclockwise:
                    if (intervalCount > 0)
                        angleDelta = angleRange / intervalCount;
                    else
                        angleDelta = 0;
                    break;

                case Direction.Bidirectional:
                    if (intervalCount > 0)
                        angleDelta = angleRange / intervalCount;
                    else
                        angleDelta = 0;
                    angleStart = angleCenter - angleRange * 0.5f;
                    break;
            }
        }


        private void EnsureRadiusParameters (int childCount)
        {
            var intervalCount = childCount - 1;

            switch (layoutDir)
            {
                case Direction.Clockwise:
                    if (intervalCount > 0)
                        radiusDelta = radiusRange / intervalCount;
                    else
                        radiusDelta = 0;
                    break;

                case Direction.Counterclockwise:

                case Direction.Bidirectional:
                    if (intervalCount > 0)
                        radiusDelta = radiusRange / intervalCount;
                    else
                        radiusDelta = 0;
                    break;
            }
        }

        private void ProcessOneChild (RectTransform child, float angle, float radius)
        {
            var pos = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0.0f);
            child.localPosition = pos * radius;

            var drivenProperties =
                DrivenTransformProperties.Anchors
                | DrivenTransformProperties.AnchoredPosition
                | DrivenTransformProperties.Rotation
                | DrivenTransformProperties.Pivot;
            m_Tracker.Add(this, child, drivenProperties);

            child.anchorMin = center;
            child.anchorMax = center;
            child.pivot = center;

            if (childRotate)
                child.localEulerAngles = new Vector3(0, 0, angle);
            else
                child.localEulerAngles = Vector3.zero;
        }
    }
}