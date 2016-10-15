using System.Linq;
using UnityEngine;

/// <summary>
/// A type of unit. These have different characteristics and abilities.
/// </summary>
public class UnitType
{

    //
    // Constants
    //

    public static readonly UnitType SETTLER = new UnitType( "Settler", "Settle", SettleCity, Color.cyan )
        .MarkCivilian()
        .SetMovementRange( 3 )
        .SetMovementRegenRate( 2 )
        .SetProductionTime( 45 );

    public static readonly UnitType WARRIOR = new UnitType( "Warrior", "Attack", AttackUnit, Color.red )
        .SetMovementRegenRate( 1 )
        .SetAttackRange( 1 )
        .SetProductionTime( 25 );

    //
    // Properties
    //

    private Color _color;

    /// <summary>
    /// The color to draw this unit as.
    /// </summary>
    public Color color
    {
        get
        {
            return _color;
        }
    }

    private string _name;

    /// <summary>
    /// The name of this type of unit.
    /// </summary>
    public string name
    {
        get
        {
            return _name;
        }
    }

    private bool _isMilitary = true;

    /// <summary>
    /// If this UnitType is a military unit or not.
    /// </summary>
    public bool isMilitary
    {
        get
        {
            return _isMilitary;
        }
    }

    private int _attackRange = 1;

    /// <summary>
    /// The number of tiles away this unit can attack from.
    /// </summary>
    public int attackRange
    {
        get
        {
            return _attackRange;
        }
    }

    private int _movementRange = 5;

    /// <summary>
    /// The number of tiles this unit could move at most.
    /// </summary>
    public int movementRange
    {
        get
        {
            return _movementRange;
        }
    }

    private int _productionTime = 15;

    /// <summary>
    /// The time it takes a province to produce a unit of this type.
    /// </summary>
    public int productionTime
    {
        get
        {
            return _productionTime;
        }
    }

    private int _movementRegenRate = 0;

    /// <summary>
    /// The number of days required to regenerate one movement.
    /// </summary>
    public int movementRegenRate
    {
        get
        {
            return _movementRegenRate;
        }
    }

    private string _specialDesc;

    /// <summary>
    /// The character for the special button label.
    /// </summary>
    public string specialDesc
    {
        get
        {
            return _specialDesc;
        }
    }

    /// <summary>
    /// A function that will perform the special action of this unit type for the given unit.
    /// </summary>
    /// <param name="unit">The unit to perform the action for.</param>
    public delegate void SpecialAction( Unit unit );

    private SpecialAction _specialAction;

    /// <summary>
    /// The method that will perform the unit type's special action.
    /// </summary>
    public SpecialAction specialAction
    {
        get
        {
            return _specialAction;
        }
    }

    //
    // Constructors
    //

    private UnitType( string name, string specialDesc, SpecialAction action, Color color )
    {
        _name = name;
        _specialDesc = specialDesc;
        _specialAction = action;
        _color = color;
    }

    //
    // Setup
    //

    private UnitType MarkCivilian()
    {
        _isMilitary = false;
        return this;
    }

    private UnitType SetAttackRange( int range )
    {
        _attackRange = range;
        return this;
    }

    private UnitType SetProductionTime( int productionTime )
    {
        _productionTime = productionTime;
        return this;
    }

    private UnitType SetMovementRange( int movementRange )
    {
        _movementRange = movementRange;
        return this;
    }

    private UnitType SetMovementRegenRate( int regenRate )
    {
        _movementRegenRate = regenRate;
        return this;
    }

    //
    // Equals & Hashcode
    //

    public override bool Equals( object obj )
    {
        UnitType type = obj as UnitType;
        if ( type == null ) return false;
        return name.Equals( type.name );
    }

    public override int GetHashCode()
    {
        return name.GetHashCode();
    }

    //
    // Special Actions
    //

    private static void SettleCity( Unit unit )
    {
        if ( unit.faction == Faction.player )
        {
            // show the dialog to name to province and then delete the unit
            UI.nameProvinceDialog.ShowDialog( s =>
            {
                World.CreateProvinceAt( unit.coordinates, unit.faction, s );
                World.DeleteUnit( unit );
            } );
        }
        else
        {
            // it's just a random string lol
            System.Random random = new System.Random();
            string name = new string( Enumerable.Repeat( "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8 ).Select( s => s[ random.Next( s.Length ) ] ).ToArray() );

            World.CreateProvinceAt( unit.coordinates, unit.faction, name );
            World.DeleteUnit( unit );
        }
    }

    private static void AttackUnit( Unit aggressor )
    {
        // el oh el, todo
    }

}
