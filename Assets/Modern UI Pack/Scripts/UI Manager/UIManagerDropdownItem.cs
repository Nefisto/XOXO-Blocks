using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class UIManagerDropdownItem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private UIManager UIManagerAsset;

        public bool overrideColors;
        public bool overrideFonts;

        [Header("Resources")]
        [SerializeField]
        private Image itemBackground;

        [SerializeField]
        private Image itemIcon;

        [SerializeField]
        private TextMeshProUGUI itemText;

        private void Awake()
        {
            if (UIManagerAsset == null)
                UIManagerAsset = Resources.Load<UIManager>("MUIP Manager");

            enabled = true;

            if (UIManagerAsset.enableDynamicUpdate == false)
            {
                UpdateDropdown();
                enabled = false;
            }
        }

        private void Update()
        {
            if (UIManagerAsset == null)
                return;
            if (UIManagerAsset.enableDynamicUpdate)
                UpdateDropdown();
        }

        private void UpdateDropdown()
        {
            if (overrideFonts == false && itemText != null)
                itemText.font = UIManagerAsset.dropdownItemFont;
            if (overrideColors == false)
            {
                if (itemBackground != null)
                    itemBackground.color = UIManagerAsset.dropdownItemBackgroundColor;
                if (itemIcon != null)
                    itemIcon.color = UIManagerAsset.dropdownItemPrimaryColor;
                if (itemText != null)
                    itemText.color = UIManagerAsset.dropdownItemPrimaryColor;
            }
        }
    }
}