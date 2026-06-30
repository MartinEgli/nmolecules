using System;

namespace NMolecules.Bricks
{
/// <summary>
    /// Built-in dependency kind identifiers used by Bricks policies, reports
    /// and attribute-based dependency evidence.
    /// </summary>
    public static class BrickDependencyKinds
    {
        /// <summary>
        /// Compiler-confirmed static dependency caused by a type reference.
        /// </summary>
        public const string TypeReference = "TypeReference";
    }
}
