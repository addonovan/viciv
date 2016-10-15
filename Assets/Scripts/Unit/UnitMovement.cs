using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple class that manages the unit movement things.
/// </summary>
public class UnitMovement
{

    //
    // Properties
    //

    /// <summary>
    /// If the unit is still moving or not.
    /// </summary>
    public bool moving { get; set; }

    /// <summary>
    /// Backing field for range.
    /// </summary>
    private int _range = 0;

    /// <summary>
    /// The range the unit can move at the given time.
    /// </summary>
    public int range
    {
        get
        {
            return _range;
        }
    }

    /// <summary>
    /// The day this leg of the trip will end on.
    /// </summary>
    private int _arrivalDate = 0;

    /// <summary>
    /// The time the unit will arrive at its destination.
    /// </summary>
    public int arrivalDate
    {
        get
        {
            return _arrivalDate;
        }
    }

    /// <summary>
    /// The path the unit will take to move to the position.
    /// </summary>
    private List< Tile > _movementPath = new List< Tile >();

    /// <summary>
    /// Returns a copy of the movement path this unit will take as well as 
    /// the tile the unit is currently at.
    /// </summary>
    public List< Tile > movementPath
    {
        get
        {
            List<Tile> list = new List<Tile>( _movementPath );
            list.Add( World.GetTileAt( unit.coordinates ) );
            return list;
        }
    }

    /// <summary>
    /// The next stop the unit will move to in its movement.
    /// </summary>
    public TileCoordinates nextStop
    {
        get
        {
            if ( !moving )
            {
                throw new System.Exception( "Can't get the next stop while the unit isn't moving!" );
            }

            return movementPath[ 0 ].coordinates;
        }
    }

    //
    // Fields
    //

    /// <summary>
    /// The unit we're acting for.
    /// </summary>
    private readonly Unit unit;

    /// <summary>
    /// The day last measured for anything related to movement.
    /// </summary>
    private int lastMeasuredMovementDay = 0;

    //
    // Constructors
    //

    /// <summary>
    /// Creates a new UnitMovement object for handling the 
    /// </summary>
    /// <param name="unit"></param>
    internal UnitMovement( Unit unit )
    {
        this.unit = unit;
        moving = false;
        _range = unit.type.movementRange;

        GameTime.RegisterTimeAction( UpdateUnitRest );
    }

    //
    // Actions
    //

    /// <summary>
    /// Checks to see if the tile is open for the unit to move to.
    /// </summary>
    /// <returns>True if the unit can move to the tile.</returns>
    public bool CanMoveToTile( TileCoordinates coords )
    {
        List< Unit >[] unitLists = World.GetUnitsAt( coords );
        List< Unit > militaryUnits = unitLists[ 0 ];
        List< Unit > civilianUnits = unitLists[ 1 ];

        // if there are already military units there, we have to check on thei22wsr factions
        if ( unit.type.isMilitary && militaryUnits.Count != 0 )
        {
            // ensure there are not more than one of our faction in the position
            foreach ( Unit u in militaryUnits )
            {
                if ( u.faction == unit.faction ) return false;
            }
        }
        // don't allow civilian units of any factions to stack together
        if ( !unit.type.isMilitary && civilianUnits.Count != 0 ) return false;

        // we can move to this position!
        return true;
    }

    /// <summary>
    /// Gets all the tiles that are able to be moved to at the current time.
    /// </summary>
    /// <returns>The tiles within th emovement range of this unit.</returns>
    public HashSet< Tile > GetMovementRange()
    {
        Tile start = World.GetTileAt( unit.coordinates );

        HashSet< Tile > visited = new HashSet< Tile >();
        visited.Add( start );

        List< List< Tile > > fringes = new List< List< Tile > >();
        fringes.Add( new List< Tile >( new Tile[] { start } ) );

        // perform a breadth-first search
        for ( int i = 1; i <= range; i++ )
        {
            fringes.Add( new List< Tile >() ); // add an empty list that we can add to

            foreach ( Tile tile in fringes[ i - 1 ] )
            {
                for ( int j = 0; j < tile.neighbors.Length; j++ )
                {
                    Tile neighbor = tile.neighbors[ j ];
                    if ( neighbor == null || !neighbor.type.isWalkable ) continue;

                    if ( !visited.Contains( neighbor ) )
                    {
                        visited.Add( neighbor );
                        fringes[ i ].Add( neighbor );
                    }
                }
            }
        }
        
        return visited;
    }

    /// <summary>
    /// Gets the edges of the movement for a unit.
    /// </summary>
    /// <returns>The edges of the movement borders.</returns>
    public List< TileBorder > GetMovementBorders()
    {
        Tile start = World.GetTileAt( unit.coordinates );

        HashSet< Tile > visited = new HashSet< Tile >();
        visited.Add( start );

        List< List< Tile > > fringes = new List< List< Tile > >();
        fringes.Add( new List< Tile >( new Tile[] { start } ) );

        List< TileBorder > ends = new List< TileBorder >();

        // perform a breadth-first search
        for ( int i = 1; i <= range; i++ )
        {
            fringes.Add( new List< Tile >() ); // add an empty list that we can add to

            foreach ( Tile tile in fringes[ i - 1 ] )
            {
                for ( int j = 0; j < tile.neighbors.Length; j++ )
                {
                    Tile neighbor = tile.neighbors[ j ];

                    if ( neighbor == null || !neighbor.type.isWalkable )
                    {
                        ends.Add( new TileBorder( tile.coordinates, ( TileEdge ) j ) );
                        continue;
                    }

                    if ( !visited.Contains( neighbor ) )
                    {
                        visited.Add( neighbor );
                        fringes[ i ].Add( neighbor );
                    }
                }
            }
        }

        // bail early if we don't have enough movement cost or something
        if ( fringes.Count <= range ) return ends;

        foreach ( Tile tile in fringes[ range ] )
        {
            for ( int i = 0; i < tile.neighbors.Length; i++ )
            {
                Tile neighbor = tile.neighbors[ i ];

                if ( neighbor == null || !neighbor.type.isWalkable || !visited.Contains( neighbor ) )
                {
                    ends.Add( new TileBorder( tile.coordinates, ( TileEdge ) i ) );
                }
            }
        }

        // return only the tiles at the max range
        return ends;
    }

    /// <summary>
    /// Calculates the most efficient route to get to the given tile.
    /// </summary>
    /// <param name="goal">The goal tile to get to.</param>
    public void MoveTo( TileCoordinates goal )
    {
        _movementPath = PathTo( unit.coordinates, goal );
        CalculateArrivalDate();
        moving = true;
        World.InvalidateRangeMesh();
        GameTime.RegisterTimeAction( UpdateMovement );
    }

    /// <summary>
    /// Cancels any pending moving this unit was undergoing.
    /// </summary>
    public void Cancel()
    {
        moving = false;
        World.InvalidateRangeMesh();
    }

    //
    // Movement Leg-work
    //

    /// <summary>
    /// Calculates the path from the given tile to the goal.
    /// </summary>
    /// <param name="start">The position to start at.</param>
    /// <param name="goal">The position to end at.</param>
    /// <returns></returns>
    private List< Tile > PathTo( TileCoordinates startPos, TileCoordinates goalPos )
    {
        Tile start = World.GetTileAt( startPos );
        Tile end = World.GetTileAt( goalPos );

        if ( !GetMovementRange().Contains( end ) )
        {
            Debug.Log( "The goal position must be within the movement range!" );
            return new List< Tile >();
        }

        // A* cuz Dijkstra's wasn't good enough
        PriorityQueue< Tile > frontier = new PriorityQueue< Tile >();
        frontier.Add( 0, start );

        Dictionary< Tile, Tile > cameFrom = new Dictionary< Tile, Tile >();
        cameFrom[ start ] = null;

        Dictionary< Tile, int > costSoFar = new Dictionary< Tile, int >();
        costSoFar[ start ] = 0;

        while ( frontier.Count > 0 )
        {
            Tile current = frontier.RemoveMin();

            if ( current.coordinates.Equals( goalPos ) ) break;

            foreach ( Tile next in current.neighbors )
            {
                // skip tiles that don't real
                if ( next == null ) continue;
                if ( !next.type.isWalkable ) continue;

                int newCost = costSoFar[ current ] + next.type.movementDelay;

                if ( !costSoFar.ContainsKey( next ) || newCost < costSoFar[ next ] )
                {
                    costSoFar[ next ] = newCost;

                    int priority = newCost + ( int ) TileCoordinates.Distance( current.coordinates, next.coordinates );
                    frontier.Add( priority, next );

                    cameFrom[ next ] = current;
                }
            }
        }

        // construct the path end-->start
        List< Tile > path = new List< Tile >();
        path.Add( end ); // don't forget this, it makes a difference!
        Tile lastTile = end;

        Tile previous;
        while ( lastTile != null && ( previous = cameFrom[ lastTile ] ) != start )
        {
            path.Add( previous );
            lastTile = previous;
        }

        // reverse the path so it goes start-->end
        path.Reverse();

        return path;
    }

    /// <summary>
    /// Calculates the arrival date to the next tile in the trip.
    /// </summary>
    private void CalculateArrivalDate()
    {
        if ( movementPath.Count == 0 ) return;

        _arrivalDate = GameTime.day + movementPath[ 0 ].type.movementDelay;
    }

    /// <summary>
    /// Coroutine responsible for updating the movement of this unit,
    /// relative to the current day.
    /// </summary>
    /// <param name="day">The current day.</param>
    /// <returns>If the unit has finished moving.</returns>
    private bool UpdateMovement( int day )
    {
        // if movement was canceled, remove this action
        if ( !moving || _movementPath.Count == 0 )
        {
            moving = false;
            return true;
        }

        Tile nextTile = _movementPath[ 0 ];

        // if the unit has spent enough time in transit, then move the unit over
        if ( day >= arrivalDate )
        {
            if ( CanMoveToTile( nextTile.coordinates ) )
            {
                unit.coordinates = nextTile.coordinates; // move the unit on the map
                _movementPath.RemoveAt( 0 ); // remove the tile from our list
                _range--; // reduce the range for moving a tile

                // rebuild our meshes
                World.InvalidateUnitMesh();
                World.InvalidateRangeMesh();

                CalculateArrivalDate();
            }
            // for some reason, we can no longer go the specified tile, it's illegal
            else
            {
                Cancel();
                UI.ShowMessage( "Movement canceled for {0} at {1}",unit.type.name, unit.coordinates.ToString() );
            }
        }

        lastMeasuredMovementDay = day;

        moving = _movementPath.Count != 0;
        return !moving;
    }

    /// <summary>
    /// CoRoutine used to increase the units range while it isn't moving.
    /// </summary>
    /// <param name="day">The current day.</param>
    /// <returns></returns>
    private bool UpdateUnitRest( int day )
    {
        if ( moving ) return false;

        // increase the unit's range while it isn't moving
        if ( _range < unit.type.movementRange && ( day - lastMeasuredMovementDay ) >= unit.type.movementRegenRate )
        {
            _range++;

            // rebuild the range mesh if we're the selected unit
            if ( unit == World.selectedUnit )
            {
                World.InvalidateRangeMesh();
            }

            lastMeasuredMovementDay = day;
        }

        return false;
    }


}