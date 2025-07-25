using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class CancelLobby : MonoBehaviour
{
    [TitleGroup("References")]
    [SerializeField]
    private GameButton cancelButton;

    private void Awake()
    {
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(CancelBehavior);

        GameEvents.GameplayEvents.AllPlayersConnected += AllPlayersConnectedHandle;
        GameEvents.WaitingRoom.OnClosingWaitingRoom += ClosingScreenHandle;
    }

    private void OnDestroy()
    {
        GameEvents.GameplayEvents.AllPlayersConnected -= AllPlayersConnectedHandle;
        GameEvents.WaitingRoom.OnClosingWaitingRoom -= ClosingScreenHandle;
    }

    private IEnumerator AllPlayersConnectedHandle (object arg1, EventArgs arg2)
    {
        cancelButton.DisableButton();
        yield break;
    }

    private IEnumerator ClosingScreenHandle (object arg1, EventArgs arg2)
    {
        cancelButton.DisableButton();

        cancelButton.FadOut();
        yield break;
    }

    private async void CancelBehavior()
    {
        cancelButton.DisableButton();
        await ServiceLocator.ApplicationController.MoveToScene(GameConstants.LOBBY_SCENE_NAME,
            new ApplicationController.MoveToSceneSettings
            {
                BeforeFadeOut = () =>
                {
                    ServiceLocator.Network.HostManager.Shutdown();
                    return UniTask.CompletedTask;
                }
            });
    }
}