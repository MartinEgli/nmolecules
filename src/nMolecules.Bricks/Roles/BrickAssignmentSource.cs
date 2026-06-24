

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes where a role assignment was sourced from. Use it for provenance
    /// in reports, resolution traces, and conflict explanations.
    /// </summary>
    public enum BrickAssignmentSource
    {
        /// <summary>The role came from a source-code attribute.</summary>
        SourceAttribute = 0,
        /// <summary>The role came from a policy file.</summary>
        PolicyFile = 1,
        /// <summary>The role came from a convention.</summary>
        Convention = 2,
        /// <summary>The role came from inference.</summary>
        Inference = 3,
        /// <summary>The role came from alias mapping.</summary>
        AliasMapping = 4,
        /// <summary>The role came from a package.</summary>
        Package = 5,
        /// <summary>The role came from a generator.</summary>
        Generator = 6
    }
}
