

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes how reliable the evidence behind a dependency or violation is.
    /// Use it to rank findings and decide whether human review is required.
    /// </summary>
    public enum BrickEvidenceLevel
    {
        /// <summary>The evidence level is unknown.</summary>
        Unknown = 0,
        /// <summary>The compiler or semantic model confirms the evidence.</summary>
        CompilerConfirmed = 1,
        /// <summary>An analyzer inferred the evidence from code structure.</summary>
        AnalyzerInferred = 2,
        /// <summary>Configuration explicitly declared the evidence.</summary>
        ConfigurationDeclared = 3,
        /// <summary>Runtime observation or runtime metadata inferred the evidence.</summary>
        RuntimeInferred = 4
    }
}
