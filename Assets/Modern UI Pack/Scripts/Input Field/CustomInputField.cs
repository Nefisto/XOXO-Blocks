using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace Michsky.MUIP
{
    [RequireComponent(typeof(TMP_InputField))]
    [RequireComponent(typeof(Animator))]
    public class CustomInputField : MonoBehaviour
    {
        [Header("Resources")]
        public TMP_InputField inputText;

        [SerializeField]
        private Animator inputFieldAnimator;

        [Header("Settings")]
        public bool processSubmit;

        public bool clearOnSubmit = true;

        [Tooltip("Set the current event system object as null.")]
        [SerializeField]
        private bool setEventSystem;

        [Header("Events")]
        public UnityEvent onSubmit = new();

        // Hidden variables
        private readonly float cachedDuration = 0.5f;
        private readonly string inAnim = "In";
        private readonly string instaInAnim = "Instant In";
        private readonly string instaOutAnim = "Instant Out";
        private readonly string outAnim = "Out";
        private bool isActive;

        private void Awake()
        {
            Initialize();

            inputText.onSelect.AddListener(delegate { AnimateIn(); });
            inputText.onEndEdit.AddListener(delegate { HandleEndEdit(); });
            inputText.onValueChanged.AddListener(delegate { UpdateState(); });

            UpdateStateInstant();
        }

        private void Update()
        {
            if (!processSubmit
                || string.IsNullOrEmpty(inputText.text)
                || EventSystem.current.currentSelectedGameObject != inputText.gameObject)
                return;

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Return))
            {
                onSubmit.Invoke();

                if (clearOnSubmit)
                {
                    inputText.text = "";
                    UpdateState();
                }
            }
#elif ENABLE_INPUT_SYSTEM
            if (Keyboard.current.enterKey.wasPressedThisFrame) 
            { 
                onSubmit.Invoke(); 
                
                if (clearOnSubmit) 
                { 
                    inputText.text = ""; 
                    UpdateState();
                } 
            }
#endif
        }

        private void OnEnable()
        {
            if (inputText == null || inputFieldAnimator == null)
                Initialize();
            inputText.ForceLabelUpdate();
            UpdateStateInstant();
        }

        private void Initialize()
        {
            if (inputText == null)
                inputText = gameObject.GetComponent<TMP_InputField>();
            if (inputFieldAnimator == null)
                inputFieldAnimator = gameObject.GetComponent<Animator>();
        }

        public void AnimateIn()
        {
            if (inputFieldAnimator.gameObject.activeInHierarchy && !isActive)
            {
                StopCoroutine("DisableAnimator");
                StartCoroutine("DisableAnimator");

                isActive = true;
                inputFieldAnimator.enabled = true;
                inputFieldAnimator.Play(inAnim);
            }
        }

        public void AnimateOut()
        {
            if (inputFieldAnimator.gameObject.activeInHierarchy && inputText.text.Length == 0 && isActive)
            {
                StopCoroutine("DisableAnimator");
                StartCoroutine("DisableAnimator");

                isActive = false;
                inputFieldAnimator.enabled = true;
                inputFieldAnimator.Play(outAnim);
            }
        }

        public void UpdateState()
        {
            if (inputText.text.Length == 0)
                AnimateOut();
            else
                AnimateIn();
        }

        public void UpdateStateInstant()
        {
            inputFieldAnimator.enabled = true;

            StopCoroutine("DisableAnimator");
            StartCoroutine("DisableAnimator");

            if (inputText.text.Length == 0)
            {
                isActive = false;
                inputFieldAnimator.Play(instaOutAnim);
            }
            else
            {
                isActive = true;
                inputFieldAnimator.Play(instaInAnim);
            }
        }

        private void HandleEndEdit()
        {
            if (setEventSystem
                && string.IsNullOrEmpty(inputText.text)
                && !EventSystem.current.alreadySelecting
                && EventSystem.current.currentSelectedGameObject == inputText.gameObject)
                EventSystem.current.SetSelectedGameObject(null);

            AnimateOut();
        }

        private IEnumerator DisableAnimator()
        {
            yield return new WaitForSecondsRealtime(cachedDuration);
            inputFieldAnimator.enabled = false;
        }
    }
}