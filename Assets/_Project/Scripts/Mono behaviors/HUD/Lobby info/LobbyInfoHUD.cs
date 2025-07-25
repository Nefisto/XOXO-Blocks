using System;
using System.Collections;
using DG.Tweening;
using NTools;
using TMPro;
using UnityEngine;
using Object = System.Object;

public class LobbyInfoHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private TMP_Text lobbyIdLabel;

    [SerializeField]
    private GameButton copyButton;

    [SerializeField]
    private Transform root;

    private void Awake()
    {
        DisableRoomInfo();

        ServiceLocator.Network.LobbyInfoHud = this;

#if UNITY_WEBGL && !UNITY_EDITOR
        copyButton.gameObject.SetActive(false);
#endif
        copyButton.onClick.RemoveAllListeners();
        copyButton.onClick.AddListener(() => GUIUtility.systemCopyBuffer = lobbyIdLabel.text);

        GameEvents.WaitingRoom.OnClosingWaitingRoom += OnClosingWaitingRoomHandle;
    }

    private void OnDestroy() => GameEvents.WaitingRoom.OnClosingWaitingRoom -= OnClosingWaitingRoomHandle;

    private IEnumerator OnClosingWaitingRoomHandle (Object caller, EventArgs args)
    {
        root
            .GetComponentsInChildren<TMP_Text>()
            .ForEach(t => t.DOFade(0, 2f));
        yield return new WaitForSeconds(2f);
        DisableRoomInfo();
    }

    public void Setup (string joinCode) => lobbyIdLabel.text = $"{joinCode}";

    public void EnableRoomInfo()
    {
        root.gameObject.SetActive(true);

        root
            .GetComponentsInChildren<TMP_Text>()
            .ForEach(t => t.DOFade(1, 0));
    }

    public void DisableRoomInfo() => root.gameObject.SetActive(false);
}