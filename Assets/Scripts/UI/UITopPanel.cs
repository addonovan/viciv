using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UITopPanel : MonoBehaviour
{


    //
    // Constants
    //

    private const string DATE_FORMAT = "Day <b>+{0}</b>";
    
    //
    // Components
    //

    /// <summary>
    /// The date label
    /// </summary>
    public Text dateLabel;

    /// <summary>
    /// Button used to pause the game.
    /// </summary>
    public Button togglePause;

    /// <summary>
    /// Button used to toggle the fastforwarding in the game.
    /// </summary>
    public Button toggleFF;

    /// <summary>
    /// The label used to display information to the user
    /// </summary>
    public Text informationLabel;

    //
    // Unity Hooks
    //

    private void Start()
    {
        GameTime.RegisterTimeAction( UpdateDate );
        GameTime.timeControlsChanged += OnTimeControlChange;

        StartCoroutine( UpdateInfoPanel() );
    }

    //
    // Actions
    //

    /// <summary>
    /// Updates the day signifying that one day has passed.
    /// </summary>
    /// <param name="day">The new day's number.</param>
    /// <returns></returns>
    private bool UpdateDate( int day )
    {
        dateLabel.text = string.Format( DATE_FORMAT, day );

        return false; // never remove this from the list
    }

    public void TogglePause()
    {
        GameTime.running ^= true; // toggles without a second call
    }

    public void ToggleFastForwarding()
    {
        GameTime.fastForwarding ^= true; // toggles without a second call
    }

    //
    // Creating Units
    //

    private List< Province > GetPlayerProvinces()
    {
        List< Province > provinces = new List< Province >();

        foreach ( Province p in Faction.player.provinces )
        {
            if ( p.underOccupation ) continue;
            provinces.Add( p );
        }

        return provinces;
    }

    public void CreateUnit( UnitType type )
    {
        List< Province > provinces = GetPlayerProvinces();
        if ( provinces.Count == 0 ) return;
        UI.chooseProvinceDialog.ShowProvinceSelection( provinces, ( p => p.Produce( type ) ) ); 
    }

    public void CreateSettler()
    {
        CreateUnit( UnitType.SETTLER );
    }

    public void CreateWarrior()
    {
        CreateUnit( UnitType.WARRIOR );
    }

    //
    // Observable actions
    //

    /// <summary>
    /// Handles a change in the time controls.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnTimeControlChange( object sender, EventArgs args )
    {
        string text = sender as string;
        if ( text == null ) return;

        if ( text.Equals( "running" ) )
        {
            Image image = togglePause.image;
            image.color = GameTime.running ? Color.white : Color.red;
            togglePause.image = image;
        }
        else if ( text.Equals( "fast forwarding" ) )
        {
            Image image = toggleFF.image;
            image.color = GameTime.fastForwarding ? Color.yellow : Color.white;
            toggleFF.image = image;
        }
    }

    //
    // Information Panel
    //

    /// <summary>
    /// Updates the information panel from the message queue.
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateInfoPanel()
    {
        WaitForSeconds showInfoWait = new WaitForSeconds( 5.0f );

        while ( true )
        {
            string text = UI.PopMessage();

            // reset the label so it will catch the user's eye
            informationLabel.text = "";
            yield return null;

            informationLabel.text = text;

            if ( text.Length == 0 )
            {
                yield return null; // just wait until the next frame, so the next info will immediately be displayed
            }
            else
            {
                yield return showInfoWait; // wait for a little bit to show the information
            }
        }
    }

}
