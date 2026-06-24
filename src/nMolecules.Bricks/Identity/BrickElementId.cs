using System;

namespace NMolecules.Bricks
{
public readonly struct BrickElementId : IEquatable<BrickElementId>
    {
        public BrickElementId(string value) => Value = value ?? string.Empty;
        public string Value { get; }
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
        public static BrickElementId From(string value) => new BrickElementId(value);
        public override string ToString() => Value;
        public override bool Equals(object obj) => obj is BrickElementId other && Equals(other);
        public bool Equals(BrickElementId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);
        public static implicit operator BrickElementId(string value) => new BrickElementId(value);
        public static implicit operator string(BrickElementId id) => id.Value;
        public static bool operator ==(BrickElementId left, BrickElementId right) => left.Equals(right);
        public static bool operator !=(BrickElementId left, BrickElementId right) => !left.Equals(right);
    }
}
