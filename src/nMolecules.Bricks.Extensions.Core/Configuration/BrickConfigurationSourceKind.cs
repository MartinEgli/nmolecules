

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes the configuration channel that supplied Bricks settings. Use it
    /// to explain configuration precedence across generated defaults, packages,
    /// policy files, MSBuild, analyzer config, and source annotations.
    /// </summary>
    public enum BrickConfigurationSourceKind
    {
        /// <summary>Generated defaults supplied the configuration.</summary>
        Generated = 0,
        /// <summary>A package supplied the configuration.</summary>
        Package = 1,
        /// <summary>A policy file supplied the configuration.</summary>
        PolicyFile = 2,
        /// <summary>MSBuild properties or items supplied the configuration.</summary>
        MSBuild = 3,
        /// <summary>.editorconfig or analyzer-config options supplied the configuration.</summary>
        AnalyzerConfig = 4,
        /// <summary>Source-code annotations supplied the configuration.</summary>
        SourceAnnotation = 5
    }
}
