

namespace NMolecules.Bricks
{
/// <summary>
    /// Defines the architectural level at which a rule, dependency, or violation
    /// is evaluated. Use it to make clear whether a check applies to a type,
    /// member, namespace, assembly, or the whole model.
    /// </summary>
    public enum BrickScope
    {
        /// <summary>The check applies at type level.</summary>
        Type = 0,
        /// <summary>The check applies at member level.</summary>
        Member = 1,
        /// <summary>The check applies at namespace level.</summary>
        Namespace = 2,
        /// <summary>The check applies at assembly level.</summary>
        Assembly = 3,
        /// <summary>The check applies globally to the complete analyzed model.</summary>
        Global = 4
    }
}
