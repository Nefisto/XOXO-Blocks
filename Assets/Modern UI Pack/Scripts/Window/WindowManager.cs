using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Michsky.MUIP
{
    public class WindowManager : MonoBehaviour
    {
        // Content
        public List<WindowItem> windows = new();

        // Settings
        public int currentWindowIndex;
        public bool cullWindows = true;
        public bool initializeButtons = true;
        public WindowChangeEvent onWindowChange;
        public bool altMode;
        private readonly string buttonFadeIn = "Hover to Pressed";
        private readonly string buttonFadeOut = "Pressed to Normal";

        // Helpers
        private readonly string windowFadeIn = "In";
        private readonly string windowFadeOut = "Out";
        private float cachedStateLength;
        private GameObject currentButton;
        private Animator currentButtonAnimator;
        private int currentButtonIndex;

        // Hidden vars
        private GameObject currentWindow;
        private Animator currentWindowAnimator;
        private bool isInitialized;
        private int newWindowIndex;
        private GameObject nextButton;
        private Animator nextButtonAnimator;
        private GameObject nextWindow;
        private Animator nextWindowAnimator;

        private void Awake()
        {
            if (windows.Count == 0)
                return;

            InitializeWindows();
        }

        private void OnEnable()
        {
            if (isInitialized && nextWindowAnimator == null)
            {
                currentWindowAnimator.Play(windowFadeIn);
                if (currentButtonAnimator != null)
                    currentButtonAnimator.Play(buttonFadeIn);
            }

            else if (isInitialized && nextWindowAnimator != null)
            {
                nextWindowAnimator.Play(windowFadeIn);
                if (nextButtonAnimator != null)
                    nextButtonAnimator.Play(buttonFadeIn);
            }
        }

        public void InitializeWindows()
        {
            if (windows[currentWindowIndex].firstSelected != null)
                EventSystem.current.firstSelectedGameObject = windows[currentWindowIndex].firstSelected;
            if (windows[currentWindowIndex].buttonObject != null)
            {
                currentButton = windows[currentWindowIndex].buttonObject;
                currentButtonAnimator = currentButton.GetComponent<Animator>();
                currentButtonAnimator.Play(buttonFadeIn);
            }

            currentWindow = windows[currentWindowIndex].windowObject;
            currentWindowAnimator = currentWindow.GetComponent<Animator>();
            currentWindowAnimator.Play(windowFadeIn);
            onWindowChange.Invoke(currentWindowIndex);

            if (altMode)
                cachedStateLength = 0.3f;
            else
                cachedStateLength = MUIPInternalTools.GetAnimatorClipLength(currentWindowAnimator,
                    MUIPInternalTools.windowManagerStateName);

            isInitialized = true;

            for (var i = 0; i < windows.Count; i++)
            {
                if (i != currentWindowIndex && cullWindows)
                    windows[i].windowObject.SetActive(false);
                if (windows[i].buttonObject != null && initializeButtons)
                {
                    var tempName = windows[i].windowName;
                    var tempButton = windows[i].buttonObject.GetComponent<ButtonManager>();

                    if (tempButton != null)
                    {
                        tempButton.onClick.RemoveAllListeners();
                        tempButton.onClick.AddListener(() => OpenPanel(tempName));
                    }
                }
            }
        }

        public void OpenFirstTab()
        {
            if (currentWindowIndex != 0)
            {
                currentWindow = windows[currentWindowIndex].windowObject;
                currentWindowAnimator = currentWindow.GetComponent<Animator>();
                currentWindowAnimator.Play(windowFadeOut);

                if (windows[currentWindowIndex].buttonObject != null)
                {
                    currentButton = windows[currentWindowIndex].buttonObject;
                    currentButtonAnimator = currentButton.GetComponent<Animator>();
                    currentButtonAnimator.Play(buttonFadeOut);
                }

                currentWindowIndex = 0;
                currentButtonIndex = 0;

                currentWindow = windows[currentWindowIndex].windowObject;
                currentWindowAnimator = currentWindow.GetComponent<Animator>();
                currentWindowAnimator.Play(windowFadeIn);

                if (windows[currentWindowIndex].firstSelected != null)
                    EventSystem.current.firstSelectedGameObject = windows[currentWindowIndex].firstSelected;
                if (windows[currentButtonIndex].buttonObject != null)
                {
                    currentButton = windows[currentButtonIndex].buttonObject;
                    currentButtonAnimator = currentButton.GetComponent<Animator>();
                    currentButtonAnimator.Play(buttonFadeIn);
                }

                onWindowChange.Invoke(currentWindowIndex);
            }

            else if (currentWindowIndex == 0)
            {
                currentWindow = windows[currentWindowIndex].windowObject;
                currentWindowAnimator = currentWindow.GetComponent<Animator>();
                currentWindowAnimator.Play(windowFadeIn);

                if (windows[currentWindowIndex].firstSelected != null)
                    EventSystem.current.firstSelectedGameObject = windows[currentWindowIndex].firstSelected;
                if (windows[currentButtonIndex].buttonObject != null)
                {
                    currentButton = windows[currentButtonIndex].buttonObject;
                    currentButtonAnimator = currentButton.GetComponent<Animator>();
                    currentButtonAnimator.Play(buttonFadeIn);
                }
            }
        }

        public void OpenWindow (string newWindow)
        {
            for (var i = 0; i < windows.Count; i++)
                if (windows[i].windowName == newWindow)
                {
                    newWindowIndex = i;
                    break;
                }

            if (newWindowIndex != currentWindowIndex)
            {
                if (cullWindows)
                    StopCoroutine("DisablePreviousWindow");

                currentWindow = windows[currentWindowIndex].windowObject;

                if (windows[currentWindowIndex].buttonObject != null)
                    currentButton = windows[currentWindowIndex].buttonObject;

                currentWindowIndex = newWindowIndex;
                nextWindow = windows[currentWindowIndex].windowObject;
                nextWindow.SetActive(true);

                currentWindowAnimator = currentWindow.GetComponent<Animator>();
                nextWindowAnimator = nextWindow.GetComponent<Animator>();

                currentWindowAnimator.Play(windowFadeOut);
                nextWindowAnimator.Play(windowFadeIn);

                if (cullWindows)
                    StartCoroutine("DisablePreviousWindow");

                currentButtonIndex = newWindowIndex;

                if (windows[currentWindowIndex].firstSelected != null)
                    EventSystem.current.firstSelectedGameObject = windows[currentWindowIndex].firstSelected;
                if (windows[currentButtonIndex].buttonObject != null)
                {
                    nextButton = windows[currentButtonIndex].buttonObject;

                    currentButtonAnimator = currentButton.GetComponent<Animator>();
                    nextButtonAnimator = nextButton.GetComponent<Animator>();

                    currentButtonAnimator.Play(buttonFadeOut);
                    nextButtonAnimator.Play(buttonFadeIn);
                }

                onWindowChange.Invoke(currentWindowIndex);
            }
        }

        // Old method
        public void OpenPanel (string newPanel)
        {
            OpenWindow(newPanel);
        }

        public void OpenWindowByIndex (int windowIndex)
        {
            for (var i = 0; i < windows.Count; i++)
                if (windows[i].windowName == windows[windowIndex].windowName)
                {
                    OpenWindow(windows[windowIndex].windowName);
                    break;
                }
        }

        public void NextWindow()
        {
            if (currentWindowIndex <= windows.Count - 2)
            {
                if (cullWindows)
                    StopCoroutine("DisablePreviousWindow");

                currentWindow = windows[currentWindowIndex].windowObject;
                currentWindow.gameObject.SetActive(true);

                if (windows[currentButtonIndex].buttonObject != null)
                {
                    currentButton = windows[currentButtonIndex].buttonObject;
                    nextButton = windows[currentButtonIndex + 1].buttonObject;

                    currentButtonAnimator = currentButton.GetComponent<Animator>();
                    currentButtonAnimator.Play(buttonFadeOut);
                }

                currentWindowAnimator = currentWindow.GetComponent<Animator>();
                currentWindowAnimator.Play(windowFadeOut);

                currentWindowIndex += 1;
                currentButtonIndex += 1;

                nextWindow = windows[currentWindowIndex].windowObject;
                nextWindow.gameObject.SetActive(true);

                nextWindowAnimator = nextWindow.GetComponent<Animator>();
                nextWindowAnimator.Play(windowFadeIn);

                if (cullWindows)
                    StartCoroutine("DisablePreviousWindow");
                if (windows[currentWindowIndex].firstSelected != null)
                    EventSystem.current.firstSelectedGameObject = windows[currentWindowIndex].firstSelected;
                if (nextButton != null)
                {
                    nextButtonAnimator = nextButton.GetComponent<Animator>();
                    nextButtonAnimator.Play(buttonFadeIn);
                }

                onWindowChange.Invoke(currentWindowIndex);
            }
        }

        public void PrevWindow()
        {
            if (currentWindowIndex >= 1)
            {
                if (cullWindows)
                    StopCoroutine("DisablePreviousWindow");

                currentWindow = windows[currentWindowIndex].windowObject;
                currentWindow.gameObject.SetActive(true);

                if (windows[currentButtonIndex].buttonObject != null)
                {
                    currentButton = windows[currentButtonIndex].buttonObject;
                    nextButton = windows[currentButtonIndex - 1].buttonObject;

                    currentButtonAnimator = currentButton.GetComponent<Animator>();
                    currentButtonAnimator.Play(buttonFadeOut);
                }

                currentWindowAnimator = currentWindow.GetComponent<Animator>();
                currentWindowAnimator.Play(windowFadeOut);

                currentWindowIndex -= 1;
                currentButtonIndex -= 1;

                nextWindow = windows[currentWindowIndex].windowObject;
                nextWindow.gameObject.SetActive(true);

                nextWindowAnimator = nextWindow.GetComponent<Animator>();
                nextWindowAnimator.Play(windowFadeIn);

                if (cullWindows)
                    StartCoroutine("DisablePreviousWindow");
                if (windows[currentWindowIndex].firstSelected != null)
                    EventSystem.current.firstSelectedGameObject = windows[currentWindowIndex].firstSelected;
                if (nextButton != null)
                {
                    nextButtonAnimator = nextButton.GetComponent<Animator>();
                    nextButtonAnimator.Play(buttonFadeIn);
                }

                onWindowChange.Invoke(currentWindowIndex);
            }
        }

        public void ShowCurrentWindow()
        {
            if (nextWindowAnimator == null)
                currentWindowAnimator.Play(windowFadeIn);
            else
                nextWindowAnimator.Play(windowFadeIn);
        }

        public void HideCurrentWindow()
        {
            if (nextWindowAnimator == null)
                currentWindowAnimator.Play(windowFadeOut);
            else
                nextWindowAnimator.Play(windowFadeOut);
        }

        public void ShowCurrentButton()
        {
            if (nextButtonAnimator == null)
                currentButtonAnimator.Play(buttonFadeIn);
            else
                nextButtonAnimator.Play(buttonFadeIn);
        }

        public void HideCurrentButton()
        {
            if (nextButtonAnimator == null)
                currentButtonAnimator.Play(buttonFadeOut);
            else
                nextButtonAnimator.Play(buttonFadeOut);
        }

        public void AddNewItem()
        {
            var window = new WindowItem();

            if (windows.Count != 0 && windows[windows.Count - 1].windowObject != null)
            {
                var tempIndex = windows.Count - 1;

                var tempWindow = windows[tempIndex].windowObject.transform.parent.GetChild(tempIndex).gameObject;
                var newWindow = Instantiate(tempWindow, new Vector3(0, 0, 0), Quaternion.identity);

                newWindow.transform.SetParent(windows[tempIndex].windowObject.transform.parent, false);
                newWindow.gameObject.name = "New Window " + tempIndex;

                window.windowName = "New Window " + tempIndex;
                window.windowObject = newWindow;

                if (windows[tempIndex].buttonObject != null)
                {
                    var tempButton = windows[tempIndex].buttonObject.transform.parent.GetChild(tempIndex).gameObject;
                    var newButton = Instantiate(tempButton, new Vector3(0, 0, 0), Quaternion.identity);

                    newButton.transform.SetParent(windows[tempIndex].buttonObject.transform.parent, false);
                    newButton.gameObject.name = "New Window " + tempIndex;

                    window.buttonObject = newButton;
                }
            }

            windows.Add(window);
        }

        private IEnumerator DisablePreviousWindow()
        {
            yield return new WaitForSecondsRealtime(cachedStateLength);

            for (var i = 0; i < windows.Count; i++)
            {
                if (i == currentWindowIndex)
                    continue;

                windows[i].windowObject.SetActive(false);
            }
        }

        // Events
        [Serializable]
        public class WindowChangeEvent : UnityEvent<int> { }

        [Serializable]
        public class WindowItem
        {
            public string windowName = "My Window";
            public GameObject windowObject;
            public GameObject buttonObject;
            public GameObject firstSelected;
        }
    }
}