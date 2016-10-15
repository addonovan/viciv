using UnityEngine;

public class ProvinceMesh : TessellatedMesh< Province >
{

    protected override string meshName
    {
        get
        {
            return "Province Mesh";
        }
    }

    protected override void RebuildMesh( Province[] provinces, Tessellator tessellator )
    {
        foreach ( Province province in provinces )
        {
            foreach ( Tile t in province.property )
            {
                for ( int i = 0; i < t.neighbors.Length; i++ )
                {
                    Tile neighbor = t.neighbors[ i ];

                    // don't draw a border here
                    // if the tile doesn't exist OR the tile isn't owned by this faction
                    //   (the faction border will be drawn here)
                    // OR if the tile is still owned by this province
                    if ( neighbor == null || neighbor.faction != province.faction || neighbor.province == province ) continue;

                    // draw the province's border in black to set it off from other province territories
                    tessellator.Tessellate2D( t.coordinates.position, Tile.OUTER_RADIUS + 0.5f, 0.5f, ( TileEdge ) i, province.faction.secondaryColor );
                }
            }
        }
    }
}
