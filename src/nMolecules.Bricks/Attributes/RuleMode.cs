

namespace NMolecules.Bricks
{
/// <summary>
    /// Defines how a <see cref="RuleAttribute"/> should be interpreted by analyzers.
    /// </summary>
    public enum RuleMode
    {
        /// <summary>
        /// Indicates that dependencies from source role to target role are forbidden.
        /// </summary>
        ForbidDependency = 0,

        /// <summary>
        /// Indicates that dependencies from source role to target role are required.
        /// </summary>
        RequireDependency = 1,

        /// <summary>
        /// Indicates that dependencies from source role to target role are allowed
        /// when a policy denies unmatched dependencies by default.
        /// </summary>
        AllowDependency = 2
    }
}
