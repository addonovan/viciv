using UnityEngine;

public class UIDialog : MonoBehaviour
{

    //
    // Constants
    //

    /// <summary>
    /// The position we move dialogs to so they don't show on the screen. A little hacky, but whatevs at this point.
    /// </summary>
    private static readonly Vector3 OFF_SCREEN = new Vector3( -1000, -1000 );

    /// <summary>
    /// The center of the screen, so the user can use the thing.
    /// </summary>
    private static readonly Vector3 ON_SCREEN = new Vector3( 0, 0 );

    //
    // Components
    //

    protected RectTransform rect;

    //
    // Unity Hooks
    //

    private void Awake()
    {
        rect = GetComponent< RectTransform >();
    }

    //
    // Visibility
    // 

    /// <summary>
    /// If the game was paused or not before the dialog was shown.
    /// </summary>
    private bool wasGamePause = false;

    /// <summary>
    /// Hides the dialog, unpauses the game if necessary, and marks the UI as not in use.
    /// </summary>
    protected void Hide()
    {
        rect.localPosition = OFF_SCREEN;
        UI.inUse = false;
        GameTime.running = wasGamePause; // if the game wasn't paused before the dialog appears, it shouldn't be afterwards
    }

    /// <summary>
    /// Shows the dialog, pauses the game, and marks the UI in use.
    /// </summary>
    protected void Show()
    {
        rect.localPosition = ON_SCREEN;
        UI.inUse = true;
        wasGamePause = GameTime.running;
        GameTime.running = false;
    }

}
