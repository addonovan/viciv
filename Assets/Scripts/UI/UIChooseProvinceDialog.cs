using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIChooseProvinceDialog : UIDialog
{

    //
    // (Unity Set) Components
    //

    public Dropdown dropDown;

    //
    // Actions
    //

    public delegate void OnProvinceSelected( Province province );

    private OnProvinceSelected callback;

    /// <summary>
    /// Shows the province selection screen for the given provinces.
    /// </summary>
    /// <param name="provinces">The provinces to show.</param>
    /// <param name="callback">The method callback for when a province is selected.</param>
    public void ShowProvinceSelection( List< Province > provinces, OnProvinceSelected callback )
    {
        dropDown.ClearOptions(); // remove all the previous options

        // add the new province options
        List< Dropdown.OptionData > options = new List< Dropdown.OptionData >();
        foreach ( Province p in provinces )
        {
            options.Add( new Dropdown.OptionData()
            {
                text = p.name
            } );
        }

        dropDown.AddOptions( options );

        this.callback = callback;
        Show();
    }

    /// <summary>
    /// DO NOT CALL. calls the callback.
    /// </summary>
    public void OnSelected()
    {
        Hide();
        if ( callback == null ) return;

        string name = dropDown.options[ dropDown.value ].text;
        Province p = World.GetProvinceByName( name );

        // ensure this isn't true
        if ( p == null )
        {
            Debug.Log( "No province for chosen name: " + name );
            return;
        }

        // make the callback
        callback( p );
        callback = null;
    }

    public void OnCancel()
    {
        Hide();
        callback = null;
    }

}
