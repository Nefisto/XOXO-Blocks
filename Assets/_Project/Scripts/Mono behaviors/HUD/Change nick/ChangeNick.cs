using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeNick : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform root;

    [SerializeField]
    private TMP_Text titleLabel;

    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private Button confirmButton;

    [SerializeField]
    private Button closingButton;

    [SerializeField]
    private TMP_Text failedReason;

    private void Awake()
    {
        GameEvents.LobbyEvents.OpenChangeNickScreen += OpenScreenHandle;
    }

    private void OnDestroy()
    {
        GameEvents.LobbyEvents.OpenChangeNickScreen -= OpenScreenHandle;
    }

    private void OpenScreenHandle (object sender, EventArgs e)
    {
        var args = e as GameEvents.OpenChangeNickScreenEventArgs ?? new GameEvents.OpenChangeNickScreenEventArgs();

        titleLabel.text = args.title;
        Setup(args.isFirstTime);
    }

    private void Setup (bool isFirstTime)
    {
        var currentNick = PlayerPrefs.GetString(GameConstants.NICKNAME_PLAYERPREF_KEY,
            $"User#{Guid.NewGuid().ToString()[..4]}");

        ((TextMeshProUGUI)inputField.placeholder).text = currentNick;
        inputField.text = isFirstTime ? currentNick : "";
        CommonOperations.EnabledStateButton(confirmButton, inputField.text.Trim().Length > 4);

        SetupConfirmButton();
        CommonOperations.EnabledStateButton(closingButton, !isFirstTime);

        closingButton.onClick.RemoveAllListeners();
        closingButton.onClick.AddListener(CloseScreen);
        root.gameObject.SetActive(true);
    }

    private void SetupConfirmButton()
    {
        confirmButton.onClick.RemoveAllListeners();
        inputField.onValueChanged.RemoveAllListeners();

        inputField.onValueChanged.AddListener(s
            => CommonOperations.EnabledStateButton(confirmButton, IsValidName(s)));
        confirmButton.onClick.AddListener(() =>
        {
            PlayerPrefs.SetString(GameConstants.NICKNAME_PLAYERPREF_KEY, inputField.text);
            CloseScreen();
        });
    }

    private bool IsValidName (string s)
    {
        if (s.Length < GameConstants.LobbyConstants.NICKNAME_MIN_LENGTH)
        {
            failedReason.text = "* Your nickname is too short";
            return false;
        }

        if (s.Length > GameConstants.LobbyConstants.NICKNAME_MAX_LENGTH)
        {
            failedReason.text = "* Your nickname is too long";
            return false;
        }

        failedReason.text = string.Empty;
        return true;
    }

    private void CloseScreen()
    {
        GameEvents.LobbyEvents.CloseChangeNickScreen?.Invoke(this, EventArgs.Empty);
        root.gameObject.SetActive(false);
    }
}