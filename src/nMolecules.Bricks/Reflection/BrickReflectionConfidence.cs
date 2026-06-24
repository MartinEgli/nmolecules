

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes confidence in reflection-based observations. Use it when a
    /// dependency can be seen only through reflection or dynamic access patterns.
    /// </summary>
    public enum BrickReflectionConfidence
    {
        /// <summary>The reflection evidence is weak and should usually be reviewed.</summary>
        Low = 0,
        /// <summary>The reflection evidence is plausible but not as strong as compiler evidence.</summary>
        Medium = 1,
        /// <summary>The reflection evidence is strong enough for regular reporting.</summary>
        High = 2
    }
}
