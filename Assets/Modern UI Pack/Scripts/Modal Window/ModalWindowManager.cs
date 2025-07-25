using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ModalWindowManager : MonoBehaviour
    {
        public enum CloseBehaviour
        {
            None,
            Disable,
            Destroy
        }

        public enum OnEnableBehaviour
        {
            None,
            Restore
        }

        public enum StartBehaviour
        {
            None,
            Disable,
            Enable
        }

        // Resources
        public Image windowIcon;
        public TextMeshProUGUI windowTitle;
        public TextMeshProUGUI windowDescription;
        public ButtonManager confirmButton;
        public ButtonManager cancelButton;
        public Animator mwAnimator;

        // Content
        public Sprite icon;
        public string titleText = "Title";

        [TextArea(1, 4)]
        public string descriptionText = "Description here";

        // Events
        public UnityEvent onOpen = new();
        public UnityEvent onClose = new();
        public UnityEvent onConfirm = new();
        public UnityEvent onCancel = new();

        // Settings
        public bool useCustomContent;
        public bool isOn;
        public bool closeOnCancel = true;
        public bool closeOnConfirm = true;
        public bool showCancelButton = true;
        public bool showConfirmButton = true;
        public StartBehaviour startBehaviour = StartBehaviour.Disable;
        public CloseBehaviour closeBehaviour = CloseBehaviour.Disable;
        public OnEnableBehaviour onEnableBehaviour = OnEnableBehaviour.None;

        // Helpers
        private float cachedStateLength;

        private void Awake()
        {
            isOn = false;

            if (mwAnimator == null)
                mwAnimator = gameObject.GetComponent<Animator>();
            if (closeOnCancel)
                onCancel.AddListener(CloseWindow);
            if (closeOnConfirm)
                onConfirm.AddListener(CloseWindow);
            if (confirmButton != null)
                confirmButton.onClick.AddListener(onConfirm.Invoke);
            if (cancelButton != null)
                cancelButton.onClick.AddListener(onCancel.Invoke);
            if (startBehaviour == StartBehaviour.Disable)
            {
                isOn = false;
                gameObject.SetActive(false);
            }
            else if (startBehaviour == StartBehaviour.Enable)
            {
                isOn = false;
                OpenWindow();
            }

            cachedStateLength =
                MUIPInternalTools.GetAnimatorClipLength(mwAnimator, MUIPInternalTools.modalWindowStateName);
            UpdateUI();
        }

        private void OnEnable()
        {
            if (onEnableBehaviour == OnEnableBehaviour.Restore && isOn)
            {
                isOn = false;
                Open();
            }
        }

        private void OnDisable()
        {
            if (onEnableBehaviour == OnEnableBehaviour.None)
                isOn = false;
        }

        public void UpdateUI()
        {
            if (useCustomContent)
                return;

            if (windowIcon != null)
                windowIcon.sprite = icon;
            if (windowTitle != null)
                windowTitle.text = titleText;
            if (windowDescription != null)
                windowDescription.text = descriptionText;

            if (showCancelButton && cancelButton != null)
                cancelButton.gameObject.SetActive(true);
            else if (cancelButton != null)
                cancelButton.gameObject.SetActive(false);

            if (showConfirmButton && confirmButton != null)
                confirmButton.gameObject.SetActive(true);
            else if (confirmButton != null)
                confirmButton.gameObject.SetActive(false);
        }

        public void Open()
        {
            if (isOn)
                return;

            isOn = true;
            gameObject.SetActive(true);
            onOpen.Invoke();

            StopCoroutine("DisableObject");
            mwAnimator.Play("Fade-in");
        }

        public void Close()
        {
            if (!isOn)
                return;

            isOn = false;
            onClose.Invoke();

            mwAnimator.Play("Fade-out");
            StartCoroutine("DisableObject");
        }

        public void AnimateWindow()
        {
            if (!isOn)
            {
                StopCoroutine("DisableObject");

                isOn = true;
                gameObject.SetActive(true);
                mwAnimator.Play("Fade-in");
            }

            else
            {
                isOn = false;
                mwAnimator.Play("Fade-out");

                StartCoroutine("DisableObject");
            }
        }

        private IEnumerator DisableObject()
        {
            yield return new WaitForSecondsRealtime(cachedStateLength);

            if (closeBehaviour == CloseBehaviour.Disable)
                gameObject.SetActive(false);
            else if (closeBehaviour == CloseBehaviour.Destroy)
                Destroy(gameObject);
        }

        #region Obsolote

        public void OpenWindow()
        {
            Open();
        }

        public void CloseWindow()
        {
            Close();
        }

        #endregion
    }
}