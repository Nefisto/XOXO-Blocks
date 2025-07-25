using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class GameVersionHUD : MonoBehaviour
{
    private static GameVersionHUD instance;

    [TitleGroup("References")]
    [SerializeField]
    private TMP_Text versionLabel;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        versionLabel.text = $"version {Application.version}";
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }
}