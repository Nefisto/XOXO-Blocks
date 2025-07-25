using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    public class PieChart : MaskableGraphic
    {
        // Chart Items
        [SerializeField]
        public List<PieChartDataNode> chartData = new();

        // Settings
        [Range(-75, 150)]
        public float borderThickness = 5;

        [SerializeField]
        private Color borderColor = new Color32(255, 255, 255, 255);

        public Transform indicatorParent;
        public string valuePrefix = "(";
        public string valueSuffix = ")";
        public bool addValueToIndicator = true;
        public bool enableBorderColor;

        private readonly float fillAmount = 1f;
        private readonly int segments = 720;

        protected override void Awake()
        {
            base.Awake();
            UpdateIndicators();
        }

        private void Update()
        {
            borderThickness = Mathf.Clamp(borderThickness, -75, rectTransform.rect.width / 3.333f);
        }

        protected override void OnPopulateMesh (VertexHelper vh)
        {
            if (chartData.Count == 0)
                return;

            var outer = -rectTransform.pivot.x * rectTransform.rect.width;
            var inner = -rectTransform.pivot.x * rectTransform.rect.width + borderThickness;

            var outer1 = -rectTransform.pivot.x * rectTransform.rect.width * 0.6f;
            var inner1 = -rectTransform.pivot.x * rectTransform.rect.width * 0.6f + borderThickness;

            vh.Clear();

            var prevX = Vector2.zero;
            var prevY = Vector2.zero;
            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(0, 1);
            var uv2 = new Vector2(1, 1);
            var uv3 = new Vector2(1, 0);
            Vector2 pos0;
            Vector2 pos1;
            Vector2 pos2;
            Vector2 pos3;

            var f = fillAmount;
            var degrees = 360f / segments;
            var fa = (int)((segments + 1) * f);

            var dataIndex = 0;
            var total = 0f;
            var currentValue = chartData[0].value;
            chartData.ForEach(s => total += s.value);
            var fillColor = chartData[0].color;

            for (var i = 0; i < fa; i++)
            {
                var rad = Mathf.Deg2Rad * (i * degrees);
                var c = Mathf.Cos(rad);
                var s = Mathf.Sin(rad);

                uv0 = new Vector2(0, 1);
                uv1 = new Vector2(1, 1);
                uv2 = new Vector2(1, 0);
                uv3 = new Vector2(0, 0);

                pos0 = prevX;
                pos1 = new Vector2(outer * c, outer * s);
                pos2 = new Vector2(inner * c, inner * s);
                pos3 = prevY;

                if (i > currentValue / total * segments)
                    if (dataIndex < chartData.Count - 1)
                    {
                        dataIndex += 1;
                        currentValue += chartData[dataIndex].value;
                        fillColor = chartData[dataIndex].color;
                    }

                vh.AddUIVertexQuad(SetVbo(new[] { pos0, pos1, pos2 * inner1 / inner, pos3 * inner1 / inner },
                    new[] { uv0, uv1, uv2, uv3 }, fillColor));

                if (enableBorderColor)
                {
                    vh.AddUIVertexQuad(SetVbo(new[] { pos0, pos1, pos2, pos3 }, new[] { uv0, uv1, uv2, uv3 },
                        borderColor));
                    vh.AddUIVertexQuad(SetVbo(
                        new[]
                        {
                            pos0 * outer1 / outer, pos1 * outer1 / outer, pos2 * inner1 / inner,
                            pos3 * inner1 / inner
                        }, new[] { uv0, uv1, uv2, uv3 }, borderColor));
                }

                prevX = pos1;
                prevY = pos2;
            }
        }

        public void SetData (List<PieChartDataNode> data)
        {
            chartData = data;
            SetVerticesDirty();
        }

        protected UIVertex[] SetVbo (Vector2[] vertices, Vector2[] uvs, Color32 color)
        {
            var vbo = new UIVertex[4];

            for (var i = 0; i < vertices.Length; i++)
            {
                var vert = UIVertex.simpleVert;
                vert.color = color;
                vert.position = vertices[i];
                vert.uv0 = uvs[i];
                vbo[i] = vert;
            }

            return vbo;
        }

        public void UpdateIndicators()
        {
            for (var i = 0; i < chartData.Count; ++i)
            {
                if (chartData[i].indicatorImage != null)
                    chartData[i].indicatorImage.color = chartData[i].color;

                if (chartData[i].indicatorText != null && addValueToIndicator)
                    chartData[i].indicatorText.text =
                        chartData[i].name + valuePrefix + chartData[i].value + valueSuffix;
                else if (chartData[i].indicatorText != null && addValueToIndicator == false)
                    chartData[i].indicatorText.text = chartData[i].name;
            }

            if (indicatorParent != null)
                StartCoroutine("UpdateIndicatorLayout");
        }

        public void ChangeValue (int itemIndex, float itemValue)
        {
            chartData[itemIndex].value = itemValue;

            enabled = false;
            enabled = true;
        }

        public void AddNewItem()
        {
            var item = new PieChartDataNode();

            if (indicatorParent.childCount != 0)
            {
                var tempIndex = indicatorParent.childCount - 1;

                var tempIndicator = indicatorParent.GetChild(tempIndex).gameObject;
                var newIndicator = Instantiate(tempIndicator, new Vector3(0, 0, 0), Quaternion.identity);

                newIndicator.transform.SetParent(indicatorParent, false);
                newIndicator.gameObject.name = "Item " + tempIndex + " Indicator";

                item.indicatorImage = newIndicator.GetComponentInChildren<Image>();
                item.indicatorText = newIndicator.GetComponentInChildren<TextMeshProUGUI>();
                item.name = "Chart Item " + tempIndex;
            }

            chartData.Add(item);
        }

        private IEnumerator UpdateIndicatorLayout()
        {
            yield return new WaitForSeconds(0.1f);
            LayoutRebuilder.ForceRebuildLayoutImmediate(indicatorParent.GetComponentInParent<RectTransform>());
        }

        [Serializable]
        public class PieChartDataNode
        {
            public string name = "Chart Item";
            public float value = 10;
            public Color32 color = new(255, 255, 255, 255);
            public Image indicatorImage;
            public TextMeshProUGUI indicatorText;
        }
    }
}