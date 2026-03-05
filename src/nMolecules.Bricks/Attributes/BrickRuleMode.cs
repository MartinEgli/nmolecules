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
        RequireDependency = 1
    }

    /// <summary>
    /// Backward-compatible alias for <see cref="RuleMode"/>.
    /// </summary>
    public enum BrickRuleMode
    {
        /// <inheritdoc cref="RuleMode.ForbidDependency"/>
        ForbidDependency = RuleMode.ForbidDependency,

        /// <inheritdoc cref="RuleMode.RequireDependency"/>
        RequireDependency = RuleMode.RequireDependency
    }
}
