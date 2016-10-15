using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class WorldManager : MonoBehaviour
{

    //
    // (Unity Set) World dimensions
    //

    /// <summary>
    /// The height of the world (in tiles).
    /// </summary>
    public int tileHeight = 6;

    /// <summary>
    /// The width of the world (in tiles).
    /// </summary>
    public int tileWidth = 6;

    //
    // (Unity Set) Prefabs
    //

    /// <summary>
    /// The prefab used to generate new tiles from.
    /// </summary>
    public Tile tilePrefab;

    /// <summary>
    /// The prefab used to generate new units from.
    /// </summary>
    public Unit unitPrefab;
    
    /// <summary>
    /// The prefab used to generate new cities from.
    /// </summary>
    public Province provincePrefab;

    /// <summary>
    /// The prefab used to generate labels for the tiles from. 
    /// </summary>
    public Text tileLabelPrefab;

    /// <summary>
    /// The prefab used to generate labels for the provinces.
    /// </summary>
    public Text provinceLabelPrefab;

    //
    // (Unity Set) UI information
    //

    /// <summary>
    /// The panel responsible for displaying information about a selected tile.
    /// </summary>
    public UITilePanel uiTilePanel;

    /// <summary>
    /// The panel responsible for displaying information about a selected unit.
    /// </summary>
    public UIUnitPanel uiUnitPanel;

    /// <summary>
    /// The panel responsible for getting province names and whatnot.
    /// </summary>
    public UINameProvinceDialog uiProvinceName;

    /// <summary>
    /// The panel responsible for letting the user choose from a list of provinces.
    /// </summary>
    public UIChooseProvinceDialog uiChooseProvince;

    /// <summary>
    /// The event system for the UI.
    /// </summary>
    public EventSystem eventSystem;

    /// <summary>
    /// Canvas used to draw the tile labels onto.
    /// </summary>
    public Canvas coordinateOverlay;

    /// <summary>
    /// Canvas used to draw the province labels.
    /// </summary>
    public Canvas provinceLabels;

    //
    // Meshes
    //

    /// <summary>
    /// The TileMesh used to create the meshes for the tiles.
    /// </summary>
    private TileMesh tileMesh;

    /// <summary>
    /// The FactionMesh used to create the meshes for factions.
    /// </summary>
    private FactionMesh factionMesh;

    /// <summary>
    /// The ProvinceMesh used to create the meshes for provinces.
    /// </summary>
    private ProvinceMesh provinceMesh;

    /// <summary>
    /// Mesh used to show selected tiles.
    /// </summary>
    private TileRangeMesh tileRangeMesh;

    /// <summary>
    /// The UnitMesh used to create the meshes for the units.
    /// </summary>
    private UnitMesh unitMesh;

    //
    // Unity Hooks
    //

    private void Awake()
    {
        // grab components
        tileMesh          = GetComponentInChildren< TileMesh      >();
        factionMesh       = GetComponentInChildren< FactionMesh   >();
        provinceMesh      = GetComponentInChildren< ProvinceMesh  >();
        tileRangeMesh     = GetComponentInChildren< TileRangeMesh >();
        unitMesh          = GetComponentInChildren< UnitMesh      >();

        // initialize the world
        World.InitializeWorld( this );
        UI.Initialize( this );
    }

    private void Start()
    {
        World.CreateWorld();

        for ( int j = 0; j < 2; j++ )
        {
            Faction faction = j == 0 ? Faction.enemy : Faction.player;

            // randomly choose a place for the player's units to go
            int i = -1;
            do
            {
                i = Random.Range( 0, World.tiles.Length );
            }
            while ( !World.tiles[ i ].type.isWalkable );

            TileCoordinates coords = World.tiles[ i ].coordinates;
            World.CreateUnitAt( UnitType.SETTLER, coords, faction );
            World.CreateUnitAt( UnitType.WARRIOR, coords, faction );

            World.GetTileAt( coords ).FocusOn();
        }

        GameTime.running = false; // pause the game
    }

    private void Update()
    {
        // print the selected tile's coordinates
        if ( Controls.click && !eventSystem.IsPointerOverGameObject() )
        {
            Vector3 clickedPos; // the position that was clicked in the world

            // if we clicked something
            if ( DoRaycast( out clickedPos ) )
            {
                HandlePrimaryClick( clickedPos );
            }
        }

        // toggle the coordinate overlay
        if ( Controls.showTileCoordinates )
        {
            ShowTransform( coordinateOverlay );
        }
        else
        {
            HideTransform( coordinateOverlay );
        }

        if ( Controls.unitDeselect )
        {
            World.selectedUnit = null;
            World.InvalidateRangeMesh();
        }

        if ( Controls.unitFocus )   uiUnitPanel.CenterOnUnit();
        if ( Controls.unitMove )    uiUnitPanel.MoveUnit();
        if ( Controls.unitSpecial ) uiUnitPanel.PerformUnitSpecial();
    }

    //
    // Input Handlers
    //

    /// <summary>
    /// Handles a click with the primary mouse button.
    /// </summary>
    /// <param name="position">The position that was returned by a raycast.</param>
    private void HandlePrimaryClick( Vector3 position )
    {
        TileCoordinates coords = TileCoordinates.FromPosition( position ); // get the coordinates of the tile
        TileQuadrant quadrant = coords.QuadrantFromPosition( position ); // get the quadrant that was clicked
        Tile tile = World.GetTileAt( coords ); // get the tile at the given coordinates

        // handle moving a unit, if the conditions are met
        // and if the unit wasn't moved, then 
        if ( !HandleUnitMove( tile ) )
        {
            // get the unit that was clicked on
            if ( !HandleUnitClick( GetUnitInTile( coords, quadrant ) ) )
            {
                // update the tile information in the gui
                HandleTileClick( tile );
            }
        }
    }

    /// <summary>
    /// Handles moving a unit to the specified tile.
    /// </summary>
    /// <param name="tile">The tile to move the unit to.</param>
    /// <returns>If the unit was moved or not.</returns>
    private bool HandleUnitMove( Tile tile )
    {
        if ( World.selectedUnit == null || !World.showUnitRange ) return false;
        if ( tile == World.GetTileAt( World.selectedUnit.coordinates ) ) return false; // clicked the same tile

        // check each tile in the unit's range to see if they clicked on a tile in the range
        foreach ( Tile t in World.selectedUnit.movement.GetMovementRange() )
        {
            // the clicked on a tile in the range, move the unit there
            if ( tile.Equals( World.GetTileAt( t.coordinates ) ) )
            {
                World.selectedUnit.MoveTo( tile.coordinates );
                return true;
            }
        }

        return false; // clicked outside of movement range
    }

    /// <summary>
    /// Handles a click that selected a unit.
    /// </summary>
    /// <param name="unit">The unit that was selected.</param>
    private bool HandleUnitClick( Unit unit )
    {
        if ( unit == null ) return false;

        // if the unit is null or was already selected, unmark th eunit
        if ( unit.Equals( World.selectedUnit ) && unit.faction == Faction.player )
        {
            World.selectedUnit = null;
        }
        // otherwise mark the unit
        else
        {
            World.selectedUnit = unit;
        }

        return true;
    }

    /// <summary>
    /// Handles a click that selected a tile.
    /// </summary>
    /// <param name="tile">The tile that was selected.</param>
    private void HandleTileClick( Tile tile )
    {
        uiTilePanel.ToggleTileInformation( tile );
    }

   

    /// <summary>
    /// Gets the unit in the given tile coordinate and quadrant.
    /// </summary>
    /// <param name="coordinates">The tile that was clicked.</param>
    /// <param name="quadrant">The quadrant of the tile that was clicked.</param>
    /// <returns></returns>
    private Unit GetUnitInTile( TileCoordinates coordinates, TileQuadrant quadrant )
    {
        // ensure that the quadrant is in the east, where the units are
        if ( quadrant.IsWest() ) return null;

        List< Unit >[] units = World.GetUnitsAt( coordinates );

        // if the user clicked in the north quadrant, check for military units of our faction (0)
        // otherwise check for civilian units of our faction (1)
        foreach ( Unit u in units[ quadrant.IsNorth() ? 0 : 1 ] )
        {
            if ( u.faction == Faction.player )
            {
                return u;
            }
        }

        return null;
    }

    //
    // Actions
    //

    /// <summary>
    /// Hides the transform of the component by setting its scale to zero.
    /// 
    /// Yes, this is an awful hack, but I can't find a better way to do it.
    /// </summary>
    /// <param name="component">The component to hide.</param>
    private void HideTransform( Component component )
    {
        component.transform.localScale = Vector3.zero;
    }

    /// <summary>
    /// Shows the transform by setting its local scale the the given scale.
    /// </summary>
    /// <param name="component">The component to show.</param>
    /// <param name="xScale">(Optional, default 1) The x scale of the component's transform.</param>
    /// <param name="yScale">(Optional, default 1) The y scale of the component's transform.</param>
    /// <param name="zScale">(Optional, default 1) The z scale of the component's transform.</param>
    private void ShowTransform( Component component, float xScale = 1f, float yScale = 1f, float zScale = 1f )
    {
        component.transform.localScale = new Vector3( xScale, yScale, zScale );
    }

    /// <summary>
    /// Generates the world.
    /// 
    /// All tiles will first be set to water before passing the tiles off to the world
    /// generator though.
    /// </summary>
    internal void GenerateWorld()
    {
        // reset the world to water
        foreach ( Tile tile in World.tiles )
        {
            tile.type = TileType.OCEAN;
        }

        WorldGeneratorSimpler.GenerateWorld( World.tiles, tileMesh, Random.Range( int.MinValue, int.MaxValue ) );
    }

    /// <summary>
    /// Performs a raycast and returns if the raycast hit something.
    /// </summary>
    /// <param name="hitLocation">The position that was clicked.</param>
    /// <returns>True iff the raycast was successful.</returns>
    internal bool DoRaycast( out Vector3 hitLocation )
    {
        Ray input = Camera.main.ScreenPointToRay( Controls.mousePosition );
        RaycastHit hit;

        bool hitSomething = Physics.Raycast( input, out hit );
        hitLocation = transform.InverseTransformDirection( hit.point ); // find where the ray hit

        return hitSomething;
    }

    //
    // Mesh Rebuilding
    //

    /// <summary>
    /// Rebuilds the tile mesh with the given tiles.
    /// </summary>
    /// <param name="tiles">The tile to rebuild the mesh with.</param>
    internal void RebuildTileMesh( Tile[] tiles )
    {
        tileMesh.Rebuild( tiles );
    }

    /// <summary>
    /// Rebuilds the faction mesh with the given factions.
    /// </summary>
    /// <param name="factions">The factions currently in play which need to have their borders rendered.</param>
    internal void RebuildFactionMesh( HashSet< Faction > factions )
    {
        factionMesh.Rebuild( factions );
    }

    /// <summary>
    /// Rebuild sthe province mesh with the given provinces.
    /// </summary>
    /// <param name="provinces">The provinces to build.</param>
    internal void RebuildProvinceMesh( List< Province > provinces )
    {
        provinceMesh.Rebuild( provinces );
    }

    /// <summary>
    /// Rebuilds the unit mesh with the given units.
    /// </summary>
    /// <param name="units">The units to rebuild the mesh with.</param>
    internal void RebuildUnitMesh( List< Unit > units )
    {
        unitMesh.Rebuild( units );
    }

    /// <summary>
    /// Rebuilds the range mesh with the given tiles.
    /// </summary>
    /// <param name="range">The range.</param>
    internal void RebuildRangeMesh( List< TileBorder > range )
    {
        // hide the mesh if we're meant to build an empty one or not supposed to show one
        if ( range == null || range.Count == 0 || !World.showUnitRange || World.selectedUnit == null )
        {
            tileRangeMesh.transform.position = new Vector3( 0f, -500f, 0f );
        }
        // otherwise build the mesh and show it
        else
        {
            tileRangeMesh.Rebuild( range );
            tileRangeMesh.transform.position = new Vector3( 0f, 0.2f, 0f );
        }
    }

    //
    // Prefab Instantiation
    //

    /// <summary>
    /// </summary>
    /// <returns>A fresh new unit.</returns>
    internal Unit CreateUnit()
    {
        Unit unit = Instantiate( unitPrefab );
        unit.transform.SetParent( unitMesh.transform );

        return unit;
    }

    /// <summary>
    /// Creates a new ocean tile at the given tile coordinate.
    /// </summary>
    /// <param name="coords">The coordinates of the tile to create.</param>
    /// <returns>The created tile.</returns>
    internal Tile CreateTile( TileCoordinates coords )
    {
        Tile tile = Instantiate( tilePrefab );

        // set the postiion and type
        tile.transform.SetParent( tileMesh.transform, false );
        tile.transform.localPosition = coords.position;
        tile.coordinates = coords;
        tile.type = TileType.OCEAN;

        return tile;
    }

    /// <summary>
    /// Creates a blank city.
    /// </summary>
    /// <returns>A fresh new city.</returns>
    internal Province CreateProvince()
    {
        Province prefab = Instantiate( provincePrefab );
        prefab.transform.SetParent( provinceMesh.transform, false );
        return prefab;
    }

    /// <summary>
    /// Creates and attaches the label for the tile with the given tile coordinates.
    /// </summary>
    /// <param name="coords">The coordinates of the tile.</param>
    internal void CreateCoordinateLabel( TileCoordinates coords )
    {
        Vector3 position = coords.position;

        Text label = Instantiate( tileLabelPrefab );
        label.rectTransform.SetParent( coordinateOverlay.transform, false );
        label.rectTransform.anchoredPosition = new Vector2( position.x, position.z );
        label.text = coords.ToSeparatedString();
    }

    /// <summary>
    /// Creates and attaches the label for the provinces at the given position.
    /// </summary>
    /// <param name="coords">The position of the label.</param>
    /// <param name="name">The name of the province.</param>
    internal void CreateProvinceLabel( TileCoordinates coords, string name )
    {
        Vector3 position = coords.position;

        Text label = Instantiate( provinceLabelPrefab );
        label.rectTransform.SetParent( provinceLabels.transform, false );
        label.rectTransform.anchoredPosition = new Vector2( position.x, position.z );
        label.text = string.Format( "<b>{0}</b>", name );
    }

    //
    // Destruction
    //

    /// <summary>
    /// Destroys the object.
    /// </summary>
    /// <param name="mb"></param>
    public void KillIt( MonoBehaviour mb )
    {
        Destroy( mb.gameObject );
    }

}
