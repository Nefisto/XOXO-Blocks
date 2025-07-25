using System.Collections;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PieceSpawnManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private Piece piecePrefab;

    //Clean: Move it to addressables
    [SerializeField]
    private PieceDatabase pieceDatabase;

    public override void OnNetworkSpawn()
    {
        ServiceLocator.GameReferences.PieceSpawnManager = this;
        ServiceLocator.Database.PieceDatabase = pieceDatabase;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnPieceServerRpc (PlayerSide playerSide, FixedString32Bytes requestedPieceCode,
        ulong clientId, bool intoMainHand, bool changeOwnership = true)
    {
        if (!IsServer)
        {
            Debug.LogError("Requesting piece spawn for a client instead of server");
            return;
        }

        StartCoroutine(Routine());

        IEnumerator Routine()
        {
            var instance = Instantiate(piecePrefab, new Vector3(0f, -10f, 0f), Quaternion.identity, null);
            var networkObject = instance.GetComponent<NetworkObject>();

            networkObject.Spawn(true);
            yield return new WaitUntil(() => networkObject.IsSpawned);

            SetupPieceClientRpc(playerSide, requestedPieceCode, networkObject.NetworkObjectId);
            if (changeOwnership)
                networkObject.ChangeOwnership(clientId);

            NotifySpawnedPieceClientRpc(playerSide, networkObject.NetworkObjectId, intoMainHand);
        }
    }

    #region Client RPCs

    [ClientRpc]
    private void SetupPieceClientRpc (PlayerSide playerSide, FixedString32Bytes requestedPieceCode,
        ulong spawnedPieceId)
    {
        var pieceName = requestedPieceCode.Value.Split("_").First();
        var pieceData = pieceDatabase.AllPieces.FirstOrDefault(p => p.name == pieceName);

        if (pieceData == null)
        {
            Debug.LogWarning($"Database does not contain any {requestedPieceCode} piece, returning a default piece",
                this);
            pieceData = pieceDatabase.AllPieces.First();
        }

        var instance = CommonOperations.Network.GetNetworkObjectId<Piece>(spawnedPieceId);

        var pieceConfiguration = pieceData.GetPieceConfiguration(requestedPieceCode.Value);
        pieceConfiguration.playerSide = playerSide;
        instance.Setup(pieceConfiguration);
    }

    [ClientRpc]
    private void NotifySpawnedPieceClientRpc (PlayerSide playerSide, ulong spawnedPieceId, bool intoMainHand)
    {
        var piece = CommonOperations.Network.GetNetworkObjectId<Piece>(spawnedPieceId);

        CommonOperations.AddPieceToPlayerSide(playerSide, piece, intoMainHand);
    }

    #endregion
}