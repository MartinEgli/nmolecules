using System;

namespace NMolecules.Bricks
{
/// <summary>
/// Strongly typed identifier for dependency kind id values used as stable keys across Bricks APIs.
/// </summary>
public readonly struct BrickDependencyKindId : IEquatable<BrickDependencyKindId>
    {
        public BrickDependencyKindId(string value) => Value = value ?? string.Empty;
        public string Value { get; }
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
        public static BrickDependencyKindId From(string value) => new BrickDependencyKindId(value);
        public override string ToString() => Value;
        public override bool Equals(object obj) => obj is BrickDependencyKindId other && Equals(other);
        public bool Equals(BrickDependencyKindId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);
        public static implicit operator BrickDependencyKindId(string value) => new BrickDependencyKindId(value);
        public static implicit operator string(BrickDependencyKindId id) => id.Value;
        public static bool operator ==(BrickDependencyKindId left, BrickDependencyKindId right) => left.Equals(right);
        public static bool operator !=(BrickDependencyKindId left, BrickDependencyKindId right) => !left.Equals(right);
    }
}
