using UnityEngine;
using System.Collections.Generic;

public class TileBorder
{

    //
    // Properties
    //

    /// <summary>
    /// The coordinates of this tile that's in range.
    /// </summary>
    public readonly TileCoordinates coordinates;

    /// <summary>
    /// The edges of this tile to draw.
    /// </summary>
    public readonly TileEdge edge;

    //
    // Constructors
    //

    public TileBorder( TileCoordinates coords, TileEdge direction )
    {
        coordinates = coords;
        edge = direction;
    }


}
