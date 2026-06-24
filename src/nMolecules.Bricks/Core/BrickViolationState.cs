

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes the lifecycle state of a violation. Use it to distinguish
    /// active violations from reviewed exceptions and expired exceptions.
    /// </summary>
    public enum BrickViolationState
    {
        /// <summary>The violation is active and still needs action or acceptance.</summary>
        Active = 0,
        /// <summary>The violation is intentionally suppressed by a reviewed suppression.</summary>
        Suppressed = 1,
        /// <summary>The violation is accepted as part of a baseline.</summary>
        Baselined = 2,
        /// <summary>The suppression exists but is past its expiry.</summary>
        ExpiredSuppression = 3,
        /// <summary>The baseline entry exists but is past its expiry.</summary>
        ExpiredBaseline = 4
    }
}
