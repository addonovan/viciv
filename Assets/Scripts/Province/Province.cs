using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// A province that holds claim to some tiles on the map, while also being
/// claimable by a faction.
/// </summary>
public class Province : MonoBehaviour, IProperty
{

    //
    // Faction Ownership
    //

    /// <summary>
    /// The current faction in control of this province.
    /// </summary>
    public Faction faction
    {
        get;
        set;
    }

    //
    // Tiles
    //

    private List< Tile > _tiles = new List< Tile >();

    /// <summary>
    /// The territory owned by this province.
    /// </summary>
    public ReadOnlyCollection< Tile > property
    {
        get
        {
            return _tiles.AsReadOnly();
        }
    }

    /// <summary>
    /// Adds a tile to the control of this province.
    /// </summary>
    /// <param name="t"></param>
    public void AddTile( Tile t )
    {
        _tiles.Add( t );
        t.province = this;
    }

    /// <summary>
    /// Removes a tile from control of this province.
    /// </summary>
    /// <param name="t"></param>
    public void RemoveTile( Tile t )
    {
        _tiles.Remove( t );
        t.province = null;
    }

    //
    // Occupation
    //

    private bool _underOccupation = false;

    /// <summary>
    /// If this province is currently under occupation by enemy forces.
    /// </summary>
    public bool underOccupation
    {
        get
        {
            return _underOccupation;
        }
    }

    private int _occupationStartDay = -1;

    /// <summary>
    /// The day that the occupation started on
    /// </summary>
    public int occupationStartDay
    {
        get
        {
            return _occupationStartDay;
        }
    }


    //
    // Unit Production
    //

    private Dictionary< int, List< UnitType > > unitProduction = new Dictionary< int, List< UnitType > >();

    /// <summary>
    /// Adds a unit of the given type to the production queue of this province.
    /// </summary>
    /// <param name="type">The type of unit to produce.</param>
    public void Produce( UnitType type )
    {
        int finishedDay = GameTime.day + type.productionTime;

        // add the unit to the production queue
        GetProductionQueueForDay( finishedDay ).Add( type );
    }

    /// <summary>
    /// Gets (and creates, if neccessary) the production queue for the given day.
    /// </summary>
    /// <param name="day">The day to fetch the queue for.</param>
    /// <returns>The production queue for the day.</returns>
    private List< UnitType > GetProductionQueueForDay( int day )
    {
        if ( !unitProduction.ContainsKey( day ) )
        {
            unitProduction[ day ] = new List< UnitType >();
        }
        return unitProduction[ day ];
    }

    //
    // Unity Hooks
    //

    private void Awake()
    {
        // register our updating routine with the system
        GameTime.RegisterTimeAction( OnDayChange );
    }

    //
    // Coroutines
    //

    /// <summary>
    /// Update province data every time the day changes.
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    private bool OnDayChange( int day )
    {
        bool wasUnderOccupation = _underOccupation;
        _underOccupation = false; // reset this

        Faction occupyingFaction = null;

        // check to see if we're under occupation
        foreach ( Tile t in _tiles )
        {
            // iterate over the military units
            foreach ( Unit u in t.units[ 0 ] )
            {
                // there is a unit...
                if ( u == null ) continue;

                // ...and its faction is different than ours...
                if ( u.faction == faction ) continue;

                // ...THEN WE'RE UNDER OCCUPATION AAAAHHHHH
                _underOccupation = true;

                occupyingFaction = u.faction;
                break;
            }

            // break out of this loop too
            if ( _underOccupation ) break;
        }

        // if the occupation just begun, set the occupation start day
        if ( _underOccupation && !wasUnderOccupation )
        {
            _occupationStartDay = day;

            if ( faction == Faction.player )
            {
                UI.ShowMessage( "{0} is under hostile occupation!", name );
            }
        }


        // invalidate the occupation start date if we're no longer under occupation
        if ( !_underOccupation && wasUnderOccupation )
        {
            _occupationStartDay = -1;

            if ( faction == Faction.player )
            {
                UI.ShowMessage( "{0} no longer under hostile occupation", name );
            }
        }


        // if we've been under occupation for 40 days, then flip to the other faction
        if ( _underOccupation && day - occupationStartDay >= 40 )
        {
            // reset any data specific to this faction
            _underOccupation = false;
            _occupationStartDay = -1;
            unitProduction.Clear();

            if ( faction == Faction.player )
            {
                UI.ShowMessage( "Lost {0} to the enemy!", name );
            }

            if ( occupyingFaction == Faction.player )
            {
                UI.ShowMessage( "Captured {0}!", name );
            }

            // flip the province
            occupyingFaction.TakeProvince( this );
        }

        // create the units to produce on this day
        if ( unitProduction.ContainsKey( day ) )
        {
            // create the unit types
            List< UnitType > units = unitProduction[ day ];

            // if we're under occupation...
            if ( underOccupation )
            {
                UI.ShowMessage( "Can't produce units in " + name + " delaying!" );
                
                // delay them by a day
                GetProductionQueueForDay( day + 1 ).AddRange( units );
            }
            // ... otherwise, produce the units
            else
            {
                // create all the units
                foreach ( UnitType type in units )
                {
                    // find a place to place them
                    bool placed = false;
                    foreach ( Tile t in _tiles )
                    {
                        if ( !t.type.isWalkable ) continue;

                        if ( type.isMilitary )
                        {
                            // if there's a military unit here, skip it
                            if ( t.units[ 0 ].Count != 0 ) continue;

                            World.CreateUnitAt( type, t.coordinates, faction );
                            placed = true;
                            break;
                        }
                        else
                        {
                            // if there's a civilian unit here, skip it
                            if ( t.units[ 1 ].Count != 0 ) continue;

                            World.CreateUnitAt( type, t.coordinates, faction );
                            placed = true;
                            break;
                        }
                    }

                    // there was absolutely no place for us to place a unit???
                    if ( !placed )
                    {
                        // queue it for another day
                        GetProductionQueueForDay( day + 1 ).Add( type );
                    }
                }
            }

            // delete this list from the queue
            unitProduction.Remove( day );
        }

        return false; // never deregister ourselves
    }

}
