using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Strongly typed identifier for role dimension id values.
    /// </summary>
    /// <remarks>
    /// Use dimension ids to group roles into comparable families, for example architecture layer,
    /// DDD building block or deployment role.
    /// </remarks>
    public readonly struct BrickDimensionId : IEquatable<BrickDimensionId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickDimensionId"/> struct.
        /// </summary>
        /// <param name="value">The dimension identifier. Null values are normalized to an empty string.</param>
        public BrickDimensionId(string value) => Value = value ?? string.Empty;

        /// <summary>
        /// Gets the normalized dimension identifier value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets a value indicating whether <see cref="Value"/> is empty or whitespace.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        /// <summary>
        /// Creates a dimension identifier from a string value.
        /// </summary>
        /// <param name="value">The dimension identifier. Null values are normalized to an empty string.</param>
        /// <returns>A normalized dimension identifier.</returns>
        public static BrickDimensionId From(string value) => new BrickDimensionId(value);

        /// <summary>
        /// Returns the dimension identifier value.
        /// </summary>
        /// <returns>The normalized identifier value.</returns>
        public override string ToString() => Value;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is BrickDimensionId other && Equals(other);

        /// <summary>
        /// Determines whether this identifier equals another identifier by ordinal value comparison.
        /// </summary>
        /// <param name="other">The other identifier.</param>
        /// <returns><c>true</c> when both values are equal; otherwise <c>false</c>.</returns>
        public bool Equals(BrickDimensionId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        /// <inheritdoc />
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);

        /// <summary>
        /// Converts a string value to a dimension identifier.
        /// </summary>
        /// <param name="value">The dimension identifier value.</param>
        public static implicit operator BrickDimensionId(string value) => new BrickDimensionId(value);

        /// <summary>
        /// Converts a dimension identifier to its string value.
        /// </summary>
        /// <param name="id">The dimension identifier.</param>
        public static implicit operator string(BrickDimensionId id) => id.Value;

        /// <summary>
        /// Compares two dimension identifiers for equality.
        /// </summary>
        public static bool operator ==(BrickDimensionId left, BrickDimensionId right) => left.Equals(right);

        /// <summary>
        /// Compares two dimension identifiers for inequality.
        /// </summary>
        public static bool operator !=(BrickDimensionId left, BrickDimensionId right) => !left.Equals(right);
    }
}
