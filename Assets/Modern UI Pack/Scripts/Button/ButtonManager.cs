﻿using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
#endif

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class ButtonManager : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        public enum AnimationSolution
        {
            Custom,
            ScriptBased
        }

        public enum RippleUpdateMode
        {
            Normal,
            UnscaledTime
        }

        // Content
        public Sprite buttonIcon;
        public string buttonText = "Button";

        [Range(0.1f, 10)]
        public float iconScale = 1;

        [Range(10, 200)]
        public float textSize = 24;

        // Auto Size
        public bool autoFitContent = true;
        public Padding padding;

        [Range(0, 100)]
        public int spacing = 15;

        public HorizontalLayoutGroup disabledLayout;
        public HorizontalLayoutGroup normalLayout;
        public HorizontalLayoutGroup highlightedLayout;

        [SerializeField]
        private HorizontalLayoutGroup mainLayout;

        [SerializeField]
        private ContentSizeFitter mainFitter;

        [SerializeField]
        private ContentSizeFitter targetFitter;

        [SerializeField]
        private RectTransform targetRect;

        // Resources
        public CanvasGroup normalCG;
        public CanvasGroup highlightCG;
        public CanvasGroup disabledCG;
        public TextMeshProUGUI normalText;
        public TextMeshProUGUI highlightedText;
        public TextMeshProUGUI disabledText;
        public Image normalImage;
        public Image highlightImage;
        public Image disabledImage;
        public AudioSource soundSource;

        [SerializeField]
        private GameObject rippleParent;

        // Settings
        public bool isInteractable = true;
        public bool enableIcon;
        public bool enableText = true;
        public bool useCustomContent;

        [SerializeField]
        private bool useCustomTextSize;

        public bool checkForDoubleClick = true;
        public bool enableButtonSounds;
        public bool useHoverSound = true;
        public bool useClickSound = true;
        public AudioClip hoverSound;
        public AudioClip clickSound;
        public bool useUINavigation;
        public Navigation.Mode navigationMode = Navigation.Mode.Automatic;
        public GameObject selectOnUp;
        public GameObject selectOnDown;
        public GameObject selectOnLeft;
        public GameObject selectOnRight;
        public bool wrapAround;
        public bool useRipple = true;

        [Range(0.1f, 1)]
        public float doubleClickPeriod = 0.25f;

        [Range(0.25f, 15)]
        public float fadingMultiplier = 8;

        [SerializeField]
        private AnimationSolution animationSolution = AnimationSolution.ScriptBased;

        // Events
        public UnityEvent onClick = new();
        public UnityEvent onDoubleClick = new();
        public UnityEvent onHover = new();
        public UnityEvent onLeave = new();

        // Ripple
        [SerializeField]
        private RippleUpdateMode rippleUpdateMode = RippleUpdateMode.UnscaledTime;

        [SerializeField]
        private Canvas targetCanvas;

        public Sprite rippleShape;

        [Range(0.1f, 5)]
        public float speed = 1f;

        [Range(0.5f, 25)]
        public float maxSize = 4f;

        public Color startColor = new(1f, 1f, 1f, 0.2f);
        public Color transitionColor = new(1f, 1f, 1f, 0f);

        [SerializeField]
        private bool renderOnTop;

        [SerializeField]
        private bool centered;

        // Helpers
        private bool isInitialized;
        private bool isPointerOn;
        private Button targetButton;
        private bool waitingForDoubleClickInput;

        private void OnEnable()
        {
            if (isInitialized == false)
                Initialize();
            UpdateUI();
        }

        private void OnDisable()
        {
            if (isInteractable == false)
                return;

            if (disabledCG != null)
                disabledCG.alpha = 0;
            if (normalCG != null)
                normalCG.alpha = 1;
            if (highlightCG != null)
                highlightCG.alpha = 0;
        }

        public void OnDeselect (BaseEventData eventData)
        {
            if (isInteractable == false)
                return;
            if (animationSolution == AnimationSolution.ScriptBased)
                StartCoroutine("SetNormal");
            if (useUINavigation)
                onLeave.Invoke();
        }

        public void OnPointerClick (PointerEventData eventData)
        {
            if (isInteractable == false || eventData.button != PointerEventData.InputButton.Left)
                return;
            if (enableButtonSounds && useClickSound && soundSource != null)
                soundSource.PlayOneShot(clickSound);

            // Invoke click actions
            onClick.Invoke();

            // Check for double click
            if (checkForDoubleClick == false || gameObject.activeInHierarchy == false)
                return;
            if (waitingForDoubleClickInput)
            {
                onDoubleClick.Invoke();
                waitingForDoubleClickInput = false;
                return;
            }

            waitingForDoubleClickInput = true;

            StopCoroutine("CheckForDoubleClick");
            StartCoroutine("CheckForDoubleClick");
        }

        public void OnPointerDown (PointerEventData eventData)
        {
            if (isInteractable == false)
                return;
#if UNITY_IOS || UNITY_ANDROID
            if (animationSolution == AnimationSolution.ScriptBased) { StartCoroutine("SetHighlight"); }
            if (useRipple == true)
#else
            if (useRipple && isPointerOn)
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                if (targetCanvas != null
                    && (targetCanvas.renderMode == RenderMode.ScreenSpaceCamera
                        || targetCanvas.renderMode == RenderMode.WorldSpace))
                    CreateRipple(targetCanvas.worldCamera.ScreenToWorldPoint(Input.mousePosition));
                else
                    CreateRipple(Input.mousePosition);
#elif ENABLE_INPUT_SYSTEM
                if (targetCanvas != null && (targetCanvas.renderMode == RenderMode.ScreenSpaceCamera || targetCanvas.renderMode == RenderMode.WorldSpace)) { CreateRipple(targetCanvas.worldCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue())); }
#if UNITY_IOS || UNITY_ANDROID
                else { CreateRipple(Touchscreen.current.primaryTouch.position.ReadValue()); }
#else
                else { CreateRipple(Mouse.current.position.ReadValue()); }
#endif
#endif
        }

        public void OnPointerEnter (PointerEventData eventData)
        {
            if (isInteractable == false)
                return;
            if (enableButtonSounds && useHoverSound && soundSource != null)
                soundSource.PlayOneShot(hoverSound);
            if (animationSolution == AnimationSolution.ScriptBased)
                StartCoroutine("SetHighlight");

            isPointerOn = true;
            onHover.Invoke();
        }

        public void OnPointerExit (PointerEventData eventData)
        {
            if (isInteractable == false)
                return;
            if (animationSolution == AnimationSolution.ScriptBased)
                StartCoroutine("SetNormal");

            isPointerOn = false;
            onLeave.Invoke();
        }

        public void OnPointerUp (PointerEventData eventData)
        {
#if UNITY_IOS || UNITY_ANDROID
            if (animationSolution == AnimationSolution.ScriptBased) { StartCoroutine("SetNormal"); }
#endif
        }

        public void OnSelect (BaseEventData eventData)
        {
            if (isInteractable == false)
                return;
            if (animationSolution == AnimationSolution.ScriptBased)
                StartCoroutine("SetHighlight");
            if (useUINavigation)
                onHover.Invoke();
        }

        public void OnSubmit (BaseEventData eventData)
        {
            if (isInteractable == false)
                return;
            if (animationSolution == AnimationSolution.ScriptBased)
                StartCoroutine("SetNormal");

            onClick.Invoke();
        }

        private void Initialize()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            if (animationSolution == AnimationSolution.ScriptBased)
            {
                var tempAnimator = GetComponent<Animator>();
                if (tempAnimator != null)
                    Destroy(tempAnimator);
            }

            if (gameObject.GetComponent<Image>() == null)
            {
                var raycastImg = gameObject.AddComponent<Image>();
                raycastImg.color = new Color(0, 0, 0, 0);
                raycastImg.raycastTarget = true;
            }

            if (targetCanvas == null)
                targetCanvas = GetComponentInParent<Canvas>();
            if (normalCG == null)
            {
                normalCG = new GameObject().AddComponent<CanvasGroup>();
                normalCG.gameObject.AddComponent<RectTransform>();
                normalCG.transform.SetParent(transform);
                normalCG.gameObject.name = "Normal";
            }

            if (highlightCG == null)
            {
                highlightCG = new GameObject().AddComponent<CanvasGroup>();
                highlightCG.gameObject.AddComponent<RectTransform>();
                highlightCG.transform.SetParent(transform);
                highlightCG.gameObject.name = "Highlight";
            }

            if (disabledCG == null)
            {
                disabledCG = new GameObject().AddComponent<CanvasGroup>();
                disabledCG.gameObject.AddComponent<RectTransform>();
                disabledCG.transform.SetParent(transform);
                disabledCG.gameObject.name = "Disabled";
            }

            if (useRipple && rippleParent != null)
                rippleParent.SetActive(false);
            else if (useRipple == false && rippleParent != null)
                Destroy(rippleParent);

            if (gameObject.activeInHierarchy)
                StartCoroutine("LayoutFix");
            if (targetButton == null)
            {
                if (gameObject.GetComponent<Button>() == null)
                    targetButton = gameObject.AddComponent<Button>();
                else
                    targetButton = GetComponent<Button>();

                targetButton.transition = Selectable.Transition.None;

                var customNav = new Navigation();
                customNav.mode = Navigation.Mode.None;
                targetButton.navigation = customNav;
            }

            if (useUINavigation)
                AddUINavigation();

            isInitialized = true;
        }

        public void UpdateUI()
        {
            if (autoFitContent == false)
            {
                if (mainFitter != null)
                    mainFitter.enabled = false;
                if (mainLayout != null)
                    mainLayout.enabled = false;
                if (targetFitter != null)
                {
                    targetFitter.enabled = false;

                    if (targetRect != null)
                    {
                        targetRect.anchorMin = new Vector2(0, 0);
                        targetRect.anchorMax = new Vector2(1, 1);
                        targetRect.offsetMin = new Vector2(0, 0);
                        targetRect.offsetMax = new Vector2(0, 0);
                    }
                }
            }

            else
            {
                if (mainFitter != null)
                    mainFitter.enabled = true;
                if (mainLayout != null)
                    mainLayout.enabled = true;
                if (targetFitter != null)
                    targetFitter.enabled = true;
            }

            if (disabledLayout != null)
            {
                disabledLayout.padding = new RectOffset(padding.left, padding.right, padding.top, padding.bottom);
                disabledLayout.spacing = spacing;
            }

            if (normalLayout != null)
            {
                normalLayout.padding = new RectOffset(padding.left, padding.right, padding.top, padding.bottom);
                normalLayout.spacing = spacing;
            }

            if (highlightedLayout != null)
            {
                highlightedLayout.padding = new RectOffset(padding.left, padding.right, padding.top, padding.bottom);
                highlightedLayout.spacing = spacing;
            }

            if (normalCG != null && isInteractable)
                normalCG.alpha = 1;
            if (disabledCG != null && isInteractable == false)
                disabledCG.alpha = 1;
            if (highlightCG != null)
                highlightCG.alpha = 0;

            if (useCustomContent == false)
            {
                // Set Text
                if (enableText)
                {
                    if (normalText != null)
                    {
                        normalText.gameObject.SetActive(true);
                        normalText.text = buttonText;
                        if (useCustomTextSize == false)
                            normalText.fontSize = textSize;
                    }

                    if (highlightedText != null)
                    {
                        highlightedText.gameObject.SetActive(true);
                        highlightedText.text = buttonText;
                        if (useCustomTextSize == false)
                            highlightedText.fontSize = textSize;
                    }

                    if (disabledText != null)
                    {
                        disabledText.gameObject.SetActive(true);
                        disabledText.text = buttonText;
                        if (useCustomTextSize == false)
                            disabledText.fontSize = textSize;
                    }
                }

                else if (enableText == false)
                {
                    if (normalText != null)
                        normalText.gameObject.SetActive(false);
                    if (highlightedText != null)
                        highlightedText.gameObject.SetActive(false);
                    if (disabledText != null)
                        disabledText.gameObject.SetActive(false);
                }

                // Set Icon
                if (enableIcon)
                {
                    var tempScale = new Vector3(iconScale, iconScale, iconScale);
                    if (normalImage != null)
                    {
                        normalImage.transform.parent.gameObject.SetActive(true);
                        normalImage.sprite = buttonIcon;
                        normalImage.transform.localScale = tempScale;
                    }

                    if (highlightImage != null)
                    {
                        highlightImage.transform.parent.gameObject.SetActive(true);
                        highlightImage.sprite = buttonIcon;
                        ;
                        highlightImage.transform.localScale = tempScale;
                    }

                    if (disabledImage != null)
                    {
                        disabledImage.transform.parent.gameObject.SetActive(true);
                        disabledImage.sprite = buttonIcon;
                        ;
                        disabledImage.transform.localScale = tempScale;
                    }
                }

                else
                {
                    if (normalImage != null)
                        normalImage.transform.parent.gameObject.SetActive(false);
                    if (highlightImage != null)
                        highlightImage.transform.parent.gameObject.SetActive(false);
                    if (disabledImage != null)
                        disabledImage.transform.parent.gameObject.SetActive(false);
                }
            }

#if UNITY_EDITOR
            if (Application.isPlaying == false && autoFitContent)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
                if (disabledCG != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(disabledCG.GetComponent<RectTransform>());
                if (normalCG != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(normalCG.GetComponent<RectTransform>());
                if (highlightCG != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(highlightCG.GetComponent<RectTransform>());
            }
#endif

            if (Application.isPlaying == false || gameObject.activeInHierarchy == false)
                return;
            if (isInteractable == false)
                StartCoroutine("SetDisabled");
            else if (isInteractable && disabledCG.alpha == 1)
                StartCoroutine("SetNormal");

            StartCoroutine("LayoutFix");
        }

        public void SetText (string text)
        {
            buttonText = text;
            UpdateUI();
        }

        public void SetIcon (Sprite icon)
        {
            buttonIcon = icon;
            UpdateUI();
        }

        public void Interactable (bool value)
        {
            isInteractable = value;

            if (gameObject.activeInHierarchy == false)
                return;
            if (isInteractable == false)
                StartCoroutine("SetDisabled");
            else if (isInteractable && disabledCG.alpha == 1)
                StartCoroutine("SetNormal");
        }

        public void AddUINavigation()
        {
            if (targetButton == null)
                return;

            targetButton.transition = Selectable.Transition.None;
            var customNav = new Navigation();
            customNav.mode = navigationMode;

            if (navigationMode == Navigation.Mode.Vertical || navigationMode == Navigation.Mode.Horizontal)
            {
                customNav.wrapAround = wrapAround;
            }
            else if (navigationMode == Navigation.Mode.Explicit)
            {
                StartCoroutine("InitUINavigation", customNav);
                return;
            }

            targetButton.navigation = customNav;
        }

        public void CreateRipple (Vector2 pos)
        {
            if (rippleParent != null)
            {
                var rippleObj = new GameObject();
                rippleObj.AddComponent<Image>();
                rippleObj.GetComponent<Image>().sprite = rippleShape;
                rippleObj.name = "Ripple";
                rippleParent.SetActive(true);
                rippleObj.transform.SetParent(rippleParent.transform);

                if (renderOnTop)
                    rippleParent.transform.SetAsLastSibling();
                else
                    rippleParent.transform.SetAsFirstSibling();

                if (centered)
                    rippleObj.transform.localPosition = new Vector2(0f, 0f);
                else
                    rippleObj.transform.position = pos;

                rippleObj.AddComponent<Ripple>();
                var tempRipple = rippleObj.GetComponent<Ripple>();
                tempRipple.speed = speed;
                tempRipple.maxSize = maxSize;
                tempRipple.startColor = startColor;
                tempRipple.transitionColor = transitionColor;

                if (rippleUpdateMode == RippleUpdateMode.Normal)
                    tempRipple.unscaledTime = false;
                else
                    tempRipple.unscaledTime = true;
            }
        }

        private IEnumerator LayoutFix()
        {
            yield return new WaitForSecondsRealtime(0.025f);

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

            if (disabledCG != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(disabledCG.GetComponent<RectTransform>());
            if (normalCG != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(normalCG.GetComponent<RectTransform>());
            if (highlightCG != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(highlightCG.GetComponent<RectTransform>());
        }

        private IEnumerator SetNormal()
        {
            StopCoroutine("SetHighlight");
            StopCoroutine("SetDisabled");

            while (normalCG.alpha < 0.99f)
            {
                normalCG.alpha += Time.unscaledDeltaTime * fadingMultiplier;
                highlightCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
                disabledCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
                yield return null;
            }

            normalCG.alpha = 1;
            highlightCG.alpha = 0;
            disabledCG.alpha = 0;
        }

        private IEnumerator SetHighlight()
        {
            StopCoroutine("SetNormal");
            StopCoroutine("SetDisabled");

            while (highlightCG.alpha < 0.99f)
            {
                normalCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
                highlightCG.alpha += Time.unscaledDeltaTime * fadingMultiplier;
                disabledCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
                yield return null;
            }

            normalCG.alpha = 0;
            highlightCG.alpha = 1;
            disabledCG.alpha = 0;
        }

        private IEnumerator SetDisabled()
        {
            StopCoroutine("SetNormal");
            StopCoroutine("SetHighlight");

            while (disabledCG.alpha < 0.99f)
            {
                normalCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
                highlightCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
                disabledCG.alpha += Time.unscaledDeltaTime * fadingMultiplier;
                yield return null;
            }

            normalCG.alpha = 0;
            highlightCG.alpha = 0;
            disabledCG.alpha = 1;
        }

        private IEnumerator CheckForDoubleClick()
        {
            yield return new WaitForSecondsRealtime(doubleClickPeriod);
            waitingForDoubleClickInput = false;
        }

        private IEnumerator InitUINavigation (Navigation nav)
        {
            yield return new WaitForSecondsRealtime(1);
            if (selectOnUp != null)
                nav.selectOnUp = selectOnUp.GetComponent<Selectable>();
            if (selectOnDown != null)
                nav.selectOnDown = selectOnDown.GetComponent<Selectable>();
            if (selectOnLeft != null)
                nav.selectOnLeft = selectOnLeft.GetComponent<Selectable>();
            if (selectOnRight != null)
                nav.selectOnRight = selectOnRight.GetComponent<Selectable>();
            targetButton.navigation = nav;
        }

        [Serializable]
        public class Padding
        {
            public int left = 20;
            public int right = 20;
            public int top = 5;
            public int bottom = 5;
        }

#if UNITY_EDITOR
        public bool isPreset;
        public int latestTabIndex;
#endif
    }
}