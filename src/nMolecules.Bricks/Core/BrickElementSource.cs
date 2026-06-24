

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes which input channel contributed an element or assignment.
    /// Use it when explaining why a role, element, or policy fact exists.
    /// </summary>
    public enum BrickElementSource
    {
        /// <summary>The source channel is unknown.</summary>
        Unknown = 0,
        /// <summary>The fact came directly from code.</summary>
        Code = 1,
        /// <summary>The fact came from configuration such as policy files or build settings.</summary>
        Configuration = 2,
        /// <summary>The fact came from a naming or structural convention.</summary>
        Convention = 3,
        /// <summary>The fact was inferred by an analyzer or heuristic.</summary>
        Inference = 4,
        /// <summary>The fact came from an imported package, profile, or role pack.</summary>
        Import = 5
    }
}
