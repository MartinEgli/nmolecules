

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes how strongly a dependency is connected to the source element.
    /// Use it to communicate whether evidence is direct, indirect, or inferred.
    /// </summary>
    public enum BrickDependencyStrength
    {
        /// <summary>The source directly references or uses the target.</summary>
        Direct = 0,
        /// <summary>The dependency exists through an intermediate element or indirection.</summary>
        Indirect = 1,
        /// <summary>The dependency was inferred and may need supporting evidence.</summary>
        Inferred = 2
    }
}
