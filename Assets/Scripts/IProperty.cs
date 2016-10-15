using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// An interface for something that can have faction owners.
/// </summary>
public interface IProperty
{

    /// <summary>
    /// The current owner controlling this piece of land.
    /// </summary>
    Faction faction
    {
        get;
        set;
    }

}
