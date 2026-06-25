using System;

namespace NMolecules.Bricks
{
/// <summary>
/// Strongly typed identifier for policy id values used as stable keys across Bricks APIs.
/// </summary>
public readonly struct BrickPolicyId : IEquatable<BrickPolicyId>
    {
        public BrickPolicyId(string value) => Value = value ?? string.Empty;
        public string Value { get; }
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
        public static BrickPolicyId From(string value) => new BrickPolicyId(value);
        public override string ToString() => Value;
        public override bool Equals(object obj) => obj is BrickPolicyId other && Equals(other);
        public bool Equals(BrickPolicyId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);
        public static implicit operator BrickPolicyId(string value) => new BrickPolicyId(value);
        public static implicit operator string(BrickPolicyId id) => id.Value;
        public static bool operator ==(BrickPolicyId left, BrickPolicyId right) => left.Equals(right);
        public static bool operator !=(BrickPolicyId left, BrickPolicyId right) => !left.Equals(right);
    }
}
