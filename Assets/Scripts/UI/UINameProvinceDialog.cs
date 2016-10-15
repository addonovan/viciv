using UnityEngine;
using UnityEngine.UI;

public class UINameProvinceDialog : UIDialog
{

    //
    // (Unity Set) Components
    //

    /// <summary>
    /// The input field we get the name of the province from.
    /// </summary>
    public InputField provinceNameField;

    //
    // Actions
    //

    /// <summary>
    /// The definition of a method callback, called when
    /// </summary>
    /// <param name="name"></param>
    public delegate void ProvinceNameCallback( string name );

    /// <summary>
    /// The callback, called whenever the province name is chosen.
    /// </summary>
    private ProvinceNameCallback callback;

    /// <summary>
    /// Shows the dialog to the user and asks for input.
    /// </summary>
    public void ShowDialog( ProvinceNameCallback callback )
    {
        // reset the name
        provinceNameField.text = "";
        this.callback = callback;

        Show();
    }

    /// <summary>
    /// DO NOT CALL. This is called whenever the button is clicked.
    /// </summary>
    public void OnNameChosen()
    {
        if ( callback == null ) return;

        // make the callback, then set it to null
        callback( provinceNameField.text );
        callback = null;

        Hide();
    }

}
