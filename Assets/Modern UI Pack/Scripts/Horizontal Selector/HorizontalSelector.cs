﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [RequireComponent(typeof(Animator))]
    public class HorizontalSelector : MonoBehaviour
    {
        // Resources
        public TextMeshProUGUI label;
        public TextMeshProUGUI labelHelper;
        public Image labelIcon;
        public Image labelIconHelper;
        public Transform indicatorParent;
        public GameObject indicatorObject;
        public Animator selectorAnimator;
        public HorizontalLayoutGroup contentLayout;
        public HorizontalLayoutGroup contentLayoutHelper;

        // Saving
        public bool enableIcon = true;
        public bool saveSelected;
        public string saveKey = "My Selector";

        // Settings
        public bool enableIndicators = true;
        public bool invokeAtStart;
        public bool invertAnimation;
        public bool loopSelection;

        [Range(0.25f, 2.5f)]
        public float iconScale = 1;

        [Range(1, 50)]
        public int contentSpacing = 15;

        public int defaultIndex;

        [HideInInspector]
        public int index;

        // Items
        public List<Item> items = new();
        public SelectorEvent onValueChanged;
        public ItemTextChangedEvent onItemTextChanged;
        private string newItemTitle;

        private void Awake()
        {
            if (selectorAnimator == null)
                selectorAnimator = gameObject.GetComponent<Animator>();
            if (label == null || labelHelper == null)
            {
                Debug.LogError("<b>[Horizontal Selector]</b> Cannot initalize the object due to missing resources.",
                    this);
                return;
            }

            SetupSelector();
            UpdateContentLayout();

            if (invokeAtStart)
            {
                items[index].onItemSelect.Invoke();
                onValueChanged.Invoke(index);
            }
        }

        private void OnEnable()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine("DisableAnimator");
        }

        public void SetupSelector()
        {
            if (items.Count == 0)
                return;

            if (saveSelected)
            {
                if (PlayerPrefs.HasKey("HorizontalSelector_" + saveKey))
                    defaultIndex = PlayerPrefs.GetInt("HorizontalSelector_" + saveKey);
                else
                    PlayerPrefs.SetInt("HorizontalSelector_" + saveKey, defaultIndex);
            }

            label.text = items[defaultIndex].itemTitle;
            labelHelper.text = label.text;
            onItemTextChanged?.Invoke(label);

            if (labelIcon != null && enableIcon)
            {
                labelIcon.sprite = items[defaultIndex].itemIcon;
                labelIconHelper.sprite = labelIcon.sprite;
            }

            else if (!enableIcon)
            {
                if (labelIcon != null)
                    labelIcon.gameObject.SetActive(false);
                if (labelIconHelper != null)
                    labelIconHelper.gameObject.SetActive(false);
            }

            index = defaultIndex;

            if (enableIndicators)
                UpdateIndicators();
            else if (indicatorParent != null)
                Destroy(indicatorParent.gameObject);
        }

        public void PreviousItem()
        {
            if (items.Count == 0)
                return;

            StopCoroutine("DisableAnimator");
            selectorAnimator.enabled = true;

            if (!loopSelection)
            {
                if (index != 0)
                {
                    labelHelper.text = label.text;
                    if (labelIcon != null && enableIcon)
                        labelIconHelper.sprite = labelIcon.sprite;

                    if (index == 0)
                        index = items.Count - 1;
                    else
                        index--;

                    label.text = items[index].itemTitle;
                    onItemTextChanged?.Invoke(label);
                    if (labelIcon != null && enableIcon)
                        labelIcon.sprite = items[index].itemIcon;

                    items[index].onItemSelect.Invoke();
                    onValueChanged.Invoke(index);

                    selectorAnimator.Play(null);
                    selectorAnimator.StopPlayback();

                    if (invertAnimation)
                        selectorAnimator.Play("Forward");
                    else
                        selectorAnimator.Play("Previous");
                }
            }

            else
            {
                labelHelper.text = label.text;
                if (labelIcon != null && enableIcon)
                    labelIconHelper.sprite = labelIcon.sprite;

                if (index == 0)
                    index = items.Count - 1;
                else
                    index--;

                label.text = items[index].itemTitle;
                onItemTextChanged?.Invoke(label);
                if (labelIcon != null && enableIcon)
                    labelIcon.sprite = items[index].itemIcon;

                items[index].onItemSelect.Invoke();
                onValueChanged.Invoke(index);

                selectorAnimator.Play(null);
                selectorAnimator.StopPlayback();

                if (invertAnimation)
                    selectorAnimator.Play("Forward");
                else
                    selectorAnimator.Play("Previous");
            }

            if (saveSelected)
                PlayerPrefs.SetInt("HorizontalSelector_" + saveKey, index);
            if (gameObject.activeInHierarchy)
                StartCoroutine("DisableAnimator");
            if (enableIndicators)
                for (var i = 0; i < items.Count; ++i)
                {
                    var go = indicatorParent.GetChild(i).gameObject;
                    var onObj = go.transform.Find("On");
                    var offObj = go.transform.Find("Off");

                    if (i == index)
                    {
                        onObj.gameObject.SetActive(true);
                        offObj.gameObject.SetActive(false);
                    }
                    else
                    {
                        onObj.gameObject.SetActive(false);
                        offObj.gameObject.SetActive(true);
                    }
                }
        }

        public void NextItem()
        {
            if (items.Count == 0)
                return;

            StopCoroutine("DisableAnimator");
            selectorAnimator.enabled = true;

            if (!loopSelection)
            {
                if (index != items.Count - 1)
                {
                    labelHelper.text = label.text;
                    if (labelIcon != null && enableIcon)
                        labelIconHelper.sprite = labelIcon.sprite;

                    if (index + 1 >= items.Count)
                        index = 0;
                    else
                        index++;

                    label.text = items[index].itemTitle;
                    onItemTextChanged?.Invoke(label);
                    if (labelIcon != null && enableIcon)
                        labelIcon.sprite = items[index].itemIcon;

                    items[index].onItemSelect.Invoke();
                    onValueChanged.Invoke(index);

                    selectorAnimator.Play(null);
                    selectorAnimator.StopPlayback();

                    if (invertAnimation)
                        selectorAnimator.Play("Previous");
                    else
                        selectorAnimator.Play("Forward");
                }
            }

            else
            {
                labelHelper.text = label.text;
                if (labelIcon != null && enableIcon)
                    labelIconHelper.sprite = labelIcon.sprite;

                if (index + 1 >= items.Count)
                    index = 0;
                else
                    index++;

                label.text = items[index].itemTitle;
                onItemTextChanged?.Invoke(label);
                if (labelIcon != null && enableIcon)
                    labelIcon.sprite = items[index].itemIcon;

                items[index].onItemSelect.Invoke();
                onValueChanged.Invoke(index);

                selectorAnimator.Play(null);
                selectorAnimator.StopPlayback();

                if (invertAnimation)
                    selectorAnimator.Play("Previous");
                else
                    selectorAnimator.Play("Forward");
            }

            if (saveSelected)
                PlayerPrefs.SetInt("HorizontalSelector_" + saveKey, index);
            if (enableIndicators)
                for (var i = 0; i < items.Count; ++i)
                {
                    var go = indicatorParent.GetChild(i).gameObject;
                    var onObj = go.transform.Find("On");
                    ;
                    var offObj = go.transform.Find("Off");

                    if (i == index)
                    {
                        onObj.gameObject.SetActive(true);
                        offObj.gameObject.SetActive(false);
                    }
                    else
                    {
                        onObj.gameObject.SetActive(false);
                        offObj.gameObject.SetActive(true);
                    }
                }

            if (gameObject.activeInHierarchy)
                StartCoroutine("DisableAnimator");
        }

        // Obsolete
        public void PreviousClick()
        {
            PreviousItem();
        }

        public void ForwardClick()
        {
            NextItem();
        }

        public void CreateNewItem (string title)
        {
            var item = new Item();
            newItemTitle = title;
            item.itemTitle = newItemTitle;
            items.Add(item);
        }

        public void CreateNewItem (string title, Sprite icon)
        {
            var item = new Item();
            newItemTitle = title;
            item.itemTitle = newItemTitle;
            item.itemIcon = icon;
            items.Add(item);
        }

        public void RemoveItem (string itemTitle)
        {
            var item = items.Find(x => x.itemTitle == itemTitle);
            items.Remove(item);
            SetupSelector();
        }

        public void UpdateUI()
        {
            selectorAnimator.enabled = true;

            label.text = items[index].itemTitle;
            onItemTextChanged?.Invoke(label);

            if (labelIcon != null && enableIcon)
                labelIcon.sprite = items[index].itemIcon;
            if (gameObject.activeInHierarchy)
                StartCoroutine("DisableAnimator");

            UpdateContentLayout();
            UpdateIndicators();
        }

        public void UpdateIndicators()
        {
            if (!enableIndicators)
                return;

            foreach (Transform child in indicatorParent)
                Destroy(child.gameObject);
            for (var i = 0; i < items.Count; ++i)
            {
                var go = Instantiate(indicatorObject, new Vector3(0, 0, 0), Quaternion.identity);
                go.transform.SetParent(indicatorParent, false);
                go.name = items[i].itemTitle;

                var onObj = go.transform.Find("On");
                var offObj = go.transform.Find("Off");

                if (i == index)
                {
                    onObj.gameObject.SetActive(true);
                    offObj.gameObject.SetActive(false);
                }
                else
                {
                    onObj.gameObject.SetActive(false);
                    offObj.gameObject.SetActive(true);
                }
            }
        }

        public void UpdateContentLayout()
        {
            if (contentLayout != null)
                contentLayout.spacing = contentSpacing;
            if (contentLayoutHelper != null)
                contentLayoutHelper.spacing = contentSpacing;
            if (labelIcon != null)
            {
                labelIcon.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
                labelIconHelper.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(label.transform.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(label.transform.parent.GetComponent<RectTransform>());
        }

        private IEnumerator DisableAnimator()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            selectorAnimator.enabled = false;
        }

        // Events
        [Serializable]
        public class SelectorEvent : UnityEvent<int> { }

        [Serializable]
        public class ItemTextChangedEvent : UnityEvent<TMP_Text> { }

        [Serializable]
        public class Item
        {
            public string itemTitle = "Item Title";
            public Sprite itemIcon;
            public UnityEvent onItemSelect = new();
        }
    }
}