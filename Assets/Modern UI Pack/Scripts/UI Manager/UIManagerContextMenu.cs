using UnityEngine;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class UIManagerContextMenu : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private UIManager UIManagerAsset;

        [Header("Resources")]
        [SerializeField]
        private Image backgroundImage;

        private void Awake()
        {
            if (UIManagerAsset == null)
                UIManagerAsset = Resources.Load<UIManager>("MUIP Manager");

            enabled = true;

            if (UIManagerAsset.enableDynamicUpdate == false)
            {
                UpdateContextMenu();
                enabled = false;
            }
        }

        private void Update()
        {
            if (UIManagerAsset == null)
                return;
            if (UIManagerAsset.enableDynamicUpdate)
                UpdateContextMenu();
        }

        private void UpdateContextMenu()
        {
            if (backgroundImage != null)
                backgroundImage.color = UIManagerAsset.contextBackgroundColor;
        }
    }
}