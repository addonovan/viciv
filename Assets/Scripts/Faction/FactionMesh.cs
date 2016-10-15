
/// <summary>
/// The faction mesh, draws the borders of a faction.
/// </summary>
public class FactionMesh : TessellatedMesh< Faction >
{

    protected override string meshName
    {
        get
        {
            return "Faction mesh";
        }
    }

    protected override void RebuildMesh( Faction[] factions, Tessellator tessellator )
    {
        // find and draw the borders of each faction
        foreach ( Faction faction in factions )
        {
            // find the borders and draw the lines
            foreach ( Tile t in faction.tiles )
            {
                for ( int i = 0; i < t.neighbors.Length; i++ )
                {
                    Tile neighbor = t.neighbors[ i ];

                    // if the tile exists AND is owned by this faction, skip it
                    if ( neighbor != null && neighbor.faction == faction ) continue;

                    // this is a border edge, draw it now!
                    tessellator.Tessellate2D( t.coordinates.position, Tile.OUTER_RADIUS + 2f, 2f, ( TileEdge ) i, faction.primaryColor );
                }
            }
        }
    }

}
