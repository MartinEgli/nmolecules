

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes how imported policies are merged into a root policy. Use it
    /// when composing platform defaults, package policies, and project-specific policies.
    /// </summary>
    public enum BrickPolicyImportMode
    {
        /// <summary>Import the referenced policy as-is.</summary>
        Import = 0,
        /// <summary>Add the referenced policy to the root policy.</summary>
        Extend = 1,
        /// <summary>Let the importing policy replace overlapping imported decisions.</summary>
        Override = 2,
        /// <summary>Disable the referenced imported policy.</summary>
        Disable = 3,
        /// <summary>Keep only stricter or narrower imported behavior.</summary>
        Narrow = 4
    }
}
