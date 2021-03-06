using PrimitiveFocus;
using Tile;
using UnityEngine;
using UnityEngine.Assertions;

public class TileExclusiveFocusInteractor : ExclusiveFocusInteractor, ITileExclusiveFocusInteractor
{
    private ITileFocusDisplay display;
    private ITileSelectionInteractor selectionInteractor;

    private void Awake()
    {
        display = GetComponent<ITileFocusDisplay>();
        selectionInteractor = GetComponent<ITileSelectionInteractor>();
        Root = GetComponent<Tile.Tile>();

        // make sure we're the only one who is using this interface on this object.
        GameObjectHelper.AssertOnlyComponentOfType<IExclusiveFocusInteractor>(this);
    }

    private void Start()
    {
        Assert.IsNotNull(GetFocusManager(), $"{GetType().Name} didn't get a manager");
    }


    protected void OnMouseEnter()
    {
        GetManager().Activate(this);
    }

    protected void OnMouseExit()
    {
        GetManager().InactivateIfActive(this);
    }

    public Tile.Tile Root { get; protected set; }

    public override void OnActivate()
    {
        display.SetFocusColor(GetColor());
        display.Show();
    }

    public override void OnInactivate()
    {
        display.Hide();
    }


    protected virtual Color GetColor()
    {
        return selectionInteractor.GetState() switch
        {
            (FilledState.Empty, TurretShopSelectionStatus.AffordableActiveTurret) => Color.green,
            (FilledState.Empty, TurretShopSelectionStatus.NoActiveTurret) => Color.white,
            (FilledState.Empty, TurretShopSelectionStatus.TooExpensiveActiveTurret) => Color.red,
            (FilledState.Filled, _) => Color.blue,
            _ => Color.magenta
        };
    }
}