

namespace NMolecules.Bricks
{
/// <summary>
    /// Defines how strongly a policy participates in tooling. Use it to move
    /// from documentation through analysis to build-breaking enforcement.
    /// </summary>
    public enum BrickEnforcementMode
    {
        /// <summary>The policy is present but not active.</summary>
        Disabled = 0,
        /// <summary>The policy is documented but does not produce active analyzer findings.</summary>
        Document = 1,
        /// <summary>The policy produces analysis results without necessarily breaking builds.</summary>
        Analyze = 2,
        /// <summary>The policy is enforced and errors may fail builds or quality gates.</summary>
        Enforce = 3
    }
}
