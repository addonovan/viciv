using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour, IProperty
{

    //
    // Faction Ownership
    //

    private Faction _faction;

    /// <summary>
    /// The current faction that owns this unit.
    /// </summary>
    public Faction faction
    {
        get
        {
            return _faction;
        }
        set
        {
            if ( _faction == value ) return;

            Faction previousFaction = _faction;
            _faction = value;

            // moving it after the setting prevents a stack overflow whenever it's being removed via the faction
            if ( previousFaction != null ) previousFaction.RemoveUnit( this );
            if ( value != null )
            {
                value.AddUnit( this );
            }
            else
            {
                _coordinates = new TileCoordinates( -1, -1 );
            }
        }
    }

    //
    // Position
    //

    private TileCoordinates _coordinates;

    /// <summary>
    /// The position this unit resides at.
    /// </summary>
    public TileCoordinates coordinates
    {
        get
        {
            return _coordinates;
        }

        set
        {
            _coordinates = value;
            World.InvalidateUnitMesh();
        }
    }

    /// <summary>
    /// The center of this unit as it appears on the map.
    /// </summary>
    public Vector3 position
    {
        get
        {
            // get the tile's coordinates
            Vector3 position = coordinates.position;

            position.x += INNER_RADIUS;

            // we're either up or down from that, depending on if we're military or civilian
            if ( type.isMilitary )
            {
                position.z += ( Tile.INNER_RADIUS / 2f );
            }
            else
            {
                position.z -= ( Tile.INNER_RADIUS / 2f );
            }

            return position;
        }
    }

    //
    // Unit Type
    //

    /// <summary>
    /// The type of unit this is.
    /// </summary>
    public UnitType type;

    //
    // Unit Movement Helper
    //

    /// <summary>
    /// The backing field for movement.
    /// </summary>
    private UnitMovement _movement;

    /// <summary>
    /// The object used for unit movement.
    /// </summary>
    public UnitMovement movement
    {
        get
        {
            return _movement;
        }
    }

    //
    // Unity Hooks
    //

    private void Start()
    {
        // create our unit movement handler
        _movement = new UnitMovement( this );
        GameTime.RegisterTimeAction( Tick );
    }

    //
    // Movement
    //

    /// <summary>
    /// Moves the unit to the specified coordinates.
    /// </summary>
    /// <param name="coordinates">The new coordinates of the tile.</param>
    /// <returns>True iff the unit's position was changed.</returns>
    public bool MoveTo( TileCoordinates coordinates )
    {
        // if the new coords don't exist, or they aren't any different, return false
        if ( coordinates == null ) return false;

        // coordinates is guaranteed to be non-null, _coordinates is not at this point
        if ( coordinates.Equals( this.coordinates ) ) return false;

        // If we can't even move to the end point, then we can't go there
        if ( !movement.CanMoveToTile( coordinates ) ) return false;

        // start the movement process if we even got to this stage
        movement.MoveTo( coordinates );

        return true; // because the position actually changed
    }

    /// <summary>
    /// Gets the tiles in the range that are can be moved to with the given number of moves from
    /// the start.
    /// </summary>
    /// <param name="movesLeft">The number of moves left.</param>
    /// <param name="start">The position to start at.</param>
    /// <returns>A list of all tiles that are able to be walked to with the given number of moves.</returns>
    private void AddTilesInRange( List< Tile > tiles, int movesLeft, Tile start )
    {
        // yes, the base case. This is somewhat helpful
        // I totally remembered this before I ran it for the first time
        if ( movesLeft == 0 ) return;

        foreach ( Tile neighbor in start.neighbors )
        {
            if ( neighbor == null ) continue;
            if ( !neighbor.type.isWalkable ) continue;

            // had to make this myself because the List.contains method is weird, even
            // after I implemented IEquatable, is still didn't work and randomly would return true
            bool inList = false;
            foreach ( Tile t in tiles )
            {
                if ( neighbor.Equals( t ) )
                {
                    inList = true;
                    break;
                }
            }

            // only add this to the list if it's not already in it
            if ( !inList ) tiles.Add( neighbor );

            // continue to recurse, because the route we took to get here might've been more efficient
            // than other routes
            AddTilesInRange( tiles, movesLeft - 1, neighbor );
        }
    }

    //
    // Equals & Hashcode
    //

    public override bool Equals( object o )
    {
        Unit unit = o as Unit;
        if ( unit == null ) return false;
        if ( !coordinates.Equals( unit.coordinates ) ) return false;
        if ( !type.Equals( unit.type ) ) return false;
        return true;
    }

    public override int GetHashCode()
    {
        return coordinates.GetHashCode() + type.GetHashCode();
    }

    //
    // Coroutines
    //

    private bool Tick( int day )
    {
        if ( !type.isMilitary ) return true;

        // if we're a military unit test fighting and what not
        if ( type.isMilitary )
        {
            var unitLists = World.GetUnitsAt( coordinates );
            List< Unit > militaryUnits = unitLists[ 0 ];
            List< Unit > civilianUnits = unitLists[ 1 ];

            foreach ( Unit u in militaryUnits )
            {
                if ( u.faction != faction )
                {
                    // start a fight!
                    Debug.Log( "1v1 me irl" );

                    // fighting is entire based on RNGesus
                    bool weDie = Random.Range( 0, 100 ) > 50;

                    // if we die, kill us, let the other one win :(
                    if ( weDie )
                    {
                        if ( faction == Faction.player )
                        {
                            UI.ShowMessage( "Lost unit at {0}" + coordinates.ToString() );
                        }

                        World.DeleteUnit( this );
                        return true; // deregister us from the ticking queue
                    }
                }
            }

            if ( militaryUnits.Count == 0 && civilianUnits.Count != 0 )
            {
                foreach ( Unit u in civilianUnits )
                {
                    // capture civlilian units of another faction
                    if ( u.faction != faction )
                    {
                        if ( faction == Faction.player )
                        {
                            UI.ShowMessage( "Captured a unit at {0}", u.coordinates.ToString() );
                        }
                        else if ( u.faction == Faction.player )
                        {
                            UI.ShowMessage( "Unit at {0} was captured!", u.coordinates.ToString() );
                        }

                        u.faction = faction;
                    }
                }
            }
        }

        return false; // don't deschedule ourselves
    }

    //
    // Constants
    //

    /// <summary>
    /// The radius from the center of a unit to one of its corners.
    /// </summary>
    public const float OUTER_RADIUS = Tile.OUTER_RADIUS / 3f;

    /// <summary>
    /// The radius from the center of a unit to the middle of one of its edges.
    /// </summary>
    public const float INNER_RADIUS = Tile.INNER_RADIUS / 3f;

    /// <summary>
    /// The height of one unit.
    /// </summary>
    public const float HEIGHT = 3f;

}