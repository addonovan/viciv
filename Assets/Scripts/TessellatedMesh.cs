using UnityEngine;
using System.Collections.Generic;

[RequireComponent( typeof( MeshFilter ), typeof( MeshRenderer ) )]
public abstract class TessellatedMesh< T > : MonoBehaviour
{

    /// <summary>
    /// The mesh to be used for tessllation to display the entities.
    /// </summary>
    private Mesh mesh;

    /// <summary>
    /// The MeshCollider used by the mesh.
    /// </summary>
    private MeshCollider meshCollider;

    /// <summary>
    /// The tessellator used for generating the triangles of this mesh.
    /// </summary>
    private Tessellator tessellator;

    /// <summary>
    /// The name of the mesh
    /// </summary>
    abstract protected string meshName
    {
        get;
    }

    //
    // Unity Hooks
    //

    public void Awake()
    {
        // create the mesh
        GetComponent< MeshFilter >().mesh = ( mesh = new Mesh() );
        mesh.name = meshName;

        // add the collider
        meshCollider = gameObject.AddComponent<MeshCollider>();

        // create our tessellator
        tessellator = new Tessellator( mesh );
    }

    //
    // Abstract
    //

    /// <summary>
    /// Rebuilds the mesh based off of the given entities.
    /// </summary>
    /// <param name="entities">The entities to include in this mesh.</param>
    public void Rebuild( HashSet< T > entities )
    {
        T[] entityArray = new T[ entities.Count ];
        entities.CopyTo( entityArray );
        Rebuild( entityArray );
    }

    /// <summary>
    /// Rebuilds the mesh based off of the given entities.
    /// </summary>
    /// <param name="entities">The entities to include in this mesh.</param>
    public void Rebuild( List< T > entities )
    {
        Rebuild( entities.ToArray() );
    }

    /// <summary>
    /// Rebilds the mesh based off of the given entities.
    /// </summary>
    /// <param name="entities">The entities to include in this mesh.</param>
    public void Rebuild( T[] entities )
    {
        // update the rendering
        tessellator.Clear();
        RebuildMesh( entities, tessellator );
        tessellator.UpdateMesh();

        // update the mesh collider
        meshCollider.sharedMesh = mesh;
    }

    /// <summary>
    /// Rebuilds the mesh for the list of entities and with the given tessellator.
    /// 
    /// This is called by the Rebuild method which handles the cleanup and updating
    /// of the mesh.
    /// </summary>
    /// <param name="entities">The entities to include in this mesh.</param>
    /// <param name="tessellator">The tessellator to use to create the mesh.</param>
    protected abstract void RebuildMesh( T[] entities, Tessellator tessellator );


}
