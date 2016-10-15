using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class UI
{

    /// <summary>
    /// True if the UI is currently being used, so regular controls should be disabled.
    /// </summary>
    public static bool inUse = false;

    #region UI Objects

    private static UITilePanel _tilePanel;

    public static UITilePanel tilePanel
    {
        get
        {
            return _tilePanel;
        }
    }

    private static UIUnitPanel _unitPanel;

    public static UIUnitPanel unitPanel
    {
        get
        {
            return _unitPanel;
        }
    }

    private static UINameProvinceDialog _nameProvinceDialog;

    public static UINameProvinceDialog nameProvinceDialog
    {
        get
        {
            return _nameProvinceDialog;
        }
    }

    private static UIChooseProvinceDialog _chooseProvinceDialog;

    public static UIChooseProvinceDialog chooseProvinceDialog
    {
        get
        {
            return _chooseProvinceDialog;
        }
    }

    #endregion

    //
    // Message Queue
    //

    /// <summary>
    /// The message "queue" of messages to show to the user in the info panel.
    /// </summary>
    private static List< string > messageQueue = new List< string >();

    /// <summary>
    /// Shows a message to the user.
    /// </summary>
    /// <param name="messageFormat">The string to format.</param>
    /// <param name="stuff">The formatting parameters.</param>
    public static void ShowMessage( string messageFormat, params object[] stuff )
    {
        messageQueue.Add( string.Format( messageFormat, stuff ) );
    }

    /// <summary>
    /// Grabs the next message from the message queue then removes the element.
    /// </summary>
    /// <returns>The next message from the queue, or an empty string if there was none.</returns>
    public static string PopMessage()
    {
        if ( messageQueue.Count > 0 )
        {
            string message = messageQueue[ 0 ];
            messageQueue.RemoveAt( 0 );
            return message;
        }
        return "";
    }

    //
    // Initialization
    //

    /// <summary>
    /// Initializes the UI to work for the given world manager.
    /// </summary>
    /// <param name="manager">The world manager object.</param>
    internal static void Initialize( WorldManager manager )
    {
        // grab the UI information
        _tilePanel = manager.uiTilePanel;
        _unitPanel = manager.uiUnitPanel;
        _nameProvinceDialog = manager.uiProvinceName;
        _chooseProvinceDialog = manager.uiChooseProvince;
    }

}
