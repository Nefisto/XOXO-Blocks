using System;
using System.Text;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientManager
{
    public async UniTask<bool> InitializeAsync (AuthKind authKind)
    {
        await UnityServices.InitializeAsync();

        var authState = await AuthenticationWrapper.DoAuth(authKind);
        return authState == AuthState.Authenticated;
    }

    public async UniTask StartClientAsync (string jointCode)
    {
        var allocation = await RelayService.Instance.JoinAllocationAsync(jointCode);

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(allocation.ToRelayServerData(GameConstants.NetworkConstants.CONNECTION_TYPE));

        await ServiceLocator.FadingManager.FadeInAsync(1f);

        NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.SceneManager.OnLoad += LoadSceneHandle;

        var userData = new UserData { userName = CommonOperations.GetUsername };
        var json = JsonUtility.ToJson(userData);
        var payloadBytes = Encoding.UTF8.GetBytes(json);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
    }

    private void LoadSceneHandle (ulong clientId, string sceneName, LoadSceneMode _, AsyncOperation asyncOperation)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId)
            return;

        if (sceneName != GameConstants.LOBBY_SCENE_NAME)
            return;

        BackToLobby();
        Shutdown();
    }

    private async void BackToLobby()
    {
        await UniTask.WaitForSeconds(0.5f);
        GameEvents.LobbyEvents.LoadedLobbyScene?.Invoke(this, EventArgs.Empty);
        await ServiceLocator.FadingManager.FadeOutAsync(1.5f);
    }

    private void Shutdown()
    {
        NetworkManager.Singleton.SceneManager.OnLoad -= LoadSceneHandle;

        NetworkManager.Singleton.Shutdown();
    }
}