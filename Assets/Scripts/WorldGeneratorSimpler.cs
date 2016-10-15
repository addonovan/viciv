using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// A much simpler world generater that won't generate any oceans
/// </summary>
public static class WorldGeneratorSimpler
{

    public static void GenerateWorld( Tile[] tiles, TileMesh tileMesh, int seed )
    {
        // call the more complicated world generator because I don't want to rewrite anything
        WorldGenerator.GenerateWorld( tiles, tileMesh, seed );

        Random random = new Random( seed );

        // convert all water tiles to random other tiles
        foreach ( Tile t in tiles )
        {
            if ( t.type == TileType.OCEAN || t.type == TileType.COAST )
            {
                switch ( random.Next( 0, 10 ) )
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        t.type = TileType.DESERT;
                        break;

                    case 4:
                    case 5:
                        t.type = TileType.MOUNTAIN;
                        break;

                    default:
                        t.type = TileType.PLAINS;
                        break;
                }
            }
        }

        tileMesh.Rebuild( tiles );
    }

}
