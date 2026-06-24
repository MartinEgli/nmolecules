

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes the authority of a role assignment. Use it as one part of
    /// precedence when resolving aliases, external policy, generated facts, and direct annotations.
    /// </summary>
    public enum BrickAssignmentAuthority
    {
        /// <summary>The assignment was derived and has the lowest authority.</summary>
        Derived = 0,
        /// <summary>The assignment came through an alias mapping.</summary>
        Alias = 1,
        /// <summary>The assignment came from external configuration or a policy document.</summary>
        External = 2,
        /// <summary>The assignment came directly from source metadata or explicit declaration.</summary>
        Direct = 3
    }
}
