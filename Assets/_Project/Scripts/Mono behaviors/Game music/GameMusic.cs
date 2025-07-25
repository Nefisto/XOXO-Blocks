using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameMusic : MonoBehaviour
{
    [TitleGroup("Settings")]
    [SerializeField]
    private AudioSource gameMusic;

    private void Awake()
    {
        GameEvents.GameplayEvents.OnStartingNewGame += NewGameHandle;
        GameEvents.GameplayEvents.OnGameEnd += GameEndHande;
    }

    private void OnDestroy()
    {
        GameEvents.GameplayEvents.OnStartingNewGame -= NewGameHandle;
        GameEvents.GameplayEvents.OnGameEnd -= GameEndHande;
    }

    private void GameEndHande (PlayerSide _)
    {
        gameMusic.Stop();
    }

    private IEnumerator NewGameHandle (object arg1, EventArgs arg2)
    {
        gameMusic.Play();
        yield break;
    }
}