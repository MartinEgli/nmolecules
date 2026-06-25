using System;

namespace NMolecules.Bricks
{
/// <summary>
/// Strongly typed identifier for dimension id values used as stable keys across Bricks APIs.
/// </summary>
public readonly struct BrickDimensionId : IEquatable<BrickDimensionId>
    {
        public BrickDimensionId(string value) => Value = value ?? string.Empty;
        public string Value { get; }
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
        public static BrickDimensionId From(string value) => new BrickDimensionId(value);
        public override string ToString() => Value;
        public override bool Equals(object obj) => obj is BrickDimensionId other && Equals(other);
        public bool Equals(BrickDimensionId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);
        public static implicit operator BrickDimensionId(string value) => new BrickDimensionId(value);
        public static implicit operator string(BrickDimensionId id) => id.Value;
        public static bool operator ==(BrickDimensionId left, BrickDimensionId right) => left.Equals(right);
        public static bool operator !=(BrickDimensionId left, BrickDimensionId right) => !left.Equals(right);
    }
}
