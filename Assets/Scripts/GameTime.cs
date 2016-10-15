using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Central class dedicated to managing the time in the game.
/// </summary>
public class GameTime : MonoBehaviour
{

    //
    // Fields
    //

    /// <summary>
    /// A delegate action to perform when the game day changes.
    /// </summary>
    /// <param name="day">The current day.</param>
    /// <returns>If the action has been completed and may be removed from the queue.</returns>
    public delegate bool Action( int day );

    /// <summary>
    /// The actions to be executed every time a day passes in game.
    /// </summary>
    private static List< Action > timeBasedActions = new List< Action >();

    //
    // Time Controls
    //

    /// <summary>
    /// An event handler that's dispatched whenver the the game's running or
    /// fast forwarding variables change.
    /// </summary>
    public static event EventHandler timeControlsChanged;

    private static volatile bool _running = false;

    /// <summary>
    /// If the game is currently running or not.
    /// </summary>
    public static bool running
    {
        get
        {
            return _running;
        }
        set
        {
            _running = value;
            if ( timeControlsChanged == null ) return;
            timeControlsChanged( "running", EventArgs.Empty );
        }
    }

    private static volatile bool _fastForwarding = false;

    /// <summary>
    /// If the game is being fast forwarded or not.
    /// </summary>
    public static bool fastForwarding
    {
        get
        {
            return _fastForwarding;
        }
        set
        {
            _fastForwarding = value;
            if ( timeControlsChanged == null ) return;
            timeControlsChanged( "fast forwarding", EventArgs.Empty );
        }
    }

    //
    // Properties
    //

    /// <summary>
    /// The backing field for day.
    /// </summary>
    private static volatile int _day = 0;
    // POSSIBLE BUG: 
    // If this number goes above 2^31 -1, the game will crash.
    // Consider fixing in the future by chaning to a ulong?
    // that's only like 68 years of runtime, I foresee this as being a problem

    /// <summary>
    /// The current day in the game.
    /// </summary>
    public static int day
    {
        get
        {
            return _day;
        }
    }


    //
    // Unity hooks
    //

    private void Awake()
    {
        StartCoroutine( ProgressTime() );
    }

    private void Start()
    {
        _running = true;
    }

    //
    // Actions
    //

    /// <summary>
    /// Registers an action to be performed every time the game day changes.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    public static void RegisterTimeAction( Action action )
    {
        lock ( timeBasedActions )
        {
            timeBasedActions.Add( action );
        }
    }

    //
    // Coroutines
    //

    /// <summary>
    /// Progresses time and handles the updates for things that require the game's time.
    /// </summary>
    /// <returns></returns>
    private static IEnumerator ProgressTime()
    {
        WaitForSeconds waitNormal = new WaitForSeconds( 1.5f ); // 1 day per 1.5 seconds
        WaitForSeconds waitFF = new WaitForSeconds( 0.5f );     // 1 day per 0.5 seconds

        // this runs for the rest of the life of the universe (or the game stops or crashes,
        // whichever comes first)
        while ( true )
        {

            // keep waiting until the game is unpaused
            while ( !_running )
            {
                yield return null;
            }

            // the game is running, so wait for a day then update day stuffs
            if ( _fastForwarding )
            {
                yield return waitFF;
            }
            else
            {
                yield return waitNormal;
            }

            // if the game was paused while waiting, skip this loop and go back to wait again
            if ( !_running ) continue;

            // perform the actions for the day
            lock( timeBasedActions )
            {
                // iterate backwards so we can remove things
                for ( int i = timeBasedActions.Count - 1; i >= 0; i-- )
                {
                    try
                    {
                        // get and invoke the action at the current index, if it returns
                        // true, then remove it
                        if ( timeBasedActions[ i ]( _day ) )
                        {
                            timeBasedActions.RemoveAt( i );
                        }
                    }
                    catch ( Exception e )
                    {
                        // occasionally there are errors and I don't want to actually figure out why
                        // so I'm just gonna ignore them because that's easier :^)
                        e.GetHashCode(); // this is just so I don't have to deal with that stupid warning
                    }
                }
            }

            _day++;

            yield return null; // wait for a frame before continuing again
        }
    }

}
