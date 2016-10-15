using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TileRangeMesh : TessellatedMesh< TileBorder >
{

    //
    // Overrides
    //

    protected override string meshName
    {
        get
        {
            return "Tile Range Mesh";
        }
    }

    /// <summary>
    /// Creates the triangles for all of the tiles.
    /// </summary>
    /// <param name="tileRanges">The tile ranges to tessellate.</param>
    /// <param name="tessellator">The tessellator used to draw the mesh.</param>
    protected override void RebuildMesh( TileBorder[] tileRanges, Tessellator tessellator )
    {
        foreach ( TileBorder tile in tileRanges )
        {
            tessellator.Tessellate2D( tile.coordinates.position, Tile.OUTER_RADIUS + 2f, 2f, tile.edge, World.selectedUnit.type.color );
        }
    }

}
