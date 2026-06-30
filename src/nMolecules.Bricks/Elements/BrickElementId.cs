using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Strongly typed identifier for element id values used as stable keys across Bricks APIs.
    /// </summary>
    /// <remarks>
    /// Use element identifiers to refer to architectural elements in assignments, dependencies,
    /// violations, reports and serialized evidence without relying on display names.
    /// </remarks>
    public readonly struct BrickElementId : IEquatable<BrickElementId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickElementId"/> struct.
        /// </summary>
        /// <param name="value">The element identifier. Null values are normalized to an empty string.</param>
        public BrickElementId(string value) => Value = value ?? string.Empty;

        /// <summary>
        /// Gets the normalized element identifier value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets a value indicating whether <see cref="Value"/> is empty or whitespace.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        /// <summary>
        /// Creates an element identifier from a string value.
        /// </summary>
        /// <param name="value">The element identifier. Null values are normalized to an empty string.</param>
        /// <returns>A normalized element identifier.</returns>
        public static BrickElementId From(string value) => new BrickElementId(value);

        /// <summary>
        /// Returns the element identifier value.
        /// </summary>
        /// <returns>The normalized identifier value.</returns>
        public override string ToString() => Value;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is BrickElementId other && Equals(other);

        /// <summary>
        /// Determines whether this identifier equals another identifier by ordinal value comparison.
        /// </summary>
        /// <param name="other">The other identifier.</param>
        /// <returns><c>true</c> when both values are equal; otherwise <c>false</c>.</returns>
        public bool Equals(BrickElementId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        /// <inheritdoc />
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);

        /// <summary>
        /// Converts a string value to an element identifier.
        /// </summary>
        /// <param name="value">The element identifier value.</param>
        public static implicit operator BrickElementId(string value) => new BrickElementId(value);

        /// <summary>
        /// Converts an element identifier to its string value.
        /// </summary>
        /// <param name="id">The element identifier.</param>
        public static implicit operator string(BrickElementId id) => id.Value;

        /// <summary>
        /// Compares two element identifiers for equality.
        /// </summary>
        public static bool operator ==(BrickElementId left, BrickElementId right) => left.Equals(right);

        /// <summary>
        /// Compares two element identifiers for inequality.
        /// </summary>
        public static bool operator !=(BrickElementId left, BrickElementId right) => !left.Equals(right);
    }
}
