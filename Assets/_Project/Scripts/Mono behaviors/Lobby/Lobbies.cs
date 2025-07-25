using System;
using System.Collections;
using System.Collections.Generic;
using NTools;
using Sirenix.OdinInspector;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class Lobbies : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform lobbiesFolder;

    [SerializeField]
    private LobbyEntry lobbyEntry;

    private NTask fetchTask;

    private bool isJoining;

    private void Awake()
    {
        lobbiesFolder.DestroyChildren();

        fetchTask = new NTask(FetchRoutine());
        GameEvents.LobbyEvents.OnTryingMoveToGame += OnTryingToCreateGameHandle;
        GameEvents.LobbyEvents.OnFailedToJoinGame += FailedToJoinGame;
    }

    private void OnDestroy()
    {
        fetchTask?.Stop();
        fetchTask = null;

        GameEvents.LobbyEvents.OnTryingMoveToGame -= OnTryingToCreateGameHandle;
        GameEvents.LobbyEvents.OnFailedToJoinGame -= FailedToJoinGame;
    }

    private void FailedToJoinGame (object sender, EventArgs e)
    {
        lobbiesFolder.DestroyChildren();

        fetchTask?.Resume();
    }

    private void OnTryingToCreateGameHandle (object sender, EventArgs e)
    {
        fetchTask?.Stop();

        foreach (var componentsInChild in lobbiesFolder.GetComponentsInChildren<LobbyEntry>())
            componentsInChild.DisableEntry();
    }

    private IEnumerator FetchRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(GameConstants.NetworkConstants.GAP_BETWEEN_FETCH_LOBBIES);

            FetchLobbies();
        }
    }

    [Button]
    [DisableInEditorMode]
    private async void FetchLobbies()
    {
        var options = new QueryLobbiesOptions
        {
            Filters = new List<QueryFilter>
            {
                new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            }
        };

        var queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);
        UpdateLobbies(queryResponse.Results);
    }

    private void UpdateLobbies (List<Lobby> lobbies)
    {
        lobbiesFolder.DestroyChildren();

        foreach (var lobby in lobbies)
        {
            var instance = Instantiate(lobbyEntry, lobbiesFolder, false);
            instance.Setup(lobby, () => JoinLobbyAsync(lobby));
        }
    }

    private async void JoinLobbyAsync (Lobby lobby)
    {
        if (isJoining)
            return;

        GameEvents.LobbyEvents.OnTryingMoveToGame?.Invoke(this, EventArgs.Empty);

        isJoining = true;

        var joinCode = lobby.Data["JoinCode"].Value;
        await ServiceLocator.Network.ClientManager.StartClientAsync(joinCode);
        isJoining = false;
    }
}