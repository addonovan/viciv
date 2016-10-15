
/// <summary>
/// The edge of the tile that is being represented.
/// </summary>
public enum TileEdge
{
    NE, E, SE, SW, W, NW
}

/// <summary>
/// An extension class for the TileEdge enum.
/// </summary>
public static class TileEdgeExtensions
{
    public static TileEdge Opposite( this TileEdge direction )
    {
        // alas, modulo cannot work here :(
        return ( int ) direction >= 3 ? direction - 3 : direction + 3;
    }
}