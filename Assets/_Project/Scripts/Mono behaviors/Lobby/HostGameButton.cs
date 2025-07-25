using System;
using Ricimi;
using UnityEngine;
using UnityEngine.UI;

public class HostGameButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Button hostButton;

    [SerializeField]
    private MyPopupOpener gameSettingPopup;

    private void Awake()
    {
        hostButton.onClick.AddListener(HostGameHandle);

        GameEvents.LobbyEvents.OnTryingMoveToGame += TryingToCreateGameHandle;
        GameEvents.LobbyEvents.OnFailedToJoinGame += FailedToJoinHandle;
    }

    private void OnDestroy()
    {
        hostButton.onClick.RemoveAllListeners();
        GameEvents.LobbyEvents.OnTryingMoveToGame -= TryingToCreateGameHandle;
        GameEvents.LobbyEvents.OnFailedToJoinGame -= FailedToJoinHandle;
    }

    private void FailedToJoinHandle (object sender, EventArgs e)
    {
        CommonOperations.EnabledStateButton(hostButton, true);
    }

    private void TryingToCreateGameHandle (object sender, EventArgs e)
    {
        hostButton.interactable = false;
        hostButton.GetComponentInChildren<ColorSwapper>().SwapColor();
    }

    private void HostGameHandle() => gameSettingPopup.OpenPopup();
}