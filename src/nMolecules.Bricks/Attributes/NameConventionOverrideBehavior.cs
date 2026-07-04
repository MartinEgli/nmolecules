namespace NMolecules.Bricks
{
    /// <summary>
    /// Describes how a name-convention override resolves active naming constraints.
    /// </summary>
    public enum NameConventionOverrideBehavior
    {
        /// <summary>Suppresses the convention from the configured source.</summary>
        Suppress = 0,

        /// <summary>Prefers the configured source over conflicting conventions.</summary>
        Prefer = 1
    }
}
