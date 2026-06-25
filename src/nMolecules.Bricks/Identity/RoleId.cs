using System;

namespace NMolecules.Bricks
{
/// <summary>
    /// Represents a typed architectural role identifier for bricks-based catalogs,
    /// comparisons, and helper APIs outside direct attribute argument lists.
    /// </summary>
    /// <remarks>
    /// CLR attribute arguments cannot use arbitrary custom structs or classes.
    /// Because of that, brick attributes continue to use string-based constructor
    /// parameters for attribute syntax, while <see cref="RoleId"/> provides a
    /// stronger type for regular runtime code.
    /// </remarks>
    /// <summary>
    /// Strongly typed identifier for role id values used as stable keys across Bricks APIs.
    /// </summary>
    public readonly struct RoleId : IEquatable<RoleId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoleId"/> struct.
        /// </summary>
        /// <param name="value">The raw role identifier value.</param>
        public RoleId(string value)
        {
            Value = value ?? string.Empty;
        }

        /// <summary>
        /// Gets the raw role identifier value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets a value indicating whether the role identifier is empty.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        /// <summary>
        /// Creates a typed role identifier from a raw string value.
        /// </summary>
        /// <param name="value">The raw role identifier value.</param>
        /// <returns>A typed role identifier wrapper.</returns>
        public static RoleId From(string value)
        {
            return new RoleId(value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is RoleId other && Equals(other);
        }

        /// <inheritdoc />
        public bool Equals(RoleId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        }

        /// <summary>
        /// Converts a raw string value into a typed role identifier.
        /// </summary>
        /// <param name="value">The raw role identifier value.</param>
        public static implicit operator RoleId(string value)
        {
            return new RoleId(value);
        }

        /// <summary>
        /// Converts a typed role identifier into its raw string value.
        /// </summary>
        /// <param name="roleId">The role identifier to unwrap.</param>
        public static implicit operator string(RoleId roleId)
        {
            return roleId.Value;
        }

        /// <summary>
        /// Compares two role identifiers for ordinal equality.
        /// </summary>
        public static bool operator ==(RoleId left, RoleId right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two role identifiers for ordinal inequality.
        /// </summary>
        public static bool operator !=(RoleId left, RoleId right)
        {
            return !left.Equals(right);
        }
    }
}
