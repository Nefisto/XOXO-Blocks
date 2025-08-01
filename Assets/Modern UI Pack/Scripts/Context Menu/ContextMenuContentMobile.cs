﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [AddComponentMenu("Modern UI Pack/Context Menu/Context Menu Content (Mobile)")]
    public class ContextMenuContentMobile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public enum ContextItemType
        {
            BUTTON
            // SUB_MENU
        }

        [Header("Resources")]
        public ContextMenuManager contextManager;

        public Transform itemParent;

        [Header("Settings")]
        [Range(0.1f, 6)]
        public float holdToOpen = 0.75f;

        [Header("Items")]
        public List<ContextItem> contexItems = new();

        private Animator contextAnimator;
        private Sprite imageHelper;
        private GameObject selectedItem;
        private Image setItemImage;
        private TextMeshProUGUI setItemText;
        private string textHelper;
        private float timer;
        private bool timerEnabled;

        private void Start()
        {
            if (contextManager == null)
                try
                {
                    contextManager = GameObject.Find("Context Menu").GetComponent<ContextMenuManager>();
                    itemParent = contextManager.transform.Find("Content/Item List").transform;
                }

                catch
                {
                    Debug.Log("<b>[Context Menu]</b> Context Manager is missing.", this);
                    return;
                }

            contextAnimator = contextManager.contextAnimator;

            foreach (Transform child in itemParent)
                Destroy(child.gameObject);
        }

        private void Update()
        {
            if (timerEnabled)
            {
                timer += Time.deltaTime;

                if (timer >= holdToOpen)
                {
                    CheckForTimer();
                    timerEnabled = false;
                    timer = 0;
                }
            }
        }

        public void OnPointerDown (PointerEventData eventData)
        {
            timerEnabled = true;
        }

        public void OnPointerUp (PointerEventData eventData)
        {
            timerEnabled = false;
            timer = 0;
        }

        public void CheckForTimer()
        {
            if (timer <= holdToOpen)
                return;

            if (contextManager.isOn)
            {
                contextAnimator.Play("Menu Out");
                contextManager.isOn = false;
            }

            else if (contextManager.isOn == false)
            {
                foreach (Transform child in itemParent)
                    Destroy(child.gameObject);

                for (var i = 0; i < contexItems.Count; ++i)
                {
                    if (contexItems[i].contextItemType == ContextItemType.BUTTON)
                        selectedItem = contextManager.contextButton;

                    var go = Instantiate(selectedItem, new Vector3(0, 0, 0), Quaternion.identity);
                    go.transform.SetParent(itemParent, false);

                    setItemText = go.GetComponentInChildren<TextMeshProUGUI>();
                    textHelper = contexItems[i].itemText;
                    setItemText.text = textHelper;

                    Transform goImage;
                    goImage = go.gameObject.transform.Find("Icon");
                    setItemImage = goImage.GetComponent<Image>();
                    imageHelper = contexItems[i].itemIcon;
                    setItemImage.sprite = imageHelper;

                    if (imageHelper == null)
                        setItemImage.color = new Color(0, 0, 0, 0);

                    Button itemButton;
                    itemButton = go.GetComponent<Button>();
                    itemButton.onClick.AddListener(contexItems[i].onClick.Invoke);
                    itemButton.onClick.AddListener(CloseOnClick);
                    StartCoroutine(ExecuteAfterTime(0.01f));
                }

                contextManager.SetContextMenuPosition();
                contextAnimator.Play("Menu In");
                contextManager.isOn = true;
                contextManager.SetContextMenuPosition();
            }
        }

        private IEnumerator ExecuteAfterTime (float time)
        {
            yield return new WaitForSeconds(time);
            itemParent.gameObject.SetActive(false);
            itemParent.gameObject.SetActive(true);
            StopCoroutine(ExecuteAfterTime(0.01f));
            StopCoroutine("ExecuteAfterTime");
        }

        public void CloseOnClick()
        {
            contextAnimator.Play("Menu Out");
            contextManager.isOn = false;
        }

        [Serializable]
        public class ContextItem
        {
            public string itemText = "Item Text";
            public Sprite itemIcon;
            public ContextItemType contextItemType;
            public UnityEvent onClick;
        }
    }
}