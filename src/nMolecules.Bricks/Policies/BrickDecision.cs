

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes the policy decision a rule makes for matching dependencies.
    /// Use it to model allowed, forbidden, and required architecture relations.
    /// </summary>
    public enum BrickDecision
    {
        /// <summary>The matching dependency is allowed.</summary>
        Allow = 0,
        /// <summary>The matching dependency is forbidden.</summary>
        Deny = 1,
        /// <summary>The matching dependency is required and missing evidence may become a violation.</summary>
        Require = 2
    }
}
