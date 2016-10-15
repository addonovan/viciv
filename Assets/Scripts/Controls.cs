using UnityEngine;

/// <summary>
/// Static class used to centralize all control bindings in one place.
/// 
/// This also makes if statements based off of controls much easier to read.
/// </summary>
public static class Controls
{

    //
    // Temp stuff
    //

    public static bool showIDInfo
    {
        get
        {
            return Input.GetKeyDown( KeyCode.F1 );
        }
    }

    //
    // Movement
    //

    /// <summary>
    /// If the input binding to move to the left is active.
    /// </summary>
    public static bool moveLeft
    {
        get
        {
            return !UI.inUse && ( Input.GetKey( KeyCode.A ) || Input.GetKey( KeyCode.LeftArrow ) );
        }
    }

    /// <summary>
    /// If the input binding to move to the right is active.
    /// </summary>
    public static bool moveRight
    {
        get
        {
            return !UI.inUse && ( Input.GetKey( KeyCode.D ) || Input.GetKey( KeyCode.RightArrow ) );
        }
    }

    /// <summary>
    /// If the input binding to move forward is active.
    /// </summary>
    public static bool moveForward
    {
        get
        {
            return !UI.inUse && ( Input.GetKey( KeyCode.W ) || Input.GetKey( KeyCode.UpArrow ) );
        }
    }

    /// <summary>
    /// If the input binding to move backward is active.
    /// </summary>
    public static bool moveBackward
    {
        get
        {
            return !UI.inUse && ( Input.GetKey( KeyCode.S ) || Input.GetKey( KeyCode.DownArrow ) );
        }
    }

    /// <summary>
    /// If the input binding to sprint is active.
    /// </summary>
    public static bool sprint
    {
        get
        {
            return !UI.inUse && ( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) );
        }
    }

    //
    // Unit
    //

    /// <summary>
    /// If the input binding for deselecting a unit is active.
    /// </summary>
    public static bool unitDeselect
    {
        get
        {
            return !UI.inUse && World.selectedUnit != null && Input.GetKeyDown( KeyCode.Escape );
        }
    }

    public static bool unitFocus
    {
        get
        {
            return !UI.inUse && World.selectedUnit != null && Input.GetKeyDown( KeyCode.R );
        }
    }

    public static bool unitMove
    {
        get
        {
            return !UI.inUse && World.selectedUnit != null && Input.GetKeyDown( KeyCode.F );
        }
    }

    public static bool unitSpecial
    {
        get
        {
            return !UI.inUse && World.selectedUnit != null && Input.GetKeyDown( KeyCode.V );
        }
    }

    //
    // Mouse
    //

    /// <summary>
    /// The amount to zoom in.
    /// </summary>
    public static float zoomDelta
    {
        get
        {
            return UI.inUse ? 0f : Input.mouseScrollDelta.y;
        }
    }

    /// <summary>
    /// The position of the mouse.
    /// </summary>
    public static Vector3 mousePosition
    {
        get
        {
            return Input.mousePosition;
        }
    }

    /// <summary>
    /// If the main mouse button was just pressed this frame.
    /// </summary>
    public static bool click
    {
        get
        {
            return Input.GetMouseButtonDown( 0 );
        }
    }

    //
    // Overlays
    //

    /// <summary>
    /// If the binding that displays the tile coordinates is active.
    /// </summary>
    public static bool showTileCoordinates
    {
        get
        {
            return !UI.inUse && Input.GetKey( KeyCode.Alpha1 );
        }
    }

}
