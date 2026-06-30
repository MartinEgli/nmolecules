using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Represents a typed rule message template for <see cref="RuleAttribute"/>.
    /// </summary>
    /// <remarks>
    /// CLR attribute arguments cannot use arbitrary custom structs or classes.
    /// Because of that, public attribute syntax still uses string-based constructor
    /// parameters, while <see cref="RuleMessage"/> provides a stronger shape for
    /// regular code and protected specialization APIs.
    /// </remarks>
    public readonly struct RuleMessage : IEquatable<RuleMessage>
    {
        /// <summary>
        /// The rule identifier placeholder.
        /// </summary>
        public const string RulePlaceholder = "{rule}";

        /// <summary>
        /// The source type placeholder.
        /// </summary>
        public const string SourcePlaceholder = "{source}";

        /// <summary>
        /// The target type or role placeholder.
        /// </summary>
        public const string TargetPlaceholder = "{target}";

        /// <summary>
        /// The member placeholder.
        /// </summary>
        public const string MemberPlaceholder = "{member}";

        /// <summary>
        /// Gets an empty rule message.
        /// </summary>
        public static RuleMessage Empty => new RuleMessage();

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleMessage"/> struct.
        /// </summary>
        /// <param name="value">The raw rule message template.</param>
        public RuleMessage(string value)
        {
            Value = value ?? string.Empty;
        }

        /// <summary>
        /// Gets the raw rule message template.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets a value indicating whether the message template is empty.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        /// <summary>
        /// Gets a value indicating whether the template contains <c>{rule}</c>.
        /// </summary>
        public bool UsesRulePlaceholder => Contains(RulePlaceholder);

        /// <summary>
        /// Gets a value indicating whether the template contains <c>{source}</c>.
        /// </summary>
        public bool UsesSourcePlaceholder => Contains(SourcePlaceholder);

        /// <summary>
        /// Gets a value indicating whether the template contains <c>{target}</c>.
        /// </summary>
        public bool UsesTargetPlaceholder => Contains(TargetPlaceholder);

        /// <summary>
        /// Gets a value indicating whether the template contains <c>{member}</c>.
        /// </summary>
        public bool UsesMemberPlaceholder => Contains(MemberPlaceholder);

        /// <summary>
        /// Creates a typed rule message from a raw template string.
        /// </summary>
        /// <param name="value">The raw rule message template.</param>
        /// <returns>A typed rule message.</returns>
        public static RuleMessage From(string value)
        {
            return new RuleMessage(value);
        }

        /// <summary>
        /// Creates a new builder for a rule message template.
        /// </summary>
        /// <returns>A fluent rule message builder.</returns>
        public static RuleMessageBuilder Builder()
        {
            return new RuleMessageBuilder();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Determines whether this message equals another message by ordinal value comparison.
        /// </summary>
        /// <param name="other">The other message.</param>
        /// <returns><c>true</c> when both templates are equal; otherwise <c>false</c>.</returns>
        public bool Equals(RuleMessage other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is RuleMessage other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        }

        /// <summary>
        /// Converts a raw string value into a typed rule message.
        /// </summary>
        /// <param name="value">The raw rule message template.</param>
        public static implicit operator RuleMessage(string value)
        {
            return new RuleMessage(value);
        }

        /// <summary>
        /// Converts a typed rule message into its raw string value.
        /// </summary>
        /// <param name="message">The typed rule message.</param>
        public static implicit operator string(RuleMessage message)
        {
            return message.Value;
        }

        /// <summary>
        /// Compares two rule messages for ordinal equality.
        /// </summary>
        public static bool operator ==(RuleMessage left, RuleMessage right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two rule messages for ordinal inequality.
        /// </summary>
        public static bool operator !=(RuleMessage left, RuleMessage right)
        {
            return !left.Equals(right);
        }

        private bool Contains(string placeholder)
        {
            return Value?.IndexOf(placeholder, StringComparison.Ordinal) >= 0;
        }
    }
}
