using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public readonly struct BrickPolicyImport : IEquatable<BrickPolicyImport>
    {
        public BrickPolicyImport(BrickPolicyId importedPolicyId, BrickPolicyImportMode mode)
        {
            ImportedPolicyId = importedPolicyId;
            Mode = mode;
        }

        public BrickPolicyId ImportedPolicyId { get; }
        public BrickPolicyImportMode Mode { get; }
        public bool Equals(BrickPolicyImport other) => ImportedPolicyId == other.ImportedPolicyId && Mode == other.Mode;
        public override bool Equals(object obj) => obj is BrickPolicyImport other && Equals(other);
        public override int GetHashCode() => ImportedPolicyId.GetHashCode() ^ Mode.GetHashCode();
        public static bool operator ==(BrickPolicyImport left, BrickPolicyImport right) => left.Equals(right);
        public static bool operator !=(BrickPolicyImport left, BrickPolicyImport right) => !left.Equals(right);
    }
}
