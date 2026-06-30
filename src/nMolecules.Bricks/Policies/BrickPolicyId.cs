using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Strongly typed identifier for policy id values used as stable keys across Bricks APIs.
    /// </summary>
    /// <remarks>
    /// Use policy identifiers to connect policy documents, imports, rules and validation issues
    /// without passing raw strings through application code.
    /// </remarks>
    public readonly struct BrickPolicyId : IEquatable<BrickPolicyId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickPolicyId"/> struct.
        /// </summary>
        /// <param name="value">The policy identifier. Null values are normalized to an empty string.</param>
        public BrickPolicyId(string value) => Value = value ?? string.Empty;

        /// <summary>
        /// Gets the normalized policy identifier value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets a value indicating whether <see cref="Value"/> is empty or whitespace.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        /// <summary>
        /// Creates a policy identifier from a string value.
        /// </summary>
        /// <param name="value">The policy identifier. Null values are normalized to an empty string.</param>
        /// <returns>A normalized policy identifier.</returns>
        public static BrickPolicyId From(string value) => new BrickPolicyId(value);

        /// <summary>
        /// Returns the policy identifier value.
        /// </summary>
        /// <returns>The normalized identifier value.</returns>
        public override string ToString() => Value;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is BrickPolicyId other && Equals(other);

        /// <summary>
        /// Determines whether this identifier equals another identifier by ordinal value comparison.
        /// </summary>
        /// <param name="other">The other identifier.</param>
        /// <returns><c>true</c> when both values are equal; otherwise <c>false</c>.</returns>
        public bool Equals(BrickPolicyId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        /// <inheritdoc />
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);

        /// <summary>
        /// Converts a string value to a policy identifier.
        /// </summary>
        /// <param name="value">The policy identifier value.</param>
        public static implicit operator BrickPolicyId(string value) => new BrickPolicyId(value);

        /// <summary>
        /// Converts a policy identifier to its string value.
        /// </summary>
        /// <param name="id">The policy identifier.</param>
        public static implicit operator string(BrickPolicyId id) => id.Value;

        /// <summary>
        /// Compares two policy identifiers for equality.
        /// </summary>
        public static bool operator ==(BrickPolicyId left, BrickPolicyId right) => left.Equals(right);

        /// <summary>
        /// Compares two policy identifiers for inequality.
        /// </summary>
        public static bool operator !=(BrickPolicyId left, BrickPolicyId right) => !left.Equals(right);
    }
}
