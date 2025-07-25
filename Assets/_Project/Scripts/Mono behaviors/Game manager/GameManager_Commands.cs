using QFSW.QC;
using Unity.Netcode;

public partial class GameManager
{
    [Command("Game.Add_Points_Cross")]
    [ServerRpc(RequireOwnership = false)]
    private void AddPointToCrossServerRpc (int points = 5)
    {
        ServiceLocator.GameState.PointsOfCross += points;
        StartCoroutine(GameEvents.GameplayEvents.OnPointScored?.YieldableInvoke(this,
            new GameEvents.OnPointScoredEventArgs { PlayerSide = PlayerSide.Cross }));
    }

    [Command("Game.Add_Points_Circle")]
    [ServerRpc(RequireOwnership = false)]
    private void AddPointToCircleServerRpc (int points = 5)
    {
        ServiceLocator.GameState.PointsOfCircle += points;
        StartCoroutine(GameEvents.GameplayEvents.OnPointScored?.YieldableInvoke(this,
            new GameEvents.OnPointScoredEventArgs { PlayerSide = PlayerSide.Circle }));
    }
}