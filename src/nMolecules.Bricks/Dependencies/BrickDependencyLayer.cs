

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes the observation layer where a dependency was found. Use it to
    /// separate compile-time references from visibility, runtime, and configuration links.
    /// </summary>
    public enum BrickDependencyLayer
    {
        /// <summary>A static code dependency such as a type reference, inheritance, or member usage.</summary>
        Static = 0,
        /// <summary>A visibility dependency such as friend assemblies or internal exposure.</summary>
        Visibility = 1,
        /// <summary>A runtime dependency such as activation, reflection, plugin loading, or service resolution.</summary>
        Runtime = 2,
        /// <summary>A configuration dependency such as DI registration, config binding, or policy wiring.</summary>
        Configuration = 3
    }
}
