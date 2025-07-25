using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameEndHud : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform root;

    [SerializeField]
    private Image backgroundImage;

    [SerializeField]
    private TMP_Text resultLabel;

    public override void OnNetworkSpawn()
    {
        GameEvents.GameplayEvents.OnGameEnd += GameEndHandle;
        root.gameObject.SetActive(false);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        GameEvents.GameplayEvents.OnGameEnd -= GameEndHandle;
    }

    private void GameEndHandle (PlayerSide winner)
    {
        root.gameObject.SetActive(true);
        var amITheWinner = CommonOperations.GetPlayerRefOf(winner).IsOwner;
        StartCoroutine(Routine());

        IEnumerator Routine()
        {
            if (amITheWinner)
                SetupWin();
            else
                SetupLose();

            yield break;
        }
    }

    private void SetupWin()
    {
        backgroundImage.color = new Color(0, 1, 0, 0.5f);
        resultLabel.text = "!!! You won !!!";
    }

    private void SetupLose()
    {
        backgroundImage.color = new Color(1, 0, 0, 0.5f);
        resultLabel.text = "!!! You lost !!!";
    }
}