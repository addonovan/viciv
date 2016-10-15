using UnityEngine;

public class UnitMesh : TessellatedMesh< Unit >
{

    //
    // Abstract
    //

    protected override string meshName
    {
        get
        {
            return "Unit Mesh";
        }
    }

    /// <summary>
    /// Rebuilds the unit mehs with the given units.
    /// </summary>
    /// <param name="units">The units to include in this mesh.</param>
    /// <param name="tessellator">The tessellator used to draw the mesh.</param>
    protected override void RebuildMesh( Unit[] units, Tessellator tessellator )
    {
        foreach ( Unit unit in units )
        {
            Color color = unit.type.isMilitary ? unit.faction.primaryColor : unit.faction.secondaryColor;
            tessellator.Tessellate3D( transform.position + unit.position, Unit.OUTER_RADIUS, Unit.HEIGHT, color );
        }
    }

}
