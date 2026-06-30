using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Strongly typed identifier for dependency kind values used as stable keys across Bricks APIs.
    /// </summary>
    /// <remarks>
    /// Use this value object when rules, dependency facts, reports or serializers need to refer to
    /// a dependency kind such as <see cref="BrickDependencyKinds.TypeReference"/> without passing
    /// unstructured strings through the public API.
    /// </remarks>
    public readonly struct BrickDependencyKindId : IEquatable<BrickDependencyKindId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickDependencyKindId"/> struct.
        /// </summary>
        /// <param name="value">The dependency kind identifier. Null values are normalized to an empty string.</param>
        public BrickDependencyKindId(string value) => Value = value ?? string.Empty;

        /// <summary>
        /// Gets the normalized dependency kind identifier value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets a value indicating whether <see cref="Value"/> is empty or whitespace.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        /// <summary>
        /// Creates a dependency kind identifier from a string value.
        /// </summary>
        /// <param name="value">The dependency kind identifier. Null values are normalized to an empty string.</param>
        /// <returns>A normalized dependency kind identifier.</returns>
        public static BrickDependencyKindId From(string value) => new BrickDependencyKindId(value);

        /// <summary>
        /// Returns the dependency kind identifier value.
        /// </summary>
        /// <returns>The normalized identifier value.</returns>
        public override string ToString() => Value;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is BrickDependencyKindId other && Equals(other);

        /// <summary>
        /// Determines whether this identifier equals another identifier by ordinal value comparison.
        /// </summary>
        /// <param name="other">The other identifier.</param>
        /// <returns><c>true</c> when both values are equal; otherwise <c>false</c>.</returns>
        public bool Equals(BrickDependencyKindId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        /// <inheritdoc />
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);

        /// <summary>
        /// Converts a string value to a dependency kind identifier.
        /// </summary>
        /// <param name="value">The dependency kind identifier value.</param>
        public static implicit operator BrickDependencyKindId(string value) => new BrickDependencyKindId(value);

        /// <summary>
        /// Converts a dependency kind identifier to its string value.
        /// </summary>
        /// <param name="id">The dependency kind identifier.</param>
        public static implicit operator string(BrickDependencyKindId id) => id.Value;

        /// <summary>
        /// Compares two dependency kind identifiers for equality.
        /// </summary>
        public static bool operator ==(BrickDependencyKindId left, BrickDependencyKindId right) => left.Equals(right);

        /// <summary>
        /// Compares two dependency kind identifiers for inequality.
        /// </summary>
        public static bool operator !=(BrickDependencyKindId left, BrickDependencyKindId right) => !left.Equals(right);
    }
}
