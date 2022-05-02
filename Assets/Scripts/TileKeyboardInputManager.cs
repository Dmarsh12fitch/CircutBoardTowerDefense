using UnityEngine;

public class TileKeyboardInputManager : MonoBehaviour
{
    /// <summary>
    ///     The thing that keeps track of
    ///     which part of the UI actively has
    ///     focus. So that way we aren't
    ///     moving around a menu cursor when
    ///     we're not supposed to be.
    /// </summary>
    [SerializeField] private ActiveRegionFocus activeRegionFocus;

    /// <summary>
    ///     The thing that creates the tiles
    ///     and abstracts its array based design.
    /// </summary>
    private TileCreationManager creationManager;

    private TileFocusManager focusManager;

    // try to use these two together,
    // definitely should be their own
    // delegation. But I'll 
    // refactor that out later.
    // and they only need to be synced for 
    // right now because we are saying 
    // that selection and focusing are the 
    // same kind of idea, but later with 
    // something like multi select, that 
    // definition may change.
    private TileSelectionManager selectionManager;

    private void Awake()
    {
        selectionManager = GetComponent<TileSelectionManager>();
        creationManager = GetComponent<TileCreationManager>();
        focusManager = GetComponent<TileFocusManager>();
    }

    private void Update()
    {
        HandleKeyboardInput();
    }

    public void Activate(Tile.Tile tile)
    {
        selectionManager.Activate(tile.GetSelectionInteractor());
        focusManager.Activate(tile.GetFocusInteractor());
    }

    public ITileSelectionInteractor GetActive()
    {
        return selectionManager.GetActive();
    }

    private bool AttemptMoveCardinalDirection(CardinalDirection direction)
    {
        var deltaLoc = GridLocation.MakeFromCardinalDirection(direction);
        // should this be the focus manager or the selection manager
        // or some other manager all together?
        var active = selectionManager.GetActive();

        // if there's nothing active, we might as well
        // guard return, because there's nothing left to be done.
        if (active == null)
        {
            Activate(creationManager.TileGrid.GetRandomNonNullGridItem());
            return true;
        }

        // now let's translate and see if we can find where we can move.
        var tileLocation = GridLocation.Add(active.GetGridPositionedComponent().GetLocation(), deltaLoc);
        while (creationManager.TileGrid.IsLocationValid(tileLocation))
        {
            var possibleNextFocusTile = creationManager.TileGrid[tileLocation];
            if (possibleNextFocusTile != null)
            {
                Activate(possibleNextFocusTile);
                return true;
            }

            tileLocation = GridLocation.Add(tileLocation, deltaLoc);
        }

        return false;
    }


    private void HandleKeyboardInput()
    {
        // if we don't have the ability to control
        // the state with the keyboard then check if we 
        // can take it, and if we can't then, return.
        if (!activeRegionFocus.IsActive(focusManager))
        {
            // I always want to return focus to the main game, when I hit escape.
            // I also always want to be the focus region when nobody else is.
            if (Input.GetKeyDown(KeyCode.Escape) || activeRegionFocus.GetActive() == null)
            {
                Debug.Log($"Activating the main game as the primary focus, was ${activeRegionFocus.GetActive()}");
                activeRegionFocus.Activate(focusManager);
            }
            else
            {
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.W)) AttemptMoveCardinalDirection(CardinalDirection.North);

        if (Input.GetKeyDown(KeyCode.S)) AttemptMoveCardinalDirection(CardinalDirection.South);

        if (Input.GetKeyDown(KeyCode.D)) AttemptMoveCardinalDirection(CardinalDirection.East);

        if (Input.GetKeyDown(KeyCode.A)) AttemptMoveCardinalDirection(CardinalDirection.West);

        if (DataSaverLoader.Gd.IsArcadeBuild)
        {
            if (Input.GetKeyDown(KeyCode.Space) && GetActive() != null)
            {
                GetActive().AttemptToPlaceTurret();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E) && GetActive() != null)
            {
                GetActive().AttemptToPlaceTurret();
            }
        }

        if (Input.GetKeyDown(DataSaverLoader.Gd.SellTurret) && GetActive() != null) GetActive().SellTurret();
    }
}