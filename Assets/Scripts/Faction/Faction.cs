using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class Faction
{

    //
    // Properties
    //

    /// <summary>
    /// The name of this faction.
    /// </summary>
    public readonly string name;

    /// <summary>
    /// The primary (major) color of this faction.
    /// </summary>
    public readonly Color primaryColor;

    /// <summary>
    /// The secondary (minor) color of this faction.
    /// </summary>
    public readonly Color secondaryColor;

    //
    // Provinces
    //

    private readonly List< Province > _provinces = new List< Province >();

    /// <summary>
    /// The provinces controlled by this faction.
    /// </summary>
    public ReadOnlyCollection< Province > provinces
    {
        get
        {
            return _provinces.AsReadOnly();
        }
    }

    /// <summary>
    /// Adds a province to this faction's control.
    /// </summary>
    /// <param name="province"></param>
    public void AddProvince( Province province )
    {
        _provinces.Add( province );
        _tiles.AddRange( province.property ); // add all of the province's property to this
        province.faction = this;

        World.InvalidateProvinceMesh();
    }

    /// <summary>
    /// Removes a province from this faction's control.
    /// </summary>
    /// <param name="province"></param>
    public void RemoveProvince( Province province )
    {
        _provinces.Remove( province );

        // remove all the province's property from the control
        foreach ( Tile t in province.property )
        {
            _tiles.Remove( t );
        }

        province.faction = null;

        World.InvalidateProvinceMesh();
    }

    /// <summary>
    /// Takes the province from the current owner and sets the owner to this faction.
    /// </summary>
    /// <param name="province">The province to take</param>
    public void TakeProvince( Province province )
    {
        province.faction.RemoveProvince( province );
        AddProvince( province );
    }

    //
    // Units
    //

    private readonly List< Unit > _units = new List< Unit >();

    /// <summary>
    /// The units owned by this faction.
    /// </summary>
    public ReadOnlyCollection< Unit > units
    {
        get
        {
            return _units.AsReadOnly();
        }
    }

    /// <summary>
    /// Adds a unit to this faction's control.
    /// </summary>
    /// <param name="unit"></param>
    public void AddUnit( Unit unit )
    {
        if ( units.Contains( unit ) ) return;

        Debug.Log( name + " +U " + unit.type.name );
        _units.Add( unit );
        unit.faction = this;
    }

    /// <summary>
    /// Removes a unit from this faction's control.
    /// </summary>
    /// <param name="unit"></param>
    public void RemoveUnit( Unit unit )
    {
        if ( !_units.Contains( unit ) ) return;

        Debug.Log( name + " -U " + unit.type.name );
        _units.Remove( unit );
        unit.faction = null;
    }

    //
    // Tiles
    //

    private readonly List< Tile > _tiles = new List< Tile >();

    /// <summary>
    /// The tiles owned by this faction.
    /// </summary>
    public ReadOnlyCollection< Tile > tiles
    {
        get
        {
            return _tiles.AsReadOnly();
        }
    }

    //
    // Constructors
    //

    /// <summary>
    /// Creates a new faction with the given name.
    /// </summary>
    /// <param name="name">The name of the faction.</param>
    /// <param name="primary">The primary color of the faction.</param>
    /// <param name="secondary">The secondary color of the faction.</param>
    private Faction( string name, Color primary, Color secondary )
    {
        this.name = name;
        primaryColor = primary;
        secondaryColor = secondary;

        World.RegisterFaction( this );
    }

    //
    // Artificial "Intelligence"
    // 

    // "oh, it's artificial alright" -me

    /// <summary>
    /// Sets up the AI.
    /// </summary>
    /// <returns>This faction so the call can be chained.</returns>
    private Faction MarkAsAI()
    {
        GameTime.RegisterTimeAction( Tick );
        return this;
    }

    private bool Tick( int day )
    {
        for ( int i = _units.Count - 1; i >= 0; i-- )
        {
            Unit u = _units[ i ];
            if ( u == null ) continue;

            // randomly decide to move if we can
            if ( !u.movement.moving && u.movement.range > 0 && Random.Range( 0, 10 ) <= 5 )
            {
                HashSet< Tile > possibleDestinations = u.movement.GetMovementRange();
                
                // choose a random one, because why no
                Tile t = possibleDestinations.ElementAt( Random.Range( 0, possibleDestinations.Count ) );

                u.movement.MoveTo( t.coordinates );
            }

            if ( u.type == UnitType.SETTLER && World.GetTileAt( u.coordinates ).faction == null )
            {
                // settle the city
                if ( Random.Range( 0, 10 ) == 2 || day == 0 )
                {
                    u.type.specialAction( u );
                }
            }
        }

        foreach ( Province p in provinces )
        {
            // occasionally produce a random unit in a province
            if ( Random.Range( 0, 100 ) == 2 || day == 0 )
            {
                p.Produce( Random.Range( 0, 2 ) == 0 ? UnitType.SETTLER : UnitType.WARRIOR );
            }
        }

        return false; // never deregister
    }

    //
    // Static Access
    //

    /// <summary>
    /// A dictionary that contains the name of the faction mapped to the faction itself.
    /// </summary>
    private static Dictionary< string, Faction > factions = new Dictionary< string, Faction >();

    public static Faction player = CreateFaction( "player", Color.blue, Color.white );

    public static Faction enemy = CreateFaction( "enemy", Color.black, Color.red ).MarkAsAI();

    /// <summary>
    /// Gets the faction with the given name. A new faction will be created
    /// if none with the given name currently exists.
    /// 
    /// The check for the name is case-insensitive; however the name will
    /// retain the cases if it is created. The name string is also trimmed
    /// to prevent excess whitespace from breaking anything.
    /// </summary>
    /// <param name="name">The name of the faction.</param>
    /// <returns>The faction with the given name.</returns>
    public static Faction GetFactionByName( string name )
    {
        // don't let this be null because I hate null stuff
        if ( name == null ) throw new System.NullReferenceException( "Faction name cannot be null!" );

        // return the pre-existing faction
        return factions[ name.Trim().ToLower() ];
    }

    /// <summary>
    /// Creates a faction with the given primary and secondary colors.
    /// </summary>
    /// <param name="name">The name of the faction.</param>
    /// <param name="primary">The primary color of the faction.</param>
    /// <param name="secondary">The secondary color of the faction.</param>
    /// <returns>The created faction.</returns>
    public static Faction CreateFaction( string name, Color primary, Color secondary )
    {
        if ( factions.ContainsKey( name ) ) throw new System.Exception( "Faction with the name " + name + " has already been created!" );

        Faction faction = new Faction( name, primary, secondary );
        factions[ name.Trim().ToLower() ] = faction;
        return faction;
    }

}
