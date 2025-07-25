using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostGame : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    protected GameButton createGameButton;

    [SerializeField]
    protected TMP_Dropdown gridSizeOptions;

    [SerializeField]
    protected Toggle stickyBombToggle;

    [SerializeField]
    protected Toggle draftDrawToggle;

    private void Awake() => createGameButton.onClick.AddListener(HostGameHandle);

    private void OnDestroy() => createGameButton.onClick.RemoveAllListeners();

    protected virtual async void HostGameHandle()
    {
        createGameButton.DisableButton();

        var gridSize = gridSizeOptions
            .options[gridSizeOptions.value]
            .text
            .Contains("3")
            ? 3
            : 4;

        var stickyBomb = stickyBombToggle.isOn;
        var draftMode = draftDrawToggle.isOn;

        await ServiceLocator.Network.HostManager.StartHostAsync(new GameSettings
        {
            BoardSize = gridSize,
            StickyBomb = stickyBomb,
            DraftDraw = draftMode
        });
    }
}