namespace NMolecules.Bricks
{
    /// <summary>
    /// Describes whether a naming convention is required by itself or participates in an alternative set.
    /// </summary>
    public enum NameConventionRequirement
    {
        /// <summary>
        /// The convention must be satisfied by every implementing or inheriting type.
        /// </summary>
        Required = 0,

        /// <summary>
        /// The convention is one accepted alternative; at least one active alternative must match.
        /// </summary>
        Alternative = 1
    }
}
