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
