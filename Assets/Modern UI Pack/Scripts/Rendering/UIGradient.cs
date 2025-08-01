﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Modern UI Pack/Effects/UI Gradient")]
    public class UIGradient : BaseMeshEffect
    {
        public enum Blend
        {
            Override,
            Add,
            Multiply
        }

        public enum Type
        {
            Horizontal,
            Vertical,
            Diamond
        }

        [SerializeField]
        private Type _gradientType;

        [SerializeField]
        private Blend _blendMode = Blend.Multiply;

        [SerializeField]
        private bool _modifyVertices = true;

        [SerializeField]
        [Range(-1, 1)]
        private float _offset;

        [SerializeField]
        [Range(0.1f, 10)]
        private float _zoom = 1f;

        [SerializeField]
        private Gradient _effectGradient = new()
        {
            colorKeys = new[] { new GradientColorKey(Color.black, 0), new GradientColorKey(Color.white, 1) }
        };

        public Blend BlendMode
        {
            get => _blendMode;
            set
            {
                _blendMode = value;
                graphic.SetVerticesDirty();
            }
        }

        public Gradient EffectGradient
        {
            get => _effectGradient;
            set
            {
                _effectGradient = value;
                graphic.SetVerticesDirty();
            }
        }

        public Type GradientType
        {
            get => _gradientType;
            set
            {
                _gradientType = value;
                graphic.SetVerticesDirty();
            }
        }

        public bool ModifyVertices
        {
            get => _modifyVertices;
            set
            {
                _modifyVertices = value;
                graphic.SetVerticesDirty();
            }
        }

        public float Offset
        {
            get => _offset;
            set
            {
                _offset = Mathf.Clamp(value, -1f, 1f);
                graphic.SetVerticesDirty();
            }
        }

        public float Zoom
        {
            get => _zoom;
            set
            {
                _zoom = Mathf.Clamp(value, 0.1f, 10f);
                graphic.SetVerticesDirty();
            }
        }

        public override void ModifyMesh (VertexHelper helper)
        {
            if (!IsActive() || helper.currentVertCount == 0)
                return;

            var _vertexList = new List<UIVertex>();
            helper.GetUIVertexStream(_vertexList);
            var nCount = _vertexList.Count;

            switch (GradientType)
            {
                case Type.Horizontal:
                case Type.Vertical:
                {
                    var bounds = GetBounds(_vertexList);
                    var min = bounds.xMin;
                    var w = bounds.width;
                    Func<UIVertex, float> GetPosition = v => v.position.x;

                    if (GradientType == Type.Vertical)
                    {
                        min = bounds.yMin;
                        w = bounds.height;
                        GetPosition = v => v.position.y;
                    }

                    var width = w == 0f ? 0f : 1f / w / Zoom;
                    var zoomOffset = (1 - 1 / Zoom) * 0.5f;
                    var offset = Offset * (1 - zoomOffset) - zoomOffset;

                    if (ModifyVertices)
                        SplitTrianglesAtGradientStops(_vertexList, bounds, zoomOffset, helper);

                    var vertex = new UIVertex();
                    for (var i = 0; i < helper.currentVertCount; i++)
                    {
                        helper.PopulateUIVertex(ref vertex, i);
                        vertex.color = BlendColor(vertex.color,
                            EffectGradient.Evaluate((GetPosition(vertex) - min) * width - offset));
                        helper.SetUIVertex(vertex, i);
                    }
                }
                    break;

                case Type.Diamond:
                {
                    var bounds = GetBounds(_vertexList);
                    var height = bounds.height == 0f ? 0f : 1f / bounds.height / Zoom;
                    var radius = bounds.center.y / 2f;
                    var center = (Vector3.right + Vector3.up) * radius + Vector3.forward * _vertexList[0].position.z;

                    if (ModifyVertices)
                    {
                        helper.Clear();
                        for (var i = 0; i < nCount; i++)
                            helper.AddVert(_vertexList[i]);

                        var centralVertex = new UIVertex();
                        centralVertex.position = center;
                        centralVertex.normal = _vertexList[0].normal;
                        centralVertex.uv0 = new Vector2(0.5f, 0.5f);
                        centralVertex.color = Color.white;
                        helper.AddVert(centralVertex);

                        for (var i = 1; i < nCount; i++)
                            helper.AddTriangle(i - 1, i, nCount);
                        helper.AddTriangle(0, nCount - 1, nCount);
                    }

                    var vertex = new UIVertex();

                    for (var i = 0; i < helper.currentVertCount; i++)
                    {
                        helper.PopulateUIVertex(ref vertex, i);
                        vertex.color = BlendColor(vertex.color, EffectGradient.Evaluate(
                            Vector3.Distance(vertex.position, center) * height - Offset));
                        helper.SetUIVertex(vertex, i);
                    }
                }
                    break;
            }
        }

        private Rect GetBounds (List<UIVertex> vertices)
        {
            var left = vertices[0].position.x;
            var right = left;
            var bottom = vertices[0].position.y;
            var top = bottom;

            for (var i = vertices.Count - 1; i >= 1; --i)
            {
                var x = vertices[i].position.x;
                var y = vertices[i].position.y;

                if (x > right)
                    right = x;
                else if (x < left)
                    left = x;

                if (y > top)
                    top = y;
                else if (y < bottom)
                    bottom = y;
            }

            return new Rect(left, bottom, right - left, top - bottom);
        }

        private void SplitTrianglesAtGradientStops (List<UIVertex> _vertexList, Rect bounds, float zoomOffset,
            VertexHelper helper)
        {
            var stops = FindStops(zoomOffset, bounds);
            if (stops.Count > 0)
            {
                helper.Clear();
                var nCount = _vertexList.Count;

                for (var i = 0; i < nCount; i += 3)
                {
                    var positions = GetPositions(_vertexList, i);
                    var originIndices = new List<int>(3);
                    var starts = new List<UIVertex>(3);
                    var ends = new List<UIVertex>(2);

                    for (var s = 0; s < stops.Count; s++)
                    {
                        var initialCount = helper.currentVertCount;
                        var hadEnds = ends.Count > 0;
                        var earlyStart = false;

                        for (var p = 0; p < 3; p++)
                            if (!originIndices.Contains(p) && positions[p] < stops[s])
                            {
                                var p1 = (p + 1) % 3;
                                var start = _vertexList[p + i];

                                if (positions[p1] > stops[s])
                                {
                                    originIndices.Insert(0, p);
                                    starts.Insert(0, start);
                                    earlyStart = true;
                                }

                                else
                                {
                                    originIndices.Add(p);
                                    starts.Add(start);
                                }
                            }

                        if (originIndices.Count == 0)
                            continue;
                        if (originIndices.Count == 3)
                            break;

                        foreach (var start in starts)
                            helper.AddVert(start);

                        ends.Clear();
                        foreach (var index in originIndices)
                        {
                            var oppositeIndex = (index + 1) % 3;

                            if (positions[oppositeIndex] < stops[s])
                                oppositeIndex = (oppositeIndex + 1) % 3;
                            ends.Add(
                                CreateSplitVertex(_vertexList[index + i], _vertexList[oppositeIndex + i], stops[s]));
                        }

                        if (ends.Count == 1)
                        {
                            var oppositeIndex = (originIndices[0] + 2) % 3;
                            ends.Add(CreateSplitVertex(_vertexList[originIndices[0] + i],
                                _vertexList[oppositeIndex + i], stops[s]));
                        }

                        foreach (var end in ends)
                            helper.AddVert(end);

                        if (hadEnds)
                        {
                            helper.AddTriangle(initialCount - 2, initialCount, initialCount + 1);
                            helper.AddTriangle(initialCount - 2, initialCount + 1, initialCount - 1);

                            if (starts.Count > 0)
                            {
                                if (earlyStart)
                                    helper.AddTriangle(initialCount - 2, initialCount + 3, initialCount);
                                else
                                    helper.AddTriangle(initialCount + 1, initialCount + 3, initialCount - 1);
                            }
                        }

                        else
                        {
                            var vertexCount = helper.currentVertCount;
                            helper.AddTriangle(initialCount, vertexCount - 2, vertexCount - 1);

                            if (starts.Count > 1)
                                helper.AddTriangle(initialCount, vertexCount - 1, initialCount + 1);
                        }

                        starts.Clear();
                    }

                    if (ends.Count > 0)
                    {
                        if (starts.Count == 0)
                            for (var p = 0; p < 3; p++)
                                if (!originIndices.Contains(p) && positions[p] > stops[stops.Count - 1])
                                {
                                    var p1 = (p + 1) % 3;
                                    var end = _vertexList[p + i];

                                    if (positions[p1] > stops[stops.Count - 1])
                                        starts.Insert(0, end);
                                    else
                                        starts.Add(end);
                                }

                        foreach (var start in starts)
                            helper.AddVert(start);

                        var vertexCount = helper.currentVertCount;

                        if (starts.Count > 1)
                        {
                            helper.AddTriangle(vertexCount - 4, vertexCount - 2, vertexCount - 1);
                            helper.AddTriangle(vertexCount - 4, vertexCount - 1, vertexCount - 3);
                        }

                        else if (starts.Count > 0)
                        {
                            helper.AddTriangle(vertexCount - 3, vertexCount - 1, vertexCount - 2);
                        }
                    }

                    else
                    {
                        helper.AddVert(_vertexList[i]);
                        helper.AddVert(_vertexList[i + 1]);
                        helper.AddVert(_vertexList[i + 2]);
                        var vertexCount = helper.currentVertCount;
                        helper.AddTriangle(vertexCount - 3, vertexCount - 2, vertexCount - 1);
                    }
                }
            }
        }

        private float[] GetPositions (List<UIVertex> _vertexList, int index)
        {
            var positions = new float[3];

            if (GradientType == Type.Horizontal)
            {
                positions[0] = _vertexList[index].position.x;
                positions[1] = _vertexList[index + 1].position.x;
                positions[2] = _vertexList[index + 2].position.x;
            }

            else
            {
                positions[0] = _vertexList[index].position.y;
                positions[1] = _vertexList[index + 1].position.y;
                positions[2] = _vertexList[index + 2].position.y;
            }

            return positions;
        }

        private List<float> FindStops (float zoomOffset, Rect bounds)
        {
            var stops = new List<float>();
            var offset = Offset * (1 - zoomOffset);
            var startBoundary = zoomOffset - offset;
            var endBoundary = 1 - zoomOffset - offset;

            foreach (var color in EffectGradient.colorKeys)
            {
                if (color.time >= endBoundary)
                    break;

                if (color.time > startBoundary)
                    stops.Add((color.time - startBoundary) * Zoom);
            }

            foreach (var alpha in EffectGradient.alphaKeys)
            {
                if (alpha.time >= endBoundary)
                    break;

                if (alpha.time > startBoundary)
                    stops.Add((alpha.time - startBoundary) * Zoom);
            }

            var min = bounds.xMin;
            var size = bounds.width;

            if (GradientType == Type.Vertical)
            {
                min = bounds.yMin;
                size = bounds.height;
            }

            stops.Sort();

            for (var i = 0; i < stops.Count; i++)
            {
                stops[i] = stops[i] * size + min;

                if (i > 0 && Math.Abs(stops[i] - stops[i - 1]) < 2)
                {
                    stops.RemoveAt(i);
                    --i;
                }
            }

            return stops;
        }

        private UIVertex CreateSplitVertex (UIVertex vertex1, UIVertex vertex2, float stop)
        {
            if (GradientType == Type.Horizontal)
            {
                var sx = vertex1.position.x - stop;
                var dx = vertex1.position.x - vertex2.position.x;
                var dy = vertex1.position.y - vertex2.position.y;
                var uvx = vertex1.uv0.x - vertex2.uv0.x;
                var uvy = vertex1.uv0.y - vertex2.uv0.y;
                var ratio = sx / dx;
                var splitY = vertex1.position.y - dy * ratio;

                var splitVertex = new UIVertex();
                splitVertex.position = new Vector3(stop, splitY, vertex1.position.z);
                splitVertex.normal = vertex1.normal;
                splitVertex.uv0 = new Vector2(vertex1.uv0.x - uvx * ratio, vertex1.uv0.y - uvy * ratio);
                splitVertex.color = Color.white;
                return splitVertex;
            }

            else
            {
                var sy = vertex1.position.y - stop;
                var dy = vertex1.position.y - vertex2.position.y;
                var dx = vertex1.position.x - vertex2.position.x;
                var uvx = vertex1.uv0.x - vertex2.uv0.x;
                var uvy = vertex1.uv0.y - vertex2.uv0.y;
                var ratio = sy / dy;
                var splitX = vertex1.position.x - dx * ratio;

                var splitVertex = new UIVertex();
                splitVertex.position = new Vector3(splitX, stop, vertex1.position.z);
                splitVertex.normal = vertex1.normal;
                splitVertex.uv0 = new Vector2(vertex1.uv0.x - uvx * ratio, vertex1.uv0.y - uvy * ratio);
                splitVertex.color = Color.white;
                return splitVertex;
            }
        }

        private Color BlendColor (Color colorA, Color colorB)
        {
            switch (BlendMode)
            {
                default:
                    return colorB;

                case Blend.Add:
                    return colorA + colorB;

                case Blend.Multiply:
                    return colorA * colorB;
            }
        }
    }
}