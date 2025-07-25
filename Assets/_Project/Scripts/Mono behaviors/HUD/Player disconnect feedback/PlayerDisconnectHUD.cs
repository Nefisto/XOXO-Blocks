using System;
using System.Collections;
using UnityEngine;

public class PlayerDisconnectHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform root;

    private void Awake()
    {
        GameEvents.GameplayEvents.OnClientDisconnected += PlayerDisconnectHandle;

        root.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameEvents.GameplayEvents.OnClientDisconnected -= PlayerDisconnectHandle;
    }

    private IEnumerator PlayerDisconnectHandle (object arg1, EventArgs arg2)
    {
        root.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
    }
}