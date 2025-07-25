using System;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyEntry : MonoBehaviour
{
    private const string GridSizeTemplate = "Grid size: {0}x{0}";

    [TitleGroup("References")]
    [SerializeField]
    private TMP_Text lobbyNameLabel;

    [SerializeField]
    private TMP_Text lobbyCodeLabel;

    [SerializeField]
    private TMP_Text lobbyGridSize;

    [TitleGroup("References")]
    [SerializeField]
    private Button connectButton;

    [SerializeField]
    private GameEntryIcon stickyBombIcon;

    [SerializeField]
    private GameEntryIcon draftDrawIcon;

    private void OnDestroy() => connectButton.onClick.RemoveAllListeners();

    public void Setup (Lobby lobby, Action onEnterLobbyCallback = null)
    {
        var joinCode = lobby.Data[GameConstants.NetworkConstants.LobbyConstants.JOIN_CODE].Value;
        var gridSize = lobby.Data[GameConstants.NetworkConstants.LobbyConstants.GRID_SIZE].Value;
        var stickyBomb = bool.Parse(lobby.Data[GameConstants.NetworkConstants.LobbyConstants.STICKY_BOMB_MODE].Value);
        var draftDraw = bool.Parse(lobby.Data[GameConstants.NetworkConstants.LobbyConstants.DRAFT_DRAW_MODE].Value);

        lobbyNameLabel.text = $"{lobby.Name}";
        lobbyCodeLabel.text = $"Code: {joinCode}";
        lobbyGridSize.text = string.Format(GridSizeTemplate, gridSize);
        if (stickyBomb)
            stickyBombIcon.EnableEntry();
        else
            stickyBombIcon.DisableEntry();

        if (draftDraw)
            draftDrawIcon.EnableEntry();
        else
            draftDrawIcon.DisableEntry();

        connectButton.onClick.AddListener(() => onEnterLobbyCallback?.Invoke());
    }

    public void DisableEntry() => connectButton.interactable = false;
}