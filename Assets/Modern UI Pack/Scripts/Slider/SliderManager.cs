using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [RequireComponent(typeof(Slider))]
    public class SliderManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // Resources
        public Slider mainSlider;
        public TextMeshProUGUI valueText;
        public TextMeshProUGUI popupValueText;

        // Saving
        public bool enableSaving;
        public bool invokeOnAwake = true;
        public string sliderTag = "My Slider";

        // Settings
        public bool usePercent;
        public bool showValue = true;
        public bool showPopupValue = true;
        public bool useRoundValue;
        public float minValue;
        public float maxValue;

        [SerializeField]
        public SliderEvent onValueChanged = new();

        [Space(8)]
        public SliderEvent sliderEvent;

        // Other Variables
        [HideInInspector]
        public Animator sliderAnimator;

        [HideInInspector]
        public float saveValue;

        private void Awake()
        {
            if (enableSaving)
            {
                if (PlayerPrefs.HasKey(sliderTag + "MUIPSliderValue") == false)
                    saveValue = mainSlider.value;
                else
                    saveValue = PlayerPrefs.GetFloat(sliderTag + "MUIPSliderValue");

                mainSlider.value = saveValue;
                mainSlider.onValueChanged.AddListener(delegate
                {
                    saveValue = mainSlider.value;
                    PlayerPrefs.SetFloat(sliderTag + "MUIPSliderValue", saveValue);
                });
            }

            mainSlider.onValueChanged.AddListener(delegate
            {
                sliderEvent.Invoke(mainSlider.value);
                UpdateUI();
            });

            if (sliderAnimator == null && showPopupValue)
                try
                {
                    sliderAnimator = gameObject.GetComponent<Animator>();
                }
                catch
                {
                    showPopupValue = false;
                }

            if (invokeOnAwake)
                sliderEvent.Invoke(mainSlider.value);
            UpdateUI();
        }

        public void OnPointerEnter (PointerEventData eventData)
        {
            if (showPopupValue && sliderAnimator != null)
                sliderAnimator.Play("Value In");
        }

        public void OnPointerExit (PointerEventData eventData)
        {
            if (showPopupValue && sliderAnimator != null)
                sliderAnimator.Play("Value Out");
        }

        public void UpdateUI()
        {
            if (useRoundValue)
            {
                if (usePercent)
                {
                    if (valueText != null)
                        valueText.text = Mathf.Round(mainSlider.value * 1.0f) + "%";
                    if (popupValueText != null)
                        popupValueText.text = Mathf.Round(mainSlider.value * 1.0f) + "%";
                }

                else
                {
                    if (valueText != null)
                        valueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString();
                    if (popupValueText != null)
                        popupValueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString();
                }
            }

            else
            {
                if (usePercent)
                {
                    if (valueText != null)
                        valueText.text = mainSlider.value.ToString("F1") + "%";
                    if (popupValueText != null)
                        popupValueText.text = mainSlider.value.ToString("F1") + "%";
                }

                else
                {
                    if (valueText != null)
                        valueText.text = mainSlider.value.ToString("F1");
                    if (popupValueText != null)
                        popupValueText.text = mainSlider.value.ToString("F1");
                }
            }
        }

        // Events
        [Serializable]
        public class SliderEvent : UnityEvent<float> { }
    }
}