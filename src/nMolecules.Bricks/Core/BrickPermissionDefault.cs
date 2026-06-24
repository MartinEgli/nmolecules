

namespace NMolecules.Bricks
{
/// <summary>
    /// Defines the default behavior for dependencies not matched by any explicit
    /// rule. Use it to choose between permissive and allow-list style policies.
    /// </summary>
    public enum BrickPermissionDefault
    {
        /// <summary>Unmatched dependencies are allowed.</summary>
        Allow = 0,
        /// <summary>Unmatched dependencies are denied unless an explicit rule allows them.</summary>
        Deny = 1
    }
}
