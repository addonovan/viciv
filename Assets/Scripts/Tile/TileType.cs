using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[System.Serializable]
public class TileType
{

    //
    // Constants
    //

    /// <summary>
    /// A list of all the tiles created and useable in the game.
    /// </summary>
    private static readonly List< TileType > ALL_TILES = new List< TileType >();

    public static readonly TileType OCEAN =
        new TileType( new Color( 0f, 0f, 1f ), "Ocean" )
            .SetIsLand( false )
            .SetFoodRange( 0, 10 );


    public static readonly TileType COAST =
        new TileType( new Color( 0.25f, 0.25f, 1f ), "Coast" )
            .SetIsLand( false )
            .SetFoodRange( 1, 20 );


    public static readonly TileType PLAINS =
        new TileType( Color.green, "Plains" )
            .SetFoodRange( 5, 50 );


    public static readonly TileType DESERT =
        new TileType( Color.yellow, "Desert" )
            .SetFoodRange( 0, 10 )
            .SetMovementDelay( 2 );


    public static readonly TileType MOUNTAIN =
        new TileType( new Color( 205f / 255f, 133f / 255f, 36f / 255f ), "Mountain" )
            .SetIsWalkable( false )
            .SetFoodRange( 0, 5 );

    //
    // Static Access
    //

    /// <summary>
    /// </summary>
    /// <param name="index">The index of the tile in the list.</param>
    /// <returns></returns>
    public static TileType GetTile( int index )
    {
        return ALL_TILES[ index ];
    }

    //
    // Properties
    //

    /// <summary>
    /// The backing field for the color property.
    /// </summary>
    private Color _color;

    /// <summary>
    /// The color of the tile type.
    /// </summary>
    public Color color
    {
        get
        {
            return _color;
        }
    }

    /// <summary>
    /// Backing field for the name property.
    /// </summary>
    [SerializeField]
    private string _name;

    /// <summary>
    /// The name of the tile type as a string.
    /// </summary>
    public string name
    {
        get
        {
            return _name;
        }
    }

    /// <summary>
    /// Backing field for isLand.
    /// </summary>
    private bool _isLand = true;

    /// <summary>
    /// If this tile type is land or water.
    /// </summary>
    public bool isLand
    {
        get
        {
            return _isLand;
        }
    }

    /// <summary>
    /// Backing field for isWalkable.
    /// </summary>
    private bool _isWalkable = true;

    /// <summary>
    /// If this tile type is able to be walked over.
    /// </summary>
    public bool isWalkable
    {
        get
        {
            return _isWalkable;
        }
    }

    /// <summary>
    /// Backing field for movementDelay.
    /// </summary>
    private int _movementDelay = 1;

    /// <summary>
    /// The number of days it takes to traverse this tile.
    /// </summary>
    public int movementDelay
    {
        get
        {
            return _movementDelay;
        }
    }

    /// <summary>
    /// The backing field for minimum food.
    /// </summary>
    private int _minimumFood = 0;

    /// <summary>
    /// The minimum base amount of food this tile type can produce.
    /// </summary>
    public int minimumFood
    {
        get
        {
            return _minimumFood;
        }
    }

    /// <summary>
    /// The backing field for maximumFood.
    /// </summary>
    private int _maximumFood = 0;

    /// <summary>
    /// The maximum base amount of food this tile type can produce.
    /// </summary>
    public int maximumFood
    {
        get
        {
            return _maximumFood;
        }
    }

    //
    // Constructors
    //

    private TileType( Color tileColor, string name )
    {
        _color = tileColor;
        _name = name;

        // add ourselves to the tiles list
        ALL_TILES.Add( this );
    }

    //
    // Attribute-Setters
    //

    private TileType SetIsLand( bool isLand )
    {
        _isLand = isLand;
        return SetIsWalkable( false ); // allows for chaining
    }

    private TileType SetIsWalkable( bool isWalkable )
    {
        _isWalkable = isWalkable;
        return this;
    }

    private TileType SetFoodRange( int min, int max )
    {
        _minimumFood = min;
        _maximumFood = max;
        return this;
    }

    private TileType SetMovementDelay( int movementDelay )
    {
        _movementDelay = movementDelay;
        return this;
    }

}

[CustomPropertyDrawer( typeof( TileType ) )]
public class DrawerTileType : PropertyDrawer
{

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        string name = property.FindPropertyRelative( "_name" ).stringValue;

        // update the label
        label.text = "Terrain";
        position = EditorGUI.PrefixLabel( position, label );
        GUI.Label( position, name );
    }

}