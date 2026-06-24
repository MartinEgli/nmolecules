

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes whether an assignment contributes a role or intentionally hides
    /// one. Use it for role overrides, suppressions, and migration scenarios.
    /// </summary>
    public enum BrickAssignmentBehavior
    {
        /// <summary>Apply the assignment.</summary>
        Apply = 0,
        /// <summary>Suppress or remove the matching assignment.</summary>
        Suppress = 1
    }
}
