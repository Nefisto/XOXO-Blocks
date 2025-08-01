using TMPro;
using UnityEngine;

namespace Michsky.MUIP
{
    [RequireComponent(typeof(CustomInputField))]
    public class SliderInput : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField]
        private SliderManager sliderManager;

        [Header("Settings")]
        [Range(1, 10)]
        public int maxChar = 5;

        [Range(0, 4)]
        public int decimals = 1;

        private TMP_InputField inputField;
        private CustomInputField muipField;

        private void Awake()
        {
            muipField = GetComponent<CustomInputField>();
            inputField = muipField.inputText;

            if (sliderManager == null)
            {
                Debug.LogWarning("'Slider Manager' is missing!");
                return;
            }

            if (sliderManager.mainSlider.wholeNumbers)
            {
                inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            }
            else if (sliderManager.mainSlider.wholeNumbers)
            {
                inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
                decimals = 0;
            }

            inputField.characterLimit = maxChar;
            inputField.onDeselect.AddListener(delegate { SetText(sliderManager.mainSlider.value); });

            muipField.processSubmit = true;
            muipField.clearOnSubmit = false;
            muipField.onSubmit.AddListener(SetValue);

            sliderManager.mainSlider.onValueChanged.AddListener(SetText);
            sliderManager.mainSlider.onValueChanged.Invoke(sliderManager.mainSlider.value);
        }

        private void SetText (float value)
        {
            if (decimals == 0)
                inputField.text = value.ToString("F0");
            else if (decimals == 1)
                inputField.text = value.ToString("F1");
            else if (decimals == 2)
                inputField.text = value.ToString("F2");
            else if (decimals == 3)
                inputField.text = value.ToString("F3");
            else if (decimals == 4)
                inputField.text = value.ToString("F4");
        }

        private void SetValue()
        {
            if (sliderManager.mainSlider.wholeNumbers)
                sliderManager.mainSlider.value = int.Parse(inputField.text);
            else
                sliderManager.mainSlider.value = float.Parse(inputField.text);
            if (float.Parse(inputField.text) > sliderManager.mainSlider.maxValue)
                sliderManager.mainSlider.value = sliderManager.mainSlider.maxValue;
        }
    }
}