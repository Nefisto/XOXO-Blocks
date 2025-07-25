using System;
using System.Collections;
using System.Collections.Generic;
using NTools;
using Unity.Netcode;
using UnityEngine;

public class EmojiManager : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private float gapBetweenEmojis = .2f;

    [SerializeField]
    private float disabledButtonDuration = 3f;

    [Header("References")]
    [SerializeField]
    private GameButton buttonToOpenListOfEmojis;

    [SerializeField]
    private List<EmojiData> possibleEmojis;

    [SerializeField]
    private EmojiEntry prefab;

    [SerializeField]
    private Transform rootForList;

    private readonly List<EmojiEntry> emojiList = new();

    private bool isOpen;
    private NTask openingTask;
    public Action<EmojiData> OnRequestEmojiSend { get; set; }

    public override void OnNetworkSpawn()
    {
        GameEvents.GameplayEvents.OnSetupReferencesNewGame += OnSetupReferencesHandle;
        GameEvents.GameplayEvents.OnSetupNewGame += SetupNewGameHandle;
    }

    public override void OnNetworkDespawn()
    {
        GameEvents.GameplayEvents.OnSetupReferencesNewGame -= OnSetupReferencesHandle;
        GameEvents.GameplayEvents.OnSetupNewGame -= SetupNewGameHandle;
    }

    private IEnumerator OnSetupReferencesHandle (object arg1, EventArgs arg2)
    {
        ServiceLocator.GameReferences.EmojiManager = this;
        yield break;
    }

    private IEnumerator SetupNewGameHandle (object arg1, EventArgs arg2)
    {
        buttonToOpenListOfEmojis.onClick.RemoveAllListeners();
        buttonToOpenListOfEmojis.onClick.AddListener(ToggleEmojiList);
        CreateListOfEmojis();

        yield break;
    }

    private void ToggleEmojiList()
    {
        if (isOpen)
            CloseEmojiList();
        else
            OpenEmojiList();
    }

    private void OpenEmojiList()
    {
        isOpen = true;
        openingTask = new NTask(Routine());

        IEnumerator Routine()
        {
            var yieldSecond = new WaitForSeconds(gapBetweenEmojis);
            foreach (Transform child in rootForList)
            {
                child.gameObject.SetActive(true);
                yield return yieldSecond;
            }
        }
    }

    private void CloseEmojiList()
    {
        openingTask?.Stop();
        openingTask = null;
        isOpen = false;

        foreach (Transform child in rootForList)
            child.gameObject.SetActive(false);
    }

    private void CreateListOfEmojis()
    {
        rootForList.DestroyChildren();

        foreach (var emoji in possibleEmojis)
        {
            var instance = Instantiate(prefab, rootForList);
            instance.Setup(emoji, () =>
            {
                StartCoroutine(ButtonCooldown());
                OnRequestEmojiSend?.Invoke(emoji);
                CloseEmojiList();
            });

            emojiList.Add(instance);
        }

        CloseEmojiList();
    }

    private IEnumerator ButtonCooldown()
    {
        buttonToOpenListOfEmojis.DisableButton();

        yield return new WaitForSeconds(disabledButtonDuration);

        buttonToOpenListOfEmojis.EnableButton();
    }
}