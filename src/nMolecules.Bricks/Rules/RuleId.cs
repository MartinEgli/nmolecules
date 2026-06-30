using System;

namespace NMolecules.Bricks
{
/// <summary>
    /// Represents a typed architectural rule identifier for bricks-based catalogs,
    /// diagnostics, and helper APIs outside direct attribute argument lists.
    /// </summary>
    /// <remarks>
    /// CLR attribute arguments cannot use arbitrary custom structs or classes.
    /// Because of that, brick attributes continue to use string-based constructor
    /// parameters for attribute syntax, while <see cref="RuleId"/> provides a
    /// stronger type for regular runtime code.
    /// </remarks>
    /// <summary>
    /// Strongly typed identifier for rule id values used as stable keys across Bricks APIs.
    /// </summary>
    public readonly struct RuleId : IEquatable<RuleId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleId"/> struct.
        /// </summary>
        /// <param name="value">The raw rule identifier value.</param>
        public RuleId(string value)
        {
            Value = value ?? string.Empty;
        }

        /// <summary>
        /// Gets the raw rule identifier value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets a value indicating whether the rule identifier is empty.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        /// <summary>
        /// Creates a typed rule identifier from a raw string value.
        /// </summary>
        /// <param name="value">The raw rule identifier value.</param>
        /// <returns>A typed rule identifier wrapper.</returns>
        public static RuleId From(string value)
        {
            return new RuleId(value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is RuleId other && Equals(other);
        }

        /// <inheritdoc />
        public bool Equals(RuleId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        }

        /// <summary>
        /// Converts a raw string value into a typed rule identifier.
        /// </summary>
        /// <param name="value">The raw rule identifier value.</param>
        public static implicit operator RuleId(string value)
        {
            return new RuleId(value);
        }

        /// <summary>
        /// Converts a typed rule identifier into its raw string value.
        /// </summary>
        /// <param name="ruleId">The rule identifier to unwrap.</param>
        public static implicit operator string(RuleId ruleId)
        {
            return ruleId.Value;
        }

        /// <summary>
        /// Compares two rule identifiers for ordinal equality.
        /// </summary>
        public static bool operator ==(RuleId left, RuleId right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two rule identifiers for ordinal inequality.
        /// </summary>
        public static bool operator !=(RuleId left, RuleId right)
        {
            return !left.Equals(right);
        }
    }
}
