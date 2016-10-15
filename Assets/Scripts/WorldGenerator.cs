using System.Collections;
using UnityEngine;

/*
 * !!!WARNING!!!
 * This product contains code known to the State of California 
 * to cause cancer and birth defects or other reproductive harm.
 * !!!WARNING!!!
 */

/// <summary>
/// Woah now! Settle down there! Looking at this class will probably give you cancer!
/// Honestly, I have no idea how any of this works, I just kept adding random equations
/// with random constants to generate random numbers that made maps look better than
/// they did before more often.
/// </summary>
public static class WorldGenerator
{

    //
    // Fields
    //

    /// <summary>
    /// The seeded random number generator.
    /// </summary>
    private static System.Random random;

    //
    // Generation Functions
    //

    /// <summary>
    /// Generates a world with the given seed.
    /// 
    /// Precondition:
    ///     Every tile must have a TileType of `OCEANS`.
    /// </summary>
    /// <param name="tiles">The tiles to generate into a world.</param>
    /// <param name="seed">The world's seed (so this is replicable).</param>
    public static void GenerateWorld( Tile[] tiles, TileMesh tileMesh, int seed )
    {
        // create a new random number generator based on the input seed
        random = new System.Random( seed );

        int landmass = CreateLandmasses( tiles );
        CreateCoasts( tiles );
        CreateMountains( tiles, landmass );
        CreateDeserts( tiles, landmass );

        tileMesh.Rebuild( tiles );
    }

    /// <summary>
    /// Creates numerous landmasses across the map.
    /// </summary>
    /// <param name="tiles"></param>
    private static int CreateLandmasses( Tile[] tiles )
    {
        int landmasses = random.Next( 2, 3 );

        int range = Mathf.RoundToInt( Mathf.Sqrt( tiles.Length / ( float ) landmasses ) );

        int landmass = 0;

        // generate each continent
        for ( int i = 0; i < landmasses; i++ )
        {
            // find a tile that isn't in range of a pre-existing plains tile
            Tile tile;

            int attempt = 0;
            do
            {
                tile = tiles[ random.Next( 0, tiles.Length ) ];
            }
            while ( IsLandInRange( tile, 5 ) && ++attempt <= 10 );

            int size = CreateLandmass( tile, range );

            landmass += size;
        }

        return landmass;
    }

    /// <summary>
    /// Creates a landmass starting at the given tile.
    /// </summary>
    /// <param name="tile">The starting point of the continent</param>
    private static int CreateLandmass( Tile tile, int range )
    {
        // set this tile's type
        tile.type = TileType.PLAINS;

        int sum = 1;

        // stop this branch whenever the range gets too low
        if ( range >= random.Next( 0, 5 ) )
        {
            // create continents at the neighbors
            foreach ( Tile neighbor in tile.neighbors )
            {
                if ( neighbor == null || neighbor.type.isLand ) continue;

                sum += CreateLandmass( neighbor, range - random.Next( 1, 5 ) );
            }
        }

        return sum;
    }

    /// <summary>
    /// This pass will create mountain ranges.
    /// </summary>
    /// <param name="tiles">The tiles.</param>
    private static void CreateMountains( Tile[] tiles, int landmass )
    {
        int mountainRanges = random.Next( 1, 5 ) * random.Next( 1, ( landmass / 50 ) + 2 );

        int range = ( int ) ( 2 * Mathf.Sqrt( landmass / mountainRanges ) );

        for ( int i = 0; i < mountainRanges; i++ )
        {
            // find a random land tile by guessing
            Tile start;
            do
            {
                start = tiles[ random.Next( 0, tiles.Length ) ]; // choose a random tile
            }
            while ( !start.type.isLand );

            // choose a random direction
            TileEdge direction = ( TileEdge ) random.Next( 0, 5 );

            int size = CreateMountainRangeAt( start, range, direction );
        }
    }

    /// <summary>
    /// Creates a mountain range starting at the given type and spreading out the
    /// given range.
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    private static int CreateMountainRangeAt( Tile tile, int range, TileEdge direction )
    {
        // if the tile doesn't exist, isn't land, or already is a mountain, end the chain
        if ( tile == null || !tile.type.isLand || tile.type == TileType.MOUNTAIN ) return 0;

        // set the type to mountains
        tile.type = TileType.MOUNTAIN;

        int sum = 1;

        if ( range >= random.Next( 0, 5 ) )
        {
            // expand in the given direction
            CreateMountainRangeAt( tile.GetNeighbor( direction ), range - random.Next( 0, 5 ), direction );
            
            // expand backwards too
            CreateMountainRangeAt( tile.GetNeighbor( direction.Opposite() ), range - random.Next( 0, 5 ), direction );

            // create mountains in the given directions
            for ( int i = 0; i < 6; i++ )
            {
                // expand mostly in the given direction and the opposite direction
                if ( i == ( int ) direction || i == ( int ) direction.Opposite() )
                {
                    CreateMountainRangeAt( tile.GetNeighbor( ( TileEdge ) i ), range - random.Next( 0, 5 ), direction );
                }
                // 70% chance that it will branch off in another direction, but will suffer massive hits to the range
                else if ( random.Next( 1, 10 ) <= 7 )
                {
                    CreateMountainRangeAt( tile.GetNeighbor( ( TileEdge ) i ), range - random.Next( 5, 15 ), direction );
                }
            }
        }

        return sum;
    }

    /// <summary>
    /// Creates all the deserts.
    /// </summary>
    /// <param name="tiles">The tile map.</param>
    /// <param name="landmass">The amount of land tiles that were generated.</param>
    private static void CreateDeserts( Tile[] tiles, int landmass )
    {
        int deserts = random.Next( 2, 6 ) * random.Next( 1, ( landmass / 100 ) + 2 );

        int range = ( int ) Mathf.Sqrt( landmass / deserts );

        for ( int i = 0; i < deserts; i++ )
        {
            Tile start;
            do
            {
                start = tiles[ random.Next( 0, tiles.Length ) ]; // choose a random tile
            }
            while ( !start.type.isWalkable && start.type != TileType.DESERT );

            int size = CreateDesertAt( start, range );
        }
    }

    /// <summary>
    /// Creates a desert starting at the given tile with the given range.
    /// </summary>
    /// <param name="tile">The tile to start at.</param>
    /// <param name="range">The range of the desert.</param>
    /// <returns>The number of tiles that were turned to deserts.</returns>
    private static int CreateDesertAt( Tile tile, int range )
    {
        if ( tile == null || tile.type == TileType.DESERT ) return 0;

        int sum = 1;
        tile.type = TileType.DESERT;

        if ( range >= random.Next( 0, 5 ) )
        {
            foreach ( Tile neighbor in tile.neighbors )
            {
                sum += CreateDesertAt( neighbor, range - random.Next( 3, 8 ) );
            }
        }

        return sum;
    }

    /// <summary>
    /// This pass will create coastal tiles.
    /// </summary>
    /// <param name="tiles">The tiles.</param>
    private static void CreateCoasts( Tile[] tiles )
    {
        int delta = 1;

        // repeat this at most ten times
        for ( int i = 0; i < 10 && delta > 0; i++, delta = 0 )
        {
            foreach ( Tile tile in tiles )
            {
                if ( tile.type != TileType.OCEAN ) continue;

                // the weight given to our conversions
                int shouldConvert = 0;

                if ( IsLandInRange( tile, 2 ) ) shouldConvert += 1;
                if ( IsLandInRange( tile, 1 ) ) shouldConvert += 3;

                int rand = random.Next( 0, 3 );

                // randomly choose if we should convert based on our weightings
                if ( shouldConvert >= rand )
                {
                    tile.type = TileType.COAST;
                    delta++;
                }
            }
        }
    }

    //
    // Helper Functions
    //

    /// <summary>
    /// Checks to see if there is a tile of the given type in the given range in the
    /// given direction.
    /// 
    /// This is an incredibly recursive function, but only the neighbors with the given direction
    /// will be searched.
    /// </summary>
    /// <param name="tile">The tile to start at.</param>
    /// <param name="type">The type of tile to search for.</param>
    /// <param name="range">The range to search.</param>
    /// <param name="direction">The direction to look in.</param>
    /// <returns></returns>
    private static bool IsLandInRange( Tile tile, int range, Direction direction = Direction.ALL )
    {
        if ( tile == null ) return false;
        if ( range <= 0 ) return tile.type.isLand;

        // it's easier to just use recursion four more times
        if ( direction == Direction.ALL )
        {
            return
                IsLandInRange( tile, range, Direction.NORTH ) ||
                IsLandInRange( tile, range, Direction.SOUTH ) ||
                IsLandInRange( tile, range, Direction.EAST )  ||
                IsLandInRange( tile, range, Direction.WEST );
        }
        
        range--;

        switch ( direction )
        {
            case Direction.NORTH:
                return 
                    IsLandInRange( tile.GetNeighbor( TileEdge.NE ), range, direction ) ||
                    IsLandInRange( tile.GetNeighbor( TileEdge.NW ), range, direction );

            case Direction.EAST:
                return
                    IsLandInRange( tile.GetNeighbor( TileEdge.E ), range, direction );

            case Direction.SOUTH:
                return
                    IsLandInRange( tile.GetNeighbor( TileEdge.SE ), range, direction ) ||
                    IsLandInRange( tile.GetNeighbor( TileEdge.SW ), range, direction );

            case Direction.WEST:
                return
                    IsLandInRange( tile.GetNeighbor( TileEdge.W ), range, direction );
        }

        return false;
    }

    /// <summary>
    /// Just a simple enum for the IsLandInRange direction.
    /// </summary>
    private enum Direction
    {
        NORTH, SOUTH, EAST, WEST, ALL
    }

}
