using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class Tile : MonoBehaviour, IProperty
{

    //
    // Unity Properties
    //

    /// <summary>
    /// The coordinates of the tile in hex map mode.
    /// </summary>
    public TileCoordinates coordinates;

    //
    // Properties
    //

    /// <summary>
    /// The type of tile this is.
    /// </summary>
    public TileType type;

    /// <summary>
    /// The backing array for all of the bording tiles.
    /// 
    /// Woah, isn't it neat that neigh|bor|s and |bor|der, both have the
    /// 'bor' root in them? I wonder if that has something to do with proximity.
    /// </summary>
    public Tile[] neighbors = new Tile[ 6 ];

    //
    // Ownership
    //

    /// <summary>
    /// The current province controlling this tile
    /// </summary>
    public Province province
    {
        get;
        set;
    }

    /// <summary>
    /// The faction owning this tile (convenience method for owner.faction)
    /// </summary>
    public Faction faction
    {
        get
        {
            if ( province == null ) return null;

            return province.faction;
        }
        set
        {
            throw new System.Exception( "The faction of a tile may not be directly modified! It must be claimed by a province first!" );
        }
    }

    //
    // Game Properties
    //

    /// <summary>
    /// The backing field for baseFood.
    /// </summary>
    private int _baseFood = -1;

    /// <summary>
    /// The base amount of food production this tile can yield.
    /// </summary>
    public int baseFood
    {
        get
        {
            // if the base food hasn't been read yet, create it
            if ( _baseFood == -1 )
            {
                _baseFood = Random.Range( type.minimumFood, type.maximumFood + 1 );
            }

            return _baseFood;
        }
    }

    /// <summary>
    /// Convenience access for World.GetUnitsAt( tile.coordinates )
    /// </summary>
    public List< Unit >[] units
    {
        get
        {
            return World.GetUnitsAt( coordinates );
        }
    }

    //
    // Actions
    //

    /// <summary>
    /// Gets the neighbor to the given direction.
    /// </summary>
    /// <param name="direction">The direction to get the neighbor from.</param>
    /// <returns>The neighbor in the given direciton.</returns>
    public Tile GetNeighbor( TileEdge direction )
    {
        return neighbors[ ( int ) direction ];
    }

    /// <summary>
    /// Sets this cell's neighbor in the given direction to the given tile.
    /// Also reciprocates and sets the opposite neighbor of the given tile
    /// to this tile.
    /// </summary>
    /// <param name="direction">The direction `tile` is in.</param>
    /// <param name="tile">The new neighbor! Howdy!</param>
    public void SetNeighbor( TileEdge direction, Tile tile )
    {
        neighbors[ ( int ) direction ] = tile;

        // avoid NRE
        if ( tile == null ) return;

        tile.neighbors[ ( int ) direction.Opposite() ] = this; // update it for that tile as well
    }

    public void FocusOn()
    {
        Vector3 tilePos = coordinates.position;

        CameraControls cc = Camera.main.GetComponentInChildren< CameraControls >();

        // account for the angle of the camera

        //     /|
        //    / | 
        //   /  | height
        //  /)a |
        // -----+
        //  zDiff
        //
        // tan(a)=(height/zDiff)
        // zDiff = height cot(a)
        // (z_current - z_viewed) = height / tan(a)
        tilePos.z -= ( cc.transform.position.y / Mathf.Tan( Mathf.Deg2Rad * cc.pitch ) );
        tilePos.y = cc.transform.position.y;

        // update the position of the camera
        cc.transform.position = tilePos;
    }

    //
    // Equals & Hashcode
    //

    public override bool Equals( object o )
    {
        Tile tile = o as Tile;
        if ( tile == null ) return false;
        if ( !coordinates.Equals( tile.coordinates ) ) return false;
        if ( type != tile.type ) return false;
        return true;
    }

    public override int GetHashCode()
    {
        return coordinates.GetHashCode() + type.GetHashCode();
    }

    //
    // Constants
    //

    /// <summary>
    /// The radius from the center of a tile to one of its corners.
    /// </summary>
    public const float OUTER_RADIUS = 10f;

    /// <summary>
    /// The radius from the center of a tile to the middle of one of its edges.
    /// </summary>
    public const float INNER_RADIUS = OUTER_RADIUS * 0.866025404f /* Sqrt(3)/2*/;

}

[CustomEditor( typeof( Tile ) )]
public class EditorTile : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Tile tile = target as Tile;

        if ( GUILayout.Button( "Focus On" ) )
        {
            tile.FocusOn();
        }
    }

}