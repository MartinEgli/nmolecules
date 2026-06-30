using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Value object that describes how one policy imports another policy.
    /// </summary>
    /// <remarks>
    /// Policy imports are interpreted by <see cref="BrickPolicyComposer"/> and control whether
    /// imported rules are extended, narrowed, overridden or disabled.
    /// </remarks>
    public readonly struct BrickPolicyImport : IEquatable<BrickPolicyImport>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickPolicyImport"/> struct.
        /// </summary>
        /// <param name="importedPolicyId">The policy id to import.</param>
        /// <param name="mode">The import mode.</param>
        public BrickPolicyImport(BrickPolicyId importedPolicyId, BrickPolicyImportMode mode)
        {
            ImportedPolicyId = importedPolicyId;
            Mode = mode;
        }

        /// <summary>
        /// Gets the policy id to import.
        /// </summary>
        public BrickPolicyId ImportedPolicyId { get; }

        /// <summary>
        /// Gets the import mode.
        /// </summary>
        public BrickPolicyImportMode Mode { get; }

        /// <summary>
        /// Determines whether this import equals another import by imported policy id and mode.
        /// </summary>
        /// <param name="other">The other import.</param>
        /// <returns><c>true</c> when both imports are equal; otherwise <c>false</c>.</returns>
        public bool Equals(BrickPolicyImport other) => ImportedPolicyId == other.ImportedPolicyId && Mode == other.Mode;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is BrickPolicyImport other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => ImportedPolicyId.GetHashCode() ^ Mode.GetHashCode();

        /// <summary>
        /// Compares two policy imports for equality.
        /// </summary>
        public static bool operator ==(BrickPolicyImport left, BrickPolicyImport right) => left.Equals(right);

        /// <summary>
        /// Compares two policy imports for inequality.
        /// </summary>
        public static bool operator !=(BrickPolicyImport left, BrickPolicyImport right) => !left.Equals(right);
    }
}
