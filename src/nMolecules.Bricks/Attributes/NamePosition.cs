namespace NMolecules.Bricks
{
    /// <summary>
    /// Describes where a configured name pattern must appear in an element name.
    /// </summary>
    public enum NamePosition
    {
        /// <summary>Matches any position when used as an alias restriction.</summary>
        Any = 0,

        /// <summary>The element name must start with the pattern.</summary>
        Prefix = 1,

        /// <summary>The element name must end with the pattern.</summary>
        Suffix = 2,

        /// <summary>The element name must contain the pattern anywhere.</summary>
        Contains = 3,

        /// <summary>The element name must equal the pattern exactly.</summary>
        Exact = 4
    }
}
