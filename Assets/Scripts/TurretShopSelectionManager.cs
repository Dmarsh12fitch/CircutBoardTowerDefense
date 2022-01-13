using ActiveOrInactiveStateManagement;
using DefaultNamespace;
using UnityEngine;

/// <summary>
/// this class does very little, because
/// right now we're currently using the other
/// old turret shop entry manager.
///
/// that is so that the ui shows up a little
/// better.
/// </summary>
public class TurretShopSelectionManager : BasicExclusiveStateManager<TurretShopEntry.ISelectionInteractor>
{
     [SerializeField]
     private TurretShopEntry.SelectionInteractor defaultDisplay;

     protected virtual void Awake()
     {
          Activate(defaultDisplay);
     }
    
    
}