

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes the severity of a finding. Use it to map Bricks findings to
    /// diagnostics, reports, CI gates, and review workflows.
    /// </summary>
    public enum BrickSeverity
    {
        /// <summary>Informational finding that does not require immediate action.</summary>
        Info = 0,
        /// <summary>Warning that should be reviewed or fixed according to team policy.</summary>
        Warning = 1,
        /// <summary>Error that represents a broken rule or build-blocking issue.</summary>
        Error = 2
    }
}
