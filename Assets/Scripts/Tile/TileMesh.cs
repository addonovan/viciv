using UnityEngine;

public class TileMesh : TessellatedMesh< Tile >
{

    //
    // Overrides
    //

    protected override string meshName
    {
        get
        {
            return "Tile Mesh";
        }
    }

    /// <summary>
    /// Creates the triangles for all of the tiles.
    /// </summary>
    /// <param name="tiles">The tiles to tessellate.</param>
    /// <param name="tessellator">The tessellator used to draw the mesh.</param>
    protected override void RebuildMesh( Tile[] tiles, Tessellator tessellator )
    {
        foreach ( Tile tile in tiles )
        {
            tessellator.Tessellate2D( tile.transform.localPosition, Tile.OUTER_RADIUS, tile.type.color );
        }
    }

}
