using UnityEngine;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class UIManagerProgressBarLoop : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private UIManager UIManagerAsset;

        public bool hasBackground;
        public bool useRegularBackground;
        public bool overrideColors;

        [Header("Resources")]
        public Image bar;

        [HideInInspector]
        public Image background;

        private void Awake()
        {
            if (UIManagerAsset == null)
                UIManagerAsset = Resources.Load<UIManager>("MUIP Manager");

            enabled = true;

            if (UIManagerAsset.enableDynamicUpdate == false)
            {
                UpdateProgressBar();
                enabled = false;
            }
        }

        private void Update()
        {
            if (UIManagerAsset == null)
                return;
            if (UIManagerAsset.enableDynamicUpdate)
                UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            if (overrideColors == false)
            {
                bar.color = UIManagerAsset.progressBarColor;

                if (hasBackground)
                {
                    if (useRegularBackground)
                        background.color = UIManagerAsset.progressBarBackgroundColor;
                    else
                        background.color = UIManagerAsset.progressBarLoopBackgroundColor;
                }
            }
        }
    }
}