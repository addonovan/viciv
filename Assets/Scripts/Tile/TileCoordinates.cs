using UnityEngine;
using UnityEditor;
using System;

[System.Serializable]
public class TileCoordinates
{

    //
    // Unity Properties
    //

    /// <summary>
    /// The X coordinate of the tile.
    /// </summary>
    public int x
    {
        get
        {
            return _x;
        }
    }

    /// <summary>
    /// The Y coordinate of the tile.
    /// </summary>
    public int y
    {
        get
        {
            return 0 - x - z;
        }
    }

    /// <summary>
    /// The Z coordinate of the tile.
    /// </summary>
    public int z
    {
        get
        {
            return _z;
        }
    }

    /// <summary>
    /// The offset x-axis position (useful for finding the position in an array).
    /// </summary>
    public int offsetX
    {
        get
        {
            return _x + ( _z / 2 );
        }
    }

    /// <summary>
    /// The offset z-axis position (useful for finding the position in an array)
    /// </summary>
    public int offsetZ
    {
        get
        {
            return _z;
        }
    }

    /// <summary>
    /// Creates a vector containing the coordinates of the the tile with these
    /// coordinates in the world.
    /// </summary>
    public Vector3 position
    {
        get
        {
            Vector3 position = new Vector3();

            position.x = ( offsetX + ( offsetZ * 0.5f ) - ( offsetZ / 2 ) ) * ( Tile.INNER_RADIUS * 2f );
            position.y = 0f;
            position.z = offsetZ * ( Tile.OUTER_RADIUS * 1.5f );

            return position;
        }
    }

    //
    // Fields
    //

    /// <summary>
    /// The backing field for the X property.
    /// </summary>
    [SerializeField]
    private int _x;

    /// <summary>
    /// The backing field for the Z property.
    /// </summary>
    [SerializeField]
    private int _z;

    //
    // Constructors
    //

    /// <summary>
    /// Creates a new object for holding the coordinates of the tile.
    /// </summary>
    /// <param name="x">The x position of the tile.</param>
    /// <param name="z">The z position of the tile.</param>
    public TileCoordinates( int x, int z )
    {
        _x = x;
        _z = z;
    }

    //
    // Actions
    //

    /// <summary>
    /// Finds the SubTileCoordinate
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public TileQuadrant QuadrantFromPosition( Vector3 position )
    {
        position -= this.position; // make the position relative to the center of the tile

        // determine the direction
        bool north = position.z >= 0;
        bool east = position.x >= 0;

        // return the correct quadrant based on these positions
        if ( north )
        {
            if ( east ) return TileQuadrant.NORTH_EAST;
            else return TileQuadrant.NORTH_WEST;
        }
        else
        {
            if ( east ) return TileQuadrant.SOUTH_EAST;
            else return TileQuadrant.SOUTH_WEST;
        }
    }

    //
    // ToStrings
    //

    /// <summary>
    /// The coordinate triple of the tile.
    /// </summary>
    /// <returns>The coordinate triple of the tile.</returns>
    public override string ToString()
    {
        return "(" + x + ", " + y + ", " + z + ")";
    }

    /// <summary>
    /// The tile coordinates, separated onto new lines.
    /// </summary>
    /// <returns>The coordinates separated onto new lines.</returns>
    public string ToSeparatedString()
    {
        return x + "\n" + y + "\n" + z;
    }

    //
    // Equals & Hashcode
    //

    /// <summary>
    /// Two TileCoordinates are equal iff their x and z values are equal.
    /// </summary>
    /// <param name="obj">The object to compare against.</param>
    /// <returns>If obj is a TileCoordinates object with the same x and z values.</returns>
    public override bool Equals( object obj )
    {
        TileCoordinates other = obj as TileCoordinates;

        return other != null && other._x == _x && other._z == _z;
    }

    /// <summary>
    /// Hashes this object.
    /// </summary>
    /// <returns>The hashcode of the the result of the ToString() method.</returns>
    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    //
    // Static
    //

    /// <summary>
    /// Creates a TileCoordinates Object from the regular offset coordinates.
    /// </summary>
    /// <param name="x">The x position (offset).</param>
    /// <param name="z">The y position (offset).</param>
    /// <returns>The TileCoordinates object.</returns>
    public static TileCoordinates FromOffsetCoordinates( int x, int z )
    {
        // note the minus sign
        // I accidentally put a plus and never noticed it until I got to raycasting and
        // debugged the FromPosition method for 2 days and suddenly realized that the
        // x and y axes were wrong and that was the only problem
        // UGHGHGHGAILSDUJFG;IAUSGFJLIKADSF HKL;AJFKJ;DLF
        // ; ASHV/OASKUHLCUKJVHZDKSXJLVH ASKDLFUYHASDLJHASN
        return new TileCoordinates( x - ( z / 2 ), z );
    }

    /// <summary>
    /// Grabs the tile coordinates from the position on the tile board.
    /// 
    /// This method also detects and corrects rounding errors.
    /// </summary>
    /// <param name="position">The position to find the cell at on the tile board mesh.</param>
    /// <returns>The TileCoordinates related to the position</returns>
    public static TileCoordinates FromPosition( Vector3 position )
    {
        float x = position.x / ( Tile.INNER_RADIUS * 2f );
        float y = -x;

        float offset = position.z / ( Tile.OUTER_RADIUS * 3f );
        x -= offset;
        y -= offset;

        // cast values to integers
        int iX = Mathf.RoundToInt( x );
        int iY = Mathf.RoundToInt( y );
        int iZ = Mathf.RoundToInt( 0 - x - y ); // all coordinates add to zero

        // in case of rounding errors:
        if ( iX + iY + iZ != 0 )
        {
            // calculate the differences in the variables from their rounded states
            float dX = Mathf.Abs( x - iX );
            float dY = Mathf.Abs( y - iY );
            float dZ = Mathf.Abs( ( 0 - x - y ) - iZ );

            if ( dX > dY && dX > dZ ) // iX sucks
            {
                iX = 0 - iY - iZ;
            }
            else if ( dZ > dY ) // dZ > dX implied by failure of last statement
            {
                iZ = 0 - iX - iY;
            }
            // iY is useless in this case so if it's messed up "whobody cares"?
        }

        return new TileCoordinates( iX, iZ );
    }

    /// <summary>
    /// Calculates the distance between the two tile coordinates.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float Distance( TileCoordinates a, TileCoordinates b )
    {
        return ( Mathf.Abs( a.x - b.x ) + Mathf.Abs( a.y - b.y ) + Mathf.Abs( a.z - b.z ) );
    }

}

/// <summary>
/// An enum representing one of four quadrants on the tile.
/// </summary>
[Flags]
public enum TileQuadrant
{
                     // N S E W
    NORTH_EAST = 10, // 1 0 1 0 = 10
    SOUTH_EAST =  6, // 0 1 1 0 =  6
    SOUTH_WEST =  5, // 0 1 0 1 =  5
    NORTH_WEST =  9  // 1 0 0 1 =  9
}

/// <summary>
/// Static extension class for adding methods to the TileQuadrant enum.
/// </summary>
public static class TileQuadrantExtensions
{
    public static bool IsNorth( this TileQuadrant enumVal )
    {
        return ( ( int ) enumVal & 8 ) != 0;
    }

    public static bool IsSouth( this TileQuadrant enumVal )
    {
        return ( ( int ) enumVal & 4 ) != 0;
    }

    public static bool IsEast( this TileQuadrant enumVal )
    {
        return ( ( int ) enumVal & 2 ) != 0;
    }

    public static bool IsWest( this TileQuadrant enumVal )
    {
        return ( ( int ) enumVal & 1 ) != 0;
    }
}

/// <summary>
/// A custom property drawer to display the TileCoordinates as read-only.
/// </summary>
[CustomPropertyDrawer( typeof( TileCoordinates ) )]
public class DrawerTileCoordinates : PropertyDrawer
{

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        TileCoordinates coords = new TileCoordinates(
            property.FindPropertyRelative( "_x" ).intValue,
            property.FindPropertyRelative( "_z" ).intValue
        );

        // update the label
        label.text = "Coordinates";
        position = EditorGUI.PrefixLabel( position, label );
        GUI.Label( position, coords.ToString() );
    }

}