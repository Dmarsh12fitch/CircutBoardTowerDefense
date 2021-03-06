using ActiveOrInactiveStateManagement;
using PrimitiveFocus;
using Tile;
using TurretShopEntry;
using UnityEngine;
using IFocusInteractor = PrimitiveFocus.IFocusInteractor;

/// <summary>
///     The interaction between the tile and the
/// </summary>
public class TileSelectionInteractor : MonoBehaviour, ITileSelectionInteractor
{
    [SerializeField] private GameObject tileText;
    private EnergyCounter energyCounter;
    private ITileExclusiveFocusInteractor focusInteractor;
    private IGridPositionedItem gridPosition;
    private ITileRangeIndicator rangeIndicator;
    private BasicExclusiveStateManager<ITileSelectionInteractor> tileSelectionManager;

    private ITileTurretBehavior turretBehavior;
    private IExclusiveStateManagerData<ITurretShopEntryBehavior> turretShop;

    private void Awake()
    {
        turretBehavior = GetComponent<ITileTurretBehavior>();
        rangeIndicator = GetComponent<ITileRangeIndicator>();

        Root = GetComponent<Tile.Tile>();

        // note if you use, `IGridPositionedItem`, you will find
        // that this class will satisfy it, even though it shouldn't
        // and doing that will cause a self-reference, which will
        // lead to a stack overflow on the call to GetLocation.
        gridPosition = GetComponent<IGridPositionedItem>();
        focusInteractor = GetComponent<ITileExclusiveFocusInteractor>();
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0)) AttemptToPlaceTurret();
        if (Input.GetMouseButtonDown(1)) SellTurret();
    }

    protected void OnMouseEnter()
    {
        tileSelectionManager.Activate(this);
    }

    protected void OnMouseExit()
    {
        tileSelectionManager.InactivateIfActive(this);
    }

    public Tile.Tile Root { get; protected set; }


    public IGridPositionedItem GetGridPositionedComponent()
    {
        return gridPosition;
    }

    public void Init(EnergyCounter energyCounter, IExclusiveStateManagerData<ISelectionInteractor> turretShop,
        BasicExclusiveStateManager<ITileSelectionInteractor> tileSelectionManager,
        ExclusiveSubsectionFocusManager focusManager)
    {
        this.energyCounter = energyCounter;
        this.turretShop = turretShop;
        this.tileSelectionManager = tileSelectionManager;
    }

    /// <summary>
    ///     This method returns the active state
    ///     of the tile.
    /// </summary>
    /// <returns></returns>
    public (FilledState, TurretShopSelectionStatus) GetState()
    {
        // determine whether the tile has something in it.
        var filledState = turretBehavior.GetTurret() != null ? FilledState.Filled : FilledState.Empty;

        // determine whether there is an active turret
        // in the shop and if we can afford it.
        TurretShopSelectionStatus affordability;

        if (turretShop.GetActive() == null)
            affordability = TurretShopSelectionStatus.NoActiveTurret;
        else if (turretShop.GetActive().GetEnergyCost() <= energyCounter.Energy)
            affordability = TurretShopSelectionStatus.AffordableActiveTurret;
        else
            affordability = TurretShopSelectionStatus.TooExpensiveActiveTurret;

        return (filledState, affordability);
    }

    public void Hovered()
    {
        if (GetState() == (FilledState.Empty, TurretShopSelectionStatus.AffordableActiveTurret))
        {
            rangeIndicator.SetRange(turretShop.GetActive().AssociatedTurretPrefab().Range);
            rangeIndicator.Show();
        }
    }

    public void UnHovered()
    {
        rangeIndicator.Hide();
    }

    public void AttemptToPlaceTurret()
    {
        switch (GetState())
        {
            case (FilledState.Empty, TurretShopSelectionStatus.AffordableActiveTurret):
                PlaceActiveShopTurret();
                break;

            case (FilledState.Empty, TurretShopSelectionStatus.TooExpensiveActiveTurret):
                DisplayWarningText("You Can't Afford That.");
                break;

            case (FilledState.Filled, _):
                //Debug.Log("Open_Menu_Here");
                DisplayWarningText("Turret already exists!");
                break;
        }
    }

    public void SellTurret()
    {
        switch (GetState())
        {
            case (FilledState.Empty, _):
                Debug.Log("Nothing to sell here");
                break;

            case (FilledState.Filled, _):
                var turret = turretBehavior.GetTurret();
                var refund = turret.SellAmount;
                energyCounter.Energy += refund;
                Destroy(turret.gameObject);
                break;
        }
    }


    /// <summary>
    ///     In this case, the active state is represents
    ///     the state of the square being "selected".
    ///     "selected" is a vague term in this cause,
    ///     but basically, all it's going to do is be able
    ///     to draw a focus element over this element.
    ///     This method cannot be called to set it's own active
    ///     state. This is an event function.
    /// </summary>
    public void OnActivate()
    {
        Hovered();
    }

    public void OnInactivate()
    {
        UnHovered();
    }

    public IFocusInteractor GetFocusInteractor()
    {
        return focusInteractor;
    }

    /// <summary>
    ///     Places a new turret at this location.
    ///     Preconditions
    ///     * There must be an active turret.
    ///     * We must be able to afford it.
    /// </summary>
    private void PlaceActiveShopTurret()
    {
        var turret = turretShop.GetActive().AssociatedTurretPrefab();

        energyCounter.Energy -= turretShop.GetActive().GetEnergyCost();
        var spawnPos = new Vector3(transform.position.x, -0.5f, transform.position.z);
        turretBehavior.SetTurret(Instantiate(turret, spawnPos, transform.rotation));
    }


    /// <summary>
    ///     Displays warning text on screen.
    ///     This should almost certainly
    ///     be another component.
    /// </summary>
    /// <param name="text"> the text to display</param>
    private void DisplayWarningText(string text)
    {
        var spawnPos = new Vector3(0, 8, 0);
        var gO = Instantiate(tileText, spawnPos, Quaternion.identity, transform);
        gO.GetComponent<TextMesh>().text = text;
        gO.GetComponent<Transform>().rotation = new Quaternion(90, 0, 0, 90);
    }
}