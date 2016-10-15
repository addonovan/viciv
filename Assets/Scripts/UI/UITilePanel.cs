using UnityEngine;
using UnityEngine.UI;

public class UITilePanel : MonoBehaviour
{

    //
    // Constants
    //

    private const string INFORMATION_FORMAT =
        "<b>{0}</b>\n" + 
        "<b>Type</b> {1}\n" +
        "<b>Food</b> {2}";

    //
    // Properties
    //

    /// <summary>
    /// The tile for which this panel will display information.
    /// </summary>
    private Tile tile;

    //
    // Fields
    //

    /// <summary>
    /// The text element we'll update.
    /// </summary>
    public Text tileLabel;

    //
    // Unity Hooks
    //

    private void Update()
    {
        if ( tile != null )
        {
            tileLabel.text = string.Format( INFORMATION_FORMAT, tile.coordinates.ToString(), tile.type.name, tile.baseFood );
            transform.localScale = Vector3.one;
        }
        else
        {
            transform.localScale = Vector3.zero;
        }
    }

    //
    // Actions
    //

    /// <summary>
    /// Toggles the tile information to display information for the given tile.
    /// 
    /// If this tile's information is already being displayed, the panel will be hidden.
    /// If t is null, this panel will be hidden.
    /// </summary>
    /// <param name="t">The tile to toggle information for, or null.</param>
    public void ToggleTileInformation( Tile t )
    {
        // hide the panel
        if ( t == null || t.Equals( tile ) )
        {
            tile = null;
            return;
        }

        tile = t;
    }

}
