

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes how specific a role assignment is. Use it as one part of
    /// precedence when multiple role assignments compete.
    /// </summary>
    public enum BrickAssignmentSpecificity
    {
        /// <summary>The assignment came from broad inference.</summary>
        Inference = 0,
        /// <summary>The assignment came from a convention.</summary>
        Convention = 1,
        /// <summary>The assignment applies at assembly level.</summary>
        Assembly = 2,
        /// <summary>The assignment applies at namespace level.</summary>
        Namespace = 3,
        /// <summary>The assignment applies directly to the element.</summary>
        Element = 4
    }
}
