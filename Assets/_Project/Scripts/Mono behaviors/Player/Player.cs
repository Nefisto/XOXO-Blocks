using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using QFSW.QC;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public partial class Player : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    protected Bag originalBag;

    [field: SerializeField]
    public bool HasAlreadySetup { get; protected set; }

    [field: SerializeField]
    public string Nickname { get; protected set; }

    [field: SerializeField]
    public PlayerSide PlayerSide { get; protected set; }

    [field: SerializeField]
    public DraggingController DraggingController { get; protected set; }

    [field: SerializeField]
    protected Bag currentBag;

    [field: Header("Debug")]
    [field: SerializeField]
    public PlayerHUD PlayerHUD { get; protected set; }

    [field: SerializeField]
    protected Hand Hand { get; set; }

    [field: SerializeField]
    protected Hand SubHand { get; set; }

    protected IDrawBehavior drawBehavior;

    public bool TimeOut { get; protected set; }

    protected UserData UserData { get; set; }

    [Command("Player.IsPlaying", MonoTargetType.Argument)]
    protected bool IsSelectingPiece { get; set; }

    protected Timer Timer { get; set; }

    public WaitingRoomPlayerEntry WaitingRoomPlayerEntry { get; set; }

    public virtual void Setup (UserData userData, PlayerHUD playerHUD, WaitingRoomPlayerEntry waitingEntry)
    {
        UserData = userData;
        PlayerHUD = playerHUD;
        WaitingRoomPlayerEntry = waitingEntry;

        HasAlreadySetup = true;

        PlayerSide = userData.playerSide;
        Nickname = userData.userName;

        gameObject.name = $"-{userData.playerSide.ToString()} - PLAYER";

        if (PlayerSide == PlayerSide.Cross)
            ServiceLocator.GameReferences.CrossPlayer = this;
        else
            ServiceLocator.GameReferences.CirclePlayer = this;

        Hand = new Hand();
        SubHand = new Hand();

        Timer = new Timer(GameConstants.GameplayCostants.TURN_TIMER);

        if (IsServer)
            Timer.OnTimerEnd += () =>
            {
                ServiceLocator.TurnState.HasFinishedSelectPhase = true;
                ServiceLocator.TurnState.FinishReason = TurnFinishedReason.TimedOut;
                NotifyClientsThatTimerFinishedClientRpc();
            };

        waitingEntry.SetupEntry(UserData.playerSide, userData.userName);

        if (!IsOwner || UserData.isBot)
        {
            ServiceLocator.GameReferences.TopPlayerHud.SetupHUD(userData, Hand, Timer, SubHand);
            return;
        }

        ServiceLocator.GameReferences.EmojiManager.OnRequestEmojiSend += RequestEmojiHandler;

        DraggingController = ServiceLocator.DraggingController;
        Hand.SetupAsMain(DraggingController);

        currentBag = CreateNewBag();
        GameEvents.GameplayEvents.OnPlacePiece += piece => { Hand.DetachPiece(piece); };
        // Clean: CAN I REMOVE THIS AND ACCESS SIDE THROUGH MY REF IN SERVICE LOCATOR?
        ServiceLocator.PlayerSide = PlayerSide;
        ServiceLocator.GameReferences.BottomPlayerHud.SetupHUD(userData, Hand, Timer, SubHand);
        IsSelectingPiece = false;
        DraggingController.DisableDrag();

        drawBehavior = ServiceLocator.GameSettings.DraftDraw
            ? new DraftDrawBehavior(this, Hand, SubHand, GetNewPiece)
            : new DefaultDrawBehavior(Hand, GetNewPiece);
    }

    private void RequestEmojiHandler (EmojiData data) => RequestEmojiToServerRpc(PlayerSide, data.Kind);

    [ServerRpc(RequireOwnership = false)]
    private void RequestEmojiToServerRpc (PlayerSide side, EmojisKind kind)
    {
        RequestEmojiToClientRpc(side, kind);
    }

    [ClientRpc]
    private void RequestEmojiToClientRpc (PlayerSide side, EmojisKind kind)
    {
        var playerSideRef = CommonOperations.GetPlayerRefOf(side);

        playerSideRef.PlayerHUD.ShowEmoji(kind);
    }

    protected Bag CreateNewBag()
    {
        var instance = originalBag.GetInstance;
        instance.ResetAllRemainingPieces();
        return instance;
    }

    public virtual IEnumerator PlayTurn()
    {
        if (!IsOwner)
            yield break;

        PlayerHUD.ActiveTurn();

        yield return drawBehavior.DrawPhase();

        IsSelectingPiece = true;
        DraggingController.EnableDrag();
        GameEvents.GameplayEvents.OnPlacePiece += PlacePieceHandle;

        TimeOut = false;
        TriggerTimerServerRpc();
        yield return new WaitWhile(() => IsSelectingPiece && !TimeOut);
        StopTimerServerRpc();
        DraggingController.DisableDrag();

        GameEvents.GameplayEvents.OnPlacePiece -= PlacePieceHandle;
    }

    public virtual IEnumerator WaitTurn()
    {
        if (!IsOwner)
            yield break;

        PlayerHUD.WaitTurn();
        IsSelectingPiece = false;
        DraggingController.DisableDrag();
    }

    public IEnumerator DiscardPhase()
    {
        if (!IsOwner)
            yield break;

        yield return drawBehavior.DiscardPhase();
        NotifyEndOfDiscardPhaseServerRpc();
    }

    [ServerRpc]
    protected void NotifyEndOfDiscardPhaseServerRpc() => ServiceLocator.TurnState.HasFinishedDiscardPhase = true;

    public virtual void GetNewPiece (bool intoMainHand)
    {
        if (!IsOwner)
            return;

        if (currentBag.IsEmpty)
            currentBag = CreateNewBag();
        ;
        if (IsHandFull(intoMainHand))
        {
            Debug.Log("Requesting failed: Hand is full");
            return;
        }

        var randomizedPieceCode = currentBag.GetPieceCode();
        ServiceLocator.GameReferences.PieceSpawnManager.RequestSpawnPieceServerRpc(ServiceLocator.PlayerSide,
            new FixedString32Bytes(randomizedPieceCode), OwnerClientId, intoMainHand);
    }

    protected bool IsHandFull (bool mainHand) => mainHand ? Hand.IsFull : SubHand.IsFull;

    [Command("Add-piece", MonoTargetType.Argument)]
    public virtual void AddPiece (Piece piece, bool intoMainHand)
    {
        if (intoMainHand)
            Hand.AddPiece(piece);
        else
            SubHand.AddPiece(piece);
    }

    public void RemovePiece (Piece piece) => Hand.DetachPiece(piece);

    protected void PlacePieceHandle (Piece obj)
    {
        NotifyPiecePlacedServerRpc();
        IsSelectingPiece = false;
    }

    public IEnumerator InitialDraw()
    {
        yield return drawBehavior.InitialDrawPhase();
    }

    #region Server RPCs

    [ServerRpc]
    protected void NotifyPiecePlacedServerRpc()
    {
        ServiceLocator.TurnState.FinishReason = TurnFinishedReason.PiecePlaced;
        ServiceLocator.TurnState.HasFinishedSelectPhase = true;
    }

    [ServerRpc]
    protected void TriggerTimerServerRpc() => TriggerTimerClientRpc();

    [ServerRpc]
    protected void StopTimerServerRpc() => StopTimerClientRpc();

    [ServerRpc]
    public void GetNewDraftHandServerRpc() => GetNewDraftHandClientRpc();

    #endregion

    #region Client RPCs

    [ClientRpc]
    protected void GetNewDraftHandClientRpc()
    {
        StartCoroutine(Routine());

        IEnumerator Routine()
        {
            Hand.ClearHand(true);

            yield return new WaitForSeconds(1f);
            var pieces = SubHand.Pieces.ToList();
            SubHand.ClearHand();
            foreach (var piece in pieces)
                Hand.AddPiece(piece);

            if (!IsOwner)
                yield break;

            yield return new WaitForSeconds(.5f);

            var counter = 0;
            while (counter < 3)
            {
                GetNewPiece(false);
                yield return new WaitForSeconds(0.2f);
                counter++;
            }
        }
    }

    [ClientRpc]
    protected void TriggerTimerClientRpc() => Timer.StartTimer().Forget();

    [ClientRpc]
    protected void StopTimerClientRpc() => Timer.StopTimer();

    [ClientRpc]
    protected void NotifyClientsThatTimerFinishedClientRpc()
    {
        ServiceLocator.TurnState.HasFinishedSelectPhase = true;
        ServiceLocator.TurnState.FinishReason = TurnFinishedReason.TimedOut;
        TimeOut = true;
    }

    #endregion
}