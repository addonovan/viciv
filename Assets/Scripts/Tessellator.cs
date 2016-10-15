using System.Collections.Generic;
using UnityEngine;

public class Tessellator
{

    //
    // Constants
    //

    /// <summary>
    /// The positions of all the corners on a unit hexagon.
    /// </summary>
    public static readonly Vector3[] UNIT_HEXAGON =
    {
        // bottom
        new Vector3(            0f, 0f,  1      ),
        new Vector3(  0.866025404f, 0f,  1 / 2f ),
        new Vector3(  0.866025404f, 0f, -1 / 2f ),
        new Vector3(            0f, 0f, -1      ),
        new Vector3( -0.866025404f, 0f, -1 / 2f ),
        new Vector3( -0.866025404f, 0f,  1 / 2f ),
        new Vector3(            0f, 0f,  1      ) // prevents out of bounds errors
    };


    //
    // Fields
    //

    /// <summary>
    /// A list of all the points that make up the vertices of a hexagon.
    /// </summary>
    private List< Vector3 > vertices = new List< Vector3 >();

    /// <summary>
    /// The triangles to draw.
    /// </summary>
    private List< int > triangles = new List< int >();

    /// <summary>
    /// The colors of each vertex.
    /// </summary>
    private List< Color > colors = new List< Color >();

    /// <summary>
    /// The mesh to use for updating and clearning and stuff.
    /// </summary>
    private Mesh mesh;

    //
    // Constructors
    //

    /// <summary>
    /// Constructs a new HexTiler object to service the given mesh.
    /// </summary>
    /// <param name="mesh">The mesh to service.</param>
    public Tessellator( Mesh mesh )
    {
        this.mesh = mesh;
    }

    //
    // Actions
    //

    /// <summary>
    /// Clears this HexTiler so it can be reused.
    /// </summary>
    public void Clear()
    {
        mesh.Clear();
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
    }

    /// <summary>
    /// Updates the mesh to resemble this HexTiler object.
    /// </summary>
    public void UpdateMesh()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
    }

    /// <summary>
    /// Creates a hexagonal prism centered around the given location with the given radius
    /// and color.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="outerRadius"></param>
    /// <param name="insideRadius"></param>
    /// <param name="color"></param>
    public void Tessellate3D( Vector3 center, float outerRadius, float height, Color color )
    {
        Vector3 offset = new Vector3( 0f, height, 0f );

        // create a container for all the vertices
        HexPrismFaces faces = new HexPrismFaces( UNIT_HEXAGON, offset, outerRadius );

        // we're going to skip drawing the base as it will never be visible unless the camera
        // glitches out

        // oh this is fun!
        // tessellate the squares that make the sides of the unit
        for ( int i = 0; i < 6; i++ )
        {
            // create the four corners of the prism we're going to tessellate
            int a = i;
            int b = ( a + 1 ) % 6;
            int c = a + 6;
            int d = b + 6;

            // a->c->b
            AddTriangle(
                center + faces.v[ b ],
                center + faces.v[ c ],
                center + faces.v[ a ]               
            );
            AddTriangleColor( color );

            // b->c->d
            AddTriangle(
                center + faces.v[ d ],
                center + faces.v[ c ],
                center + faces.v[ b ]
            );
            AddTriangleColor( color );
        }

        // create the top face
        Tessellate2D( center + offset, outerRadius, color );
    }

    /// <summary>
    /// Creates a hex tile of the given color centered about the given position.
    /// </summary>
    /// <param name="center">The center of the tile.</param>
    /// <param name="outerRadius">The radius of the hexagon.</param>
    /// <param name="color">The color of the hexagon.</param>
    public void Tessellate2D( Vector3 center, float outerRadius, Color color )
    {
        for ( int i = 0; i < UNIT_HEXAGON.Length - 1; i++ )
        {
            // scale the unit hexagon up to the size of the outer radius
            AddTriangle(
                center,
                center + ( UNIT_HEXAGON[ i ] * outerRadius ),
                center + ( UNIT_HEXAGON[ i + 1 ] * outerRadius )
            );
            AddTriangleColor( color );
        }
    }

    /// <summary>
    /// Tessellates a single part of the hexagonal outline with the given position, color, and thickness.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="outerRadius"></param>
    /// <param name="thickness"></param>
    /// <param name="direction"></param>
    /// <param name="color"></param>
    public void Tessellate2D( Vector3 center, float outerRadius, float thickness, TileEdge direction, Color color )
    {
        float innerRadius = outerRadius - thickness;

        int i = ( int ) direction;

        AddTriangle(
                center + ( UNIT_HEXAGON[ i ] * innerRadius ),
                center + ( UNIT_HEXAGON[ i ] * outerRadius ),
                center + ( UNIT_HEXAGON[ i + 1 ] * innerRadius )
            );
        AddTriangleColor( color );

        AddTriangle(
            center + ( UNIT_HEXAGON[ i ] * outerRadius ),
            center + ( UNIT_HEXAGON[ i + 1 ] * outerRadius ),
            center + ( UNIT_HEXAGON[ i + 1 ] * innerRadius )
        );
        AddTriangleColor( color );
    }

    /// <summary>
    /// Adds the points to the vertices lists and then adds the the
    /// vertex indices to the triangles list. Makes a stupidly boring
    /// task slightly less so.
    /// </summary>
    /// <param name="v1">The first vertex.</param>
    /// <param name="v2">The second.</param>
    /// <param name="v3">The third.</param>
    private void AddTriangle( Vector3 v1, Vector3 v2, Vector3 v3 )
    {
        int vertexIndex = vertices.Count;
        vertices.Add( v1 );
        vertices.Add( v2 );
        vertices.Add( v3 );
        triangles.Add( vertexIndex );
        triangles.Add( vertexIndex + 1 );
        triangles.Add( vertexIndex + 2 );
    }

    /// <summary>
    /// Adds the color to the colors list three times, once for each vertex
    /// on the triangle.
    /// </summary>
    /// <param name="color">The color of the triangle.</param>
    private void AddTriangleColor( Color color )
    {
        colors.Add( color );
        colors.Add( color );
        colors.Add( color );
    }

    //
    // Structs
    //

    /// <summary>
    /// A struct for the faces on a hexagonal prism.
    /// </summary>
    private struct HexPrismFaces
    {
        /// <summary>
        /// The array of vertices. Named v to keep stuff short.
        /// 
        /// 0-5 are the bottom face.
        /// 6-11 are the top face.
        /// </summary>
        public readonly Vector3[] v;

        //
        // Constructor
        //

        public HexPrismFaces( Vector3[] bottomFace, Vector3 faceOffset, float scale = 1f )
        {
            v = new Vector3[ 12 ];

            // pull the bottom face's v[ertices from the array
            v[ 0 ] = bottomFace[ 0 ] * scale;
            v[ 1 ] = bottomFace[ 1 ] * scale;
            v[ 2 ] = bottomFace[ 2 ] * scale;
            v[ 3 ] = bottomFace[ 3 ] * scale;
            v[ 4 ] = bottomFace[ 4 ] * scale;
            v[ 5 ] = bottomFace[ 5 ] * scale;

            // add the face offset v[ector to the bottom face's v[ertices to get the v[ertices of the top face
            // these don't need to be scaled as they're base vectors already have been
            v[ 6 ] = v[ 0 ] + faceOffset;
            v[ 7 ] = v[ 1 ] + faceOffset;
            v[ 8 ] = v[ 2 ] + faceOffset;
            v[ 9 ] = v[ 3 ] + faceOffset;
            v[ 10 ] = v[ 4 ] + faceOffset;
            v[ 11 ] = v[ 5 ] + faceOffset;
        }

    }

}
