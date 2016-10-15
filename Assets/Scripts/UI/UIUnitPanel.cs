using UnityEngine;
using UnityEngine.UI;

public class UIUnitPanel : MonoBehaviour
{

    //
    // Constants
    //

    private const string FORMAT_BOLD = "<b>{0}</b>";

    private const string FORMAT_MOVEMENT = "Will arrive at <b>{0}</b>\n  On day <b>{1}</b>";

    //
    // Components
    //

    /// <summary>
    /// The label for the description of the current unit.
    /// </summary>
    public Text descriptionLabel;

    /// <summary>
    /// The label for the position of the current unit.
    /// </summary>
    public Text positionLabel;

    /// <summary>
    /// The label for explaining the unit's movement.
    /// </summary>
    public Text movementLabel;

    /// <summary>
    /// The button used for letting the player move the unit.
    /// </summary>
    public Text moveButtonLabel;

    /// <summary>
    /// The button used for letting the player perform the unit's special.
    /// </summary>
    public Text specialButtonLabel;

    //
    // Unity Hooks
    //

    private void Update()
    {
        Unit unit = World.selectedUnit;

        // hide ourselves if we're null
        if ( unit == null )
        {
            transform.localScale = Vector3.zero;
            return;
        }

        // unhide the panel
        transform.localScale = Vector3.one;

        // update the information of the unit
        descriptionLabel.text = string.Format( FORMAT_BOLD, unit.type.name );
        positionLabel.text = string.Format( FORMAT_BOLD, unit.coordinates.ToString() );

        if ( unit.movement.moving )
        {
            movementLabel.text = string.Format( FORMAT_MOVEMENT, unit.movement.nextStop.ToString(), unit.movement.arrivalDate );
            moveButtonLabel.text = string.Format( FORMAT_BOLD, "Stop" );
        }
        else
        {
            movementLabel.text = "";
            moveButtonLabel.text = string.Format( FORMAT_BOLD, "Move" );
        }

        specialButtonLabel.text = string.Format( FORMAT_BOLD, unit.type.specialDesc );

    }

    //
    // Actions (from Buttons)
    //

    public void CenterOnUnit()
    {
        World.GetTileAt( World.selectedUnit.coordinates ).FocusOn();
    }

    public void MoveUnit()
    {
        if ( !World.selectedUnit.movement.moving )
        {
            World.showUnitRange = !World.showUnitRange;
        }
        else
        {
            World.selectedUnit.movement.Cancel();
        }
    }

    public void PerformUnitSpecial()
    {
        World.selectedUnit.type.specialAction( World.selectedUnit );
    }

}
