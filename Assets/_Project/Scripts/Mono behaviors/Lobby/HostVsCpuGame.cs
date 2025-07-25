public class HostVsCpuGame : HostGame
{
    protected override async void HostGameHandle()
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

        await ServiceLocator.Network.HostManager.StartSinglePlayerHostAsync(new GameSettings
        {
            BoardSize = gridSize,
            StickyBomb = stickyBomb,
            DraftDraw = draftMode
        });
    }
}