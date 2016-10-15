using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public static class World
{

    //
    // World Management
    //

    /// <summary>
    /// The world manager.
    /// </summary>
    private static WorldManager worldManager;

    /// <summary>
    /// The transform of the underlying world manager this class services.
    /// </summary>
    public static Transform transform
    {
        get
        {
            return worldManager.transform;
        }
    }

    /*
     * While I tend to shy away from using #regions to group sections of code and usually
     * just use my triple EOL comment method, this file is rather large and everything is already
     * grouped together, so I decided to #region it so that I wouldn't have to deal with having
     * to scroll to get to the part I want.
     */

    #region Dimensions

    /// <summary>
    /// The backing field for tileHeight.
    /// </summary>
    private static int _tileHeight = -1;

    /// <summary>
    /// The height of the world in tiles.
    /// </summary>
    public static int tileHeight
    {
        get
        {
            return _tileHeight;
        }
    }

    /// <summary>
    /// The backing field for tileWidth.
    /// </summary>
    public static int _tileWidth = -1;

    /// <summary>
    /// The width of the world in tiles.
    /// </summary>
    public static int tileWidth
    {
        get
        {
            return _tileWidth;
        }
    }

    /// <summary>
    /// The backing field for xLength.
    /// </summary>
    public static float _xLength = -1f;

    /// <summary>
    /// The length of the world in the x direction (as it appears on the screen).
    /// </summary>
    public static float xLength
    {
        get
        {
            return _xLength;
        }
    }

    /// <summary>
    /// The backing field for zLength.
    /// </summary>
    public static float _zLength = -1f;

    /// <summary>
    /// The length of the world in the z direction (as it appears on the screen).
    /// </summary>
    public static float zLength
    {
        get
        {
            return _zLength;
        }
    }

    #endregion

    #region Tiles

    /// <summary>
    /// The backing field for the tiles property.
    /// </summary>
    private static Tile[] _tiles;

    /// <summary>
    /// The tiles in this world.
    /// </summary>
    public static Tile[] tiles
    {
        get
        {
            return _tiles;
        }
    }

    /// <summary>
    /// True if some part of the tile mesh has changed and must be updated.
    /// </summary>
    private static volatile bool tileMeshValid;

    /// <summary>
    /// Invalidates the tile mesh so that it has to be rebuilt on the next
    /// frame update.
    /// </summary>
    public static void InvalidateTileMesh()
    {
        tileMeshValid = false;
        InvalidateProvinceMesh();
    }

    /// <summary>
    /// The tile with the given coordinates.
    /// </summary>
    /// <param name="coordinates">The coordinates of the tile.</param>
    /// <returns>The tile with the coordinates.</returns>
    public static Tile GetTileAt( TileCoordinates coordinates )
    {
        if ( coordinates.offsetX >= tileWidth || coordinates.offsetX < 0 ) return null;
        if ( coordinates.offsetZ >= tileHeight || coordinates.offsetZ < 0 ) return null;

        int index = coordinates.offsetX + ( coordinates.offsetZ * tileWidth );
        return tiles[ index ];
    }

    #endregion

    #region Factions

    /// <summary>
    /// The factions that are in this world.
    /// </summary>
    private static HashSet< Faction > _factions = new HashSet< Faction >();

    /// <summary>
    /// If this is true, then the faction mesh will be rebuilt as quickly as possible.
    /// </summary>
    private static bool factionMeshValid = false;

    /// <summary>
    /// Invalidates the faction mesh so that it will be rebuilt as quickly as possible.
    /// </summary>
    public static void InvalidateFactionMesh()
    {
        factionMeshValid = false;
    }

    /// <summary>
    /// Registers a faction with the world.
    /// </summary>
    /// <param name="faction">The faction to register with the world.</param>
    public static void RegisterFaction( Faction faction )
    {
        _factions.Add( faction );
    }

    #endregion

    #region Provinces

    /// <summary>
    /// A list of all provinces created at the moment.
    /// </summary>
    private static List< Province > _provinces = new List< Province >();

    /// <summary>
    /// Creates a province starting at the given position.
    /// </summary>
    /// <param name="coords">The starting location of the province.</param>
    /// <param name="faction">The faction to which this province belongs.</param>
    /// <param name="range">The radius of the new province.</param>
    public static void CreateProvinceAt( TileCoordinates coords, Faction faction, string name, int range = 3 )
    {
        Province province = worldManager.CreateProvince();
        province.faction = faction;
        province.name = name.Replace( " ", "\n" );

        // add territory within the range
        for ( int x = -range; x <= range; x++ )
        {
            for ( int z = -range; z <= range; z++ )
            {
                if ( Math.Abs( x + z ) > range ) continue;

                Tile tile = GetTileAt( new TileCoordinates( x + coords.x, z + coords.z ) );
                if ( tile == null || tile.faction != null ) continue; // if the tile doesn't exist or the tile is already owned

                province.AddTile( tile );
            }
        }

        _provinces.Add( province );
        faction.AddProvince( province );
        worldManager.CreateProvinceLabel( coords, name );

        InvalidateProvinceMesh();
    }

    /// <summary>
    /// Gets the province with the given name.
    /// </summary>
    /// <param name="name">The name of the province.</param>
    /// <returns>The province, or null if there isn't one.</returns>
    public static Province GetProvinceByName( string name )
    {
        foreach ( Province p in _provinces )
        {
            if ( p.name == name ) return p;
        }

        return null;
    }

    /// <summary>
    /// If the province mesh is valid or if it should be rebuilt.
    /// </summary>
    private static bool provinceMeshValid = false;

    /// <summary>
    /// Invalidates the province mesh so that it will be rebuilt.
    /// </summary>
    public static void InvalidateProvinceMesh()
    {
        provinceMeshValid = false;
        InvalidateFactionMesh();
    }

    #endregion

    #region Unit Range

    /// <summary>
    /// The backing field for selectedUnit
    /// </summary>
    private static Unit _selectedUnit;

    /// <summary>
    /// The currently selected Unit. This may be null if no unit is selected.
    /// </summary>
    public static Unit selectedUnit
    {
        get
        {
            return _selectedUnit;
        }
        set
        {
            if ( !units.Contains( value ) && value != null )
            {
                throw new ArgumentException( "Cannot set selected unit to a unit that is not in the unit list!" );
            }

            // update the selected value
            _selectedUnit = value;

            showUnitRange = false;
            InvalidateRangeMesh();
        }
    }

    /// <summary>
    /// True if some part of the unit range mesh has changed and must be updated.
    /// </summary>
    private static volatile bool rangeMeshValid;

    /// <summary>
    /// Invalidates the unit range mesh so that it must be rebuild on the next
    /// frame update.
    /// </summary>
    public static void InvalidateRangeMesh()
    {
        rangeMeshValid = false;
    }

    /// <summary>
    /// Backing field for showUnitRange
    /// </summary>
    private static bool _showUnitRange = false;

    /// <summary>
    /// If the unit's range should be shown or not.
    /// </summary>
    public static bool showUnitRange
    {
        get
        {
            return _showUnitRange;
        }
        set
        {
            _showUnitRange = value;
            InvalidateRangeMesh();
        }
    }

    #endregion

    #region Units

    /// <summary>
    /// The backing field for the units property.
    /// </summary>
    private static List< Unit > units;

    /// <summary>
    /// True if some part of the unit mesh has changed and must be updated.
    /// </summary>
    private static volatile bool unitMeshValid;

    /// <summary>
    /// Invalidates the unit mesh so that it must be rebuilt on the next
    /// frame update.
    /// </summary>
    public static void InvalidateUnitMesh()
    {
        unitMeshValid = false;
    }

    /// <summary>
    /// Gets the units at the given tile coordinate.
    /// 
    /// [0] - the list of military units at the given coordinates (if any).
    /// [1] - the list of civilian units at the given coordinates (if any).
    /// </summary>
    /// <param name="coordinates">The coordinates to get the units from.</param>
    /// <returns>The units at the given coordinates.</returns>
    public static List< Unit >[] GetUnitsAt( TileCoordinates coordinates )
    {
        List< Unit > civilianUnits = new List< Unit >();
        List< Unit > militaryUnits = new List< Unit >();

        foreach ( Unit unit in units )
        {
            if ( unit.faction == null ) continue;

            // skip units not at this position
            if ( !coordinates.Equals( unit.coordinates ) ) continue;

            // add the unit to the correct list
            if ( unit.type.isMilitary )
            {
                militaryUnits.Add( unit );
            }
            else
            {
                civilianUnits.Add( unit );
            }
        }

        return new List< Unit >[] { militaryUnits, civilianUnits };
    }

    /// <summary>
    /// Creates a new unit at the given position with the given type.
    /// </summary>
    /// <param name="type">The type of unit to create.</param>
    /// <param name="coordinates">The position to place the unit at.</param>
    /// <returns>The new unit.</returns>
    public static Unit CreateUnitAt( UnitType type, TileCoordinates coordinates, Faction faction )
    {
        Unit unit = worldManager.CreateUnit();
        unit.type = type;
        unit.coordinates = coordinates;
        unit.faction = faction;

        // the unit was placed there just fine
        units.Add( unit );
        return unit;
    }

    /// <summary>
    /// Removes the unit from all the lists and invalidates the unit mesh.
    /// </summary>
    /// <param name="unit">The unit to remove.</param>
    public static void DeleteUnit( Unit unit )
    {
        units.Remove( unit );
        unit.faction.RemoveUnit( unit );

        // unselect it if it's currently selected
        if ( selectedUnit == unit )
        {
            selectedUnit = null;
        }

        worldManager.KillIt( unit );
        InvalidateUnitMesh();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializaes the world based off of the given world manager.
    /// </summary>
    /// <param name="manager">The managing script for the world.</param>
    public static void InitializeWorld( WorldManager manager )
    {
        worldManager = manager;

        // grab the width and height of the world
        _tileHeight = manager.tileHeight;
        _tileWidth = manager.tileWidth;

        // calculate the real-world dimentions
        _xLength = _tileWidth * ( 1.5f * Tile.OUTER_RADIUS );
        _zLength = _tileHeight * ( 2f * Tile.INNER_RADIUS );

        // create the tile and unit containers
        _tiles = new Tile[ _tileWidth * _tileHeight ];
        units = new List< Unit >();

        // create the default cells
        for ( int index = 0, z = 0; z < _tileHeight; z++ )
        {
            for ( int x = 0; x < _tileWidth; x++, index++ )
            {
                CreateTile( x, z, index );
            }
        }
    }

    /// <summary>
    /// Generates the world.
    /// </summary>
    internal static void CreateWorld()
    {
        // create a new world
        worldManager.GenerateWorld();

        // we need to immediately rebuild these
        tileMeshValid = false;
        unitMeshValid = false;

        // start the routine to update the meshes
        worldManager.StartCoroutine( RebuildMeshes() );
    }

    /// <summary>
    /// Creates an ocean tile at the given index and places it in the
    /// tiles array.
    /// </summary>
    /// <param name="x">The x position of the tile.</param>
    /// <param name="z">The z position of the tile.</param>
    /// <param name="index">The index of the tile in the tiles array.</param>
    private static void CreateTile( int x, int z, int index )
    {
        int col = x;
        int row = z;

        // create the tile
        TileCoordinates coords = TileCoordinates.FromOffsetCoordinates( x, z );
        Vector3 position = coords.position;
        Tile tile = worldManager.CreateTile( coords );

        // assign lateral neighbors
        if ( col > 0 )
        {
            tile.SetNeighbor( TileEdge.W, tiles[ index - 1 ] );
        }

        // assign vertical neighbors
        if ( row > 0 )
        {
            // if the row is even
            if ( ( row & 1 ) == 0 )
            {
                // connect SouthEast neighbor
                tile.SetNeighbor( TileEdge.SE, tiles[ index - tileWidth ] );

                // connect SouthWest neighbor
                if ( col > 0 )
                {
                    tile.SetNeighbor( TileEdge.SW, tiles[ index - tileWidth - 1 ] );
                }
            }
            // odd
            else
            {
                // connect SouthWest neighbor
                tile.SetNeighbor( TileEdge.SW, tiles[ index - tileWidth ] );

                // connect SouthEast neighbor
                if ( x < tileWidth - 1 )
                {
                    tile.SetNeighbor( TileEdge.SE, tiles[ index - tileWidth + 1 ] );
                }
            }
        }

        // add the tile to the array
        _tiles[ index ] = tile;

        // create the label for the tile
        worldManager.CreateCoordinateLabel( tile.coordinates );
    }

    #endregion

    #region Coroutines

    private static IEnumerator RebuildMeshes()
    {
        yield return null; // wait one frame before trying anything

        // continually do this while the game is running
        while ( true )
        {

            if ( !tileMeshValid )
            {
                worldManager.RebuildTileMesh( _tiles );
                tileMeshValid = true;
            }

            if ( !provinceMeshValid )
            {
                worldManager.RebuildProvinceMesh( _provinces );
                provinceMeshValid = true;
            }

            if ( !factionMeshValid )
            {
                worldManager.RebuildFactionMesh( _factions );
                factionMeshValid = true;
            }

            // update the unit range mesh if we need to do so
            if ( !rangeMeshValid )
            {
                if ( selectedUnit != null )
                {
                    worldManager.RebuildRangeMesh( selectedUnit.movement.GetMovementBorders() );
                }
                else
                {
                    worldManager.RebuildRangeMesh( null );
                }
                rangeMeshValid = true;
            }

            if ( !unitMeshValid )
            {
                worldManager.RebuildUnitMesh( units );
                unitMeshValid = true; // we dispatched the update
            }


            yield return null; // wait until the next frame
        }
    }

    #endregion

}
