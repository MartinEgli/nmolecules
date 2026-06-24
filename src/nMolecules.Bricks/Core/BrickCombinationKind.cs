

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes how two roles may be combined on one element. Use it to express
    /// whether roles can coexist, compete, or must never be combined.
    /// </summary>
    public enum BrickCombinationKind
    {
        /// <summary>The roles may be combined and reinforce each other.</summary>
        Additive = 0,
        /// <summary>Only one of the roles should win after precedence resolution.</summary>
        Exclusive = 1,
        /// <summary>The roles must not be combined on the same element.</summary>
        Incompatible = 2
    }
}
