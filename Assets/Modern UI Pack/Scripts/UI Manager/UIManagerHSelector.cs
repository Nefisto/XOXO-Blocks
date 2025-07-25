using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class UIManagerHSelector : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private UIManager UIManagerAsset;

        [HideInInspector]
        public bool overrideColors;

        [HideInInspector]
        public bool overrideFonts;

        [Header("Resources")]
        [SerializeField]
        private List<GameObject> images = new();

        [SerializeField]
        private List<GameObject> imagesHighlighted = new();

        [SerializeField]
        private List<GameObject> texts = new();

        private Color latestColor;

        private void Awake()
        {
            if (UIManagerAsset == null)
                UIManagerAsset = Resources.Load<UIManager>("MUIP Manager");

            enabled = true;

            if (UIManagerAsset.enableDynamicUpdate == false)
            {
                UpdateSelector();
                enabled = false;
            }
        }

        private void Update()
        {
            if (UIManagerAsset == null)
                return;
            if (UIManagerAsset.enableDynamicUpdate)
                UpdateSelector();
        }

        private void UpdateSelector()
        {
            if (overrideColors == false && latestColor != UIManagerAsset.selectorColor)
            {
                for (var i = 0; i < images.Count; ++i)
                {
                    var currentImage = images[i].GetComponent<Image>();
                    currentImage.color = new Color(UIManagerAsset.selectorColor.r, UIManagerAsset.selectorColor.g,
                        UIManagerAsset.selectorColor.b, currentImage.color.a);
                }

                for (var i = 0; i < imagesHighlighted.Count; ++i)
                {
                    var currentAlphaImage = imagesHighlighted[i].GetComponent<Image>();
                    currentAlphaImage.color = new Color(UIManagerAsset.selectorHighlightedColor.r,
                        UIManagerAsset.selectorHighlightedColor.g, UIManagerAsset.selectorHighlightedColor.b,
                        currentAlphaImage.color.a);
                }

                latestColor = UIManagerAsset.selectorColor;
            }

            for (var i = 0; i < texts.Count; ++i)
            {
                var currentText = texts[i].GetComponent<TextMeshProUGUI>();

                if (overrideColors == false)
                    currentText.color = new Color(UIManagerAsset.selectorColor.r, UIManagerAsset.selectorColor.g,
                        UIManagerAsset.selectorColor.b, currentText.color.a);
                if (overrideFonts == false)
                    currentText.font = UIManagerAsset.selectorFont;
            }
        }
    }
}