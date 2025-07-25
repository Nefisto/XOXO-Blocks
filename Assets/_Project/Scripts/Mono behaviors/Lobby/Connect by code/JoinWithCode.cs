using System;
using Ricimi;
using TMPro;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.UI;

public class JoinWithCode : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Button requestButton;

    [SerializeField]
    private TMP_InputField inputCode;

    [SerializeField]
    private PopupOpener failToFindModalPopup;

    private void Awake()
    {
        requestButton.onClick.AddListener(ConnectHandle);

        GameEvents.LobbyEvents.OnTryingMoveToGame += TryingToCreateGameHandle;
        GameEvents.LobbyEvents.OnFailedToJoinGame += FailedToJoinHandle;
    }

    private void Start()
    {
        CommonOperations.EnabledStateButton(requestButton, false);
        inputCode.onValueChanged.AddListener(ValueChangedHandle);
    }

    private void OnDestroy()
    {
        GameEvents.LobbyEvents.OnTryingMoveToGame -= TryingToCreateGameHandle;
        GameEvents.LobbyEvents.OnFailedToJoinGame -= FailedToJoinHandle;

        inputCode.onValueChanged.RemoveAllListeners();
    }

    private void FailedToJoinHandle (object sender, EventArgs e)
    {
        CommonOperations.EnabledStateButton(requestButton, true);
    }

    private void ValueChangedHandle (string text)
    {
        CommonOperations.EnabledStateButton(requestButton, text.Length > 4);
    }

    private void TryingToCreateGameHandle (object sender, EventArgs e)
    {
        requestButton.interactable = false;
        requestButton.GetComponentInChildren<ColorSwapper>().SwapColor();
    }

    private async void ConnectHandle()
    {
        try
        {
            GameEvents.LobbyEvents.OnTryingMoveToGame?.Invoke(this, EventArgs.Empty);
            await ServiceLocator.Network.ClientManager.StartClientAsync(inputCode.text);
        }
        catch (RelayServiceException)
        {
            failToFindModalPopup.OpenPopup();
            GameEvents.LobbyEvents.OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
        }
    }
}