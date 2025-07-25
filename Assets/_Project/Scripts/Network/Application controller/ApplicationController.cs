using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationController : MonoBehaviour
{
    [field: Header("References")]
    [field: SerializeField]
    public Player PlayerPrefab { get; set; }

    [field: SerializeField]
    public Player BotPrefab { get; set; }

    private async void Start()
    {
        ServiceLocator.ApplicationController = this;
        GameEvents.NetworkEvents.LoggingAsGuest += (_, _) => Launch(AuthKind.Anonymously).Forget();

        await ServiceLocator.FadingManager.FadeOutAsync(1f);

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR
        Launch(AuthKind.Anonymously).Forget();
#endif
    }

    private async UniTask Launch (AuthKind authKind)
    {
        var clientManager = new ClientManager();
        var isAuthenticated = await clientManager.InitializeAsync(authKind);

        var hostManager = new HostManager();

        ServiceLocator.Network.ClientManager = clientManager;
        ServiceLocator.Network.HostManager = hostManager;

        if (!isAuthenticated)
            return;

        MoveToScene(GameConstants.LOBBY_SCENE_NAME, new MoveToSceneSettings
            {
                BeforeFadeOut = () =>
                {
                    GameEvents.LobbyEvents.LoadedLobbyScene?.Invoke(this, EventArgs.Empty);

                    if (!PlayerPrefs.HasKey(GameConstants.NICKNAME_PLAYERPREF_KEY))
                        GameEvents.LobbyEvents.OpenChangeNickScreen?.Invoke(this,
                            new GameEvents.OpenChangeNickScreenEventArgs
                            {
                                title = "Type a beautiful nick \\o/",
                                isFirstTime = true
                            });

                    return UniTask.CompletedTask;
                }
            })
            .Forget();
    }

    public async UniTask MoveToScene (string sceneName, MoveToSceneSettings settings = null)
    {
        settings ??= new MoveToSceneSettings();

        await ServiceLocator.FadingManager.FadeInAsync(settings.fadeInDuration);
        await SceneManager.LoadSceneAsync(sceneName);

        await (settings.BeforeFadeOut?.Invoke() ?? UniTask.CompletedTask);
        await ServiceLocator.FadingManager.FadeOutAsync(settings.fadeOutDuration);
        await (settings.AfterFadeOut?.Invoke() ?? UniTask.CompletedTask);
    }

    public class MoveToSceneSettings
    {
        public float fadeInDuration = 1f;
        public float fadeOutDuration = 1f;

        public Func<UniTask> BeforeFadeOut { get; set; }
        public Func<UniTask> AfterFadeOut { get; set; }
    }
}