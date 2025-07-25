using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    public class CustomDropdown : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler, IPointerClickHandler,
        ISubmitHandler
    {
        public enum AnimationType
        {
            Modular,
            Custom
        }

        public enum PanelDirection
        {
            Bottom,
            Top
        }

        // Resources
        public Animator dropdownAnimator;
        public GameObject triggerObject;
        public TextMeshProUGUI selectedText;
        public Image selectedImage;
        public Transform itemParent;
        public GameObject itemObject;
        public GameObject scrollbar;
        public VerticalLayoutGroup itemList;
        public AudioSource soundSource;
        public RectTransform listRect;
        public CanvasGroup listCG;
        public CanvasGroup contentCG;

        // Settings
        public bool isInteractable = true;
        public bool enableIcon = true;
        public bool enableTrigger = true;
        public bool enableScrollbar = true;
        public bool updateOnEnable = true;
        public bool outOnPointerExit;
        public bool setHighPriority = true;
        public bool invokeAtStart;
        public bool initAtStart = true;
        public bool enableDropdownSounds;
        public bool useHoverSound = true;
        public bool useClickSound = true;

        [Range(1, 50)]
        public int itemPaddingTop = 8;

        [Range(1, 50)]
        public int itemPaddingBottom = 8;

        [Range(1, 50)]
        public int itemPaddingLeft = 8;

        [Range(1, 50)]
        public int itemPaddingRight = 25;

        [Range(1, 50)]
        public int itemSpacing = 8;

        public int selectedItemIndex;

        // Animation
        public AnimationType animationType;
        public PanelDirection panelDirection;

        [Range(25, 1000)]
        public float panelSize = 200;

        [Range(0.5f, 10)]
        public float curveSpeed = 3;

        public AnimationCurve animationCurve = new(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));

        // Saving
        public bool saveSelected;
        public string saveKey = "My Dropdown";

        // Item list
        [SerializeField]
        public List<Item> items = new();

        public DropdownEvent onValueChanged = new();
        public ItemTextChangedEvent onItemTextChanged = new();

        // Audio
        public AudioClip hoverSound;
        public AudioClip clickSound;

        [HideInInspector]
        public bool isOn;

        [HideInInspector]
        public int index;

        [HideInInspector]
        public int siblingIndex;

        [HideInInspector]
        public TextMeshProUGUI setItemText;

        [HideInInspector]
        public Image setItemImage;

#if UNITY_EDITOR
        public bool extendEvents;
#endif
        private Sprite imageHelper;

        // Helpers
        private bool isInitialized;
        private string textHelper;
        private EventTrigger triggerEvent;

        private void OnEnable()
        {
            if (!isInitialized)
                Initialize();
            if (updateOnEnable && index < items.Count)
                SetDropdownIndex(selectedItemIndex, false);

            listCG.alpha = 0;
            listCG.interactable = false;
            listCG.blocksRaycasts = false;
            listRect.sizeDelta = new Vector2(listRect.sizeDelta.x, 0);
        }

        public void OnPointerClick (PointerEventData eventData)
        {
            if (!isInteractable)
                return;
            if (enableDropdownSounds && useClickSound)
                soundSource.PlayOneShot(clickSound);

            Animate();
        }

        public void OnPointerEnter (PointerEventData eventData)
        {
            if (!isInteractable)
                return;
            if (enableDropdownSounds && useHoverSound)
                soundSource.PlayOneShot(hoverSound);
        }

        public void OnPointerExit (PointerEventData eventData)
        {
            if (!isInteractable)
                return;
            if (outOnPointerExit && isOn)
            {
                Animate();
                isOn = false;
            }
        }

        public void OnSubmit (BaseEventData eventData)
        {
            if (!isInteractable)
                return;
            if (enableDropdownSounds && useClickSound)
                soundSource.PlayOneShot(clickSound);

            Animate();
        }

        private void Initialize()
        {
            if (enableTrigger && triggerObject != null)
            {
                // triggerButton = gameObject.GetComponent<Button>();
                triggerEvent = triggerObject.AddComponent<EventTrigger>();
                var entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;
                entry.callback.AddListener(eventData => { Animate(); });
                triggerEvent.GetComponent<EventTrigger>().triggers.Add(entry);
            }

            if (setHighPriority)
            {
                if (contentCG == null)
                    contentCG = transform.Find("Content/Item List").GetComponent<CanvasGroup>();
                contentCG.alpha = 1;

                var tempCanvas = contentCG.gameObject.AddComponent<Canvas>();
                tempCanvas.overrideSorting = true;
                tempCanvas.sortingOrder = 30000;
                contentCG.gameObject.AddComponent<GraphicRaycaster>();
            }

            dropdownAnimator = gameObject.GetComponent<Animator>();

            if (listCG == null)
                listCG = gameObject.GetComponentInChildren<CanvasGroup>();
            if (listRect == null)
                listRect = listCG.GetComponent<RectTransform>();
            if (initAtStart && items.Count != 0)
                SetupDropdown();
            if (animationType == AnimationType.Modular && dropdownAnimator != null)
                Destroy(dropdownAnimator);

            isInitialized = true;
        }

        public void SetupDropdown()
        {
            if (!enableScrollbar && scrollbar != null)
                Destroy(scrollbar);
            if (itemList == null)
                itemList = itemParent.GetComponent<VerticalLayoutGroup>();

            UpdateItemLayout();
            index = 0;

            foreach (Transform child in itemParent)
                Destroy(child.gameObject);
            for (var i = 0; i < items.Count; ++i)
            {
                var go = Instantiate(itemObject, new Vector3(0, 0, 0), Quaternion.identity);
                go.transform.SetParent(itemParent, false);
                go.name = items[i].itemName;

                setItemText = go.GetComponentInChildren<TextMeshProUGUI>();
                textHelper = items[i].itemName;
                setItemText.text = textHelper;

                onItemTextChanged?.Invoke(setItemText);

                var goImage = go.gameObject.transform.Find("Icon");
                setItemImage = goImage.GetComponent<Image>();

                if (items[i].itemIcon == null)
                {
                    setItemImage.gameObject.SetActive(false);
                }
                else
                {
                    imageHelper = items[i].itemIcon;
                    setItemImage.sprite = imageHelper;
                }

                items[i].itemIndex = i;
                var mainItem = items[i];

                var itemButton = go.GetComponent<Button>();
                itemButton.onClick.AddListener(Animate);
                itemButton.onClick.AddListener(items[i].OnItemSelection.Invoke);
                itemButton.onClick.AddListener(delegate
                {
                    SetDropdownIndex(index = mainItem.itemIndex);
                    onValueChanged.Invoke(index = mainItem.itemIndex);
                    if (saveSelected)
                        PlayerPrefs.SetInt("Dropdown_" + saveKey, mainItem.itemIndex);
                });
            }

            if (selectedImage != null && !enableIcon)
                selectedImage.gameObject.SetActive(false);
            else if (selectedImage != null)
                selectedImage.sprite = items[selectedItemIndex].itemIcon;
            if (selectedText != null)
            {
                selectedText.text = items[selectedItemIndex].itemName;
                onItemTextChanged?.Invoke(selectedText);
            }

            if (saveSelected)
            {
                if (invokeAtStart)
                    items[PlayerPrefs.GetInt("Dropdown_" + saveKey)].OnItemSelection.Invoke();
                else
                    SetDropdownIndex(PlayerPrefs.GetInt("Dropdown_" + saveKey), false);
            }
            else if (invokeAtStart)
            {
                items[selectedItemIndex].OnItemSelection.Invoke();
            }
        }

        // Obsolete
        public void ChangeDropdownInfo (int itemIndex)
        {
            SetDropdownIndex(itemIndex);
        }

        public void SetDropdownIndex (int itemIndex)
        {
            SetDropdownIndex(itemIndex, true);
        }

        public void SetDropdownIndex (int itemIndex, bool bypassSound = false)
        {
            if (selectedImage != null && enableIcon && items[itemIndex].itemIcon != null)
            {
                selectedImage.gameObject.SetActive(true);
                selectedImage.sprite = items[itemIndex].itemIcon;
            }
            else if (selectedImage != null && enableIcon && items[itemIndex].itemIcon == null)
            {
                selectedImage.gameObject.SetActive(false);
            }

            if (selectedText != null)
            {
                selectedText.text = items[itemIndex].itemName;
                onItemTextChanged?.Invoke(selectedText);
            }

            if (!bypassSound && enableDropdownSounds && useClickSound)
                soundSource.PlayOneShot(clickSound);

            selectedItemIndex = itemIndex;
        }

        public void Animate()
        {
            if (!isOn && animationType == AnimationType.Modular)
            {
                isOn = true;
                listCG.blocksRaycasts = true;
                listCG.interactable = true;
                listCG.gameObject.SetActive(true);

                StopCoroutine("StartMinimize");
                StopCoroutine("StartExpand");
                StartCoroutine("StartExpand");
            }

            else if (isOn && animationType == AnimationType.Modular)
            {
                isOn = false;
                listCG.blocksRaycasts = false;
                listCG.interactable = false;

                StopCoroutine("StartMinimize");
                StopCoroutine("StartExpand");
                StartCoroutine("StartMinimize");
            }

            else if (!isOn && animationType == AnimationType.Custom)
            {
                dropdownAnimator.Play("Stylish In");
                isOn = true;
            }

            else if (isOn && animationType == AnimationType.Custom)
            {
                dropdownAnimator.Play("Stylish Out");
                isOn = false;
            }

            if (enableTrigger && !isOn)
                triggerObject.SetActive(false);
            else if (enableTrigger && isOn)
                triggerObject.SetActive(true);
            if (enableTrigger && outOnPointerExit)
                triggerObject.SetActive(false);
        }

        public void Interactable (bool value)
        {
            isInteractable = value;
        }

        public void CreateNewItem (string title, Sprite icon, bool notify = false)
        {
            var item = new Item
            {
                itemName = title,
                itemIcon = icon
            };
            items.Add(item);

            if (selectedItemIndex > items.Count)
                selectedItemIndex = 0;
            if (notify)
                SetupDropdown();
        }

        public void CreateNewItem (string title, bool notify = false)
        {
            var item = new Item { itemName = title };
            items.Add(item);

            if (selectedItemIndex > items.Count)
                selectedItemIndex = 0;
            if (notify)
                SetupDropdown();
        }

        public void RemoveItem (string itemTitle, bool notify = false)
        {
            var item = items.Find(x => x.itemName == itemTitle);
            items.Remove(item);

            if (selectedItemIndex > items.Count)
                selectedItemIndex = 0;
            if (notify)
                SetupDropdown();
        }

        public void UpdateItemLayout()
        {
            if (itemList == null)
                return;

            itemList.spacing = itemSpacing;
            itemList.padding.top = itemPaddingTop;
            itemList.padding.bottom = itemPaddingBottom;
            itemList.padding.left = itemPaddingLeft;
            itemList.padding.right = itemPaddingRight;
        }

        private IEnumerator StartExpand()
        {
            float elapsedTime = 0;

            var startPos = listRect.sizeDelta;
            var endPos = new Vector2(listRect.sizeDelta.x, panelSize);

            while (listRect.sizeDelta.y <= panelSize - 0.1f)
            {
                elapsedTime += Time.unscaledDeltaTime;

                listCG.alpha += Time.unscaledDeltaTime * (curveSpeed * 2);
                listRect.sizeDelta = Vector2.Lerp(startPos, endPos, animationCurve.Evaluate(elapsedTime * curveSpeed));
                yield return null;
            }

            listCG.alpha = 1;
            listRect.sizeDelta = endPos;
        }

        private IEnumerator StartMinimize()
        {
            float elapsedTime = 0;

            var startPos = listRect.sizeDelta;
            var endPos = new Vector2(listRect.sizeDelta.x, 0);

            while (listRect.sizeDelta.y >= 0.1f)
            {
                elapsedTime += Time.unscaledDeltaTime;

                listCG.alpha -= Time.unscaledDeltaTime * (curveSpeed * 2);
                listRect.sizeDelta = Vector2.Lerp(startPos, endPos, animationCurve.Evaluate(elapsedTime * curveSpeed));

                yield return null;
            }

            listCG.alpha = 0;
            listRect.sizeDelta = endPos;
            listCG.gameObject.SetActive(false);
        }

        // Events
        [Serializable]
        public class DropdownEvent : UnityEvent<int> { }

        [Serializable]
        public class ItemTextChangedEvent : UnityEvent<TMP_Text> { }

        [Serializable]
        public class Item
        {
            public string itemName = "Dropdown Item";
            public Sprite itemIcon;

            [HideInInspector]
            public int itemIndex;

            public UnityEvent OnItemSelection = new();
        }
    }
}