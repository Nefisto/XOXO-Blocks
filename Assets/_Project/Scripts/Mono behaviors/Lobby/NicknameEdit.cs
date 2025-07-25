using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NicknameEdit : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Button editButton;

    [SerializeField]
    private TMP_Text nickLabel;

    private void Awake()
    {
        editButton.onClick.RemoveAllListeners();
        editButton.onClick.AddListener(OpenChangeNickScreen);

        GameEvents.LobbyEvents.LoadedLobbyScene += LoadSceneHandle;
        GameEvents.LobbyEvents.CloseChangeNickScreen += CloseChangeNickScreenHandle;
    }

    private void OnDestroy()
    {
        GameEvents.LobbyEvents.LoadedLobbyScene -= LoadSceneHandle;
        GameEvents.LobbyEvents.CloseChangeNickScreen -= CloseChangeNickScreenHandle;
    }

    private void LoadSceneHandle (object sender, EventArgs e)
    {
        nickLabel.text = PlayerPrefs.GetString(GameConstants.NICKNAME_PLAYERPREF_KEY, "-");
    }

    private void CloseChangeNickScreenHandle (object sender, EventArgs e)
    {
        nickLabel.text = PlayerPrefs.GetString(GameConstants.NICKNAME_PLAYERPREF_KEY);
    }

    public void OpenChangeNickScreen()
    {
        GameEvents.LobbyEvents.OpenChangeNickScreen?.Invoke(this,
            new GameEvents.OpenChangeNickScreenEventArgs { title = "Select a new nick!" });
    }
}