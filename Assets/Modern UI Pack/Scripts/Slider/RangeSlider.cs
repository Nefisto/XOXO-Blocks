﻿using TMPro;
using UnityEngine;

namespace Michsky.MUIP
{
    public class RangeSlider : MonoBehaviour
    {
        [Header("Settings")]
        [Range(0, 2)]
        public int decimalPlaces;

        public float minValue;
        public float maxValue = 1;
        public bool showLabels = true;
        public bool useWholeNumbers = true;

        [Header("Min Slider")]
        public RangeMinSlider minSlider;

        public TextMeshProUGUI minSliderLabel;

        [Header("Max Slider")]
        public RangeMaxSlider maxSlider;

        public TextMeshProUGUI maxSliderLabel;

        public float CurrentLowerValue => minSlider.value;
        public float CurrentUpperValue => maxSlider.realValue;

        private void Awake()
        {
            if (minSlider == null || maxSlider == null)
                return;

            if (showLabels)
            {
                minSlider.label = minSliderLabel;
                minSlider.numberFormat = "n" + decimalPlaces;
                maxSlider.label = maxSliderLabel;
                maxSlider.numberFormat = "n" + decimalPlaces;
            }

            else
            {
                minSliderLabel.gameObject.SetActive(false);
                maxSliderLabel.gameObject.SetActive(false);
            }

            if (useWholeNumbers)
            {
                minSlider.wholeNumbers = true;
                maxSlider.wholeNumbers = true;
            }

            minSlider.minValue = minValue;
            minSlider.maxValue = maxValue;
            minSlider.onValueChanged.AddListener(CheckForMinState);

            maxSlider.minValue = minValue;
            maxSlider.maxValue = maxValue;
        }

        public void CheckForMinState (float value)
        {
            if (minSlider.value >= maxSlider.realValue)
            {
                maxSlider.realValue = minSlider.value;
                minSlider.value = maxSlider.realValue - 1;
            }
        }
    }
}