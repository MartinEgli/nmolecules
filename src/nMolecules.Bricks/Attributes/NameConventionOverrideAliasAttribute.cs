using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Declares a name-convention override for a target type that cannot be annotated directly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class NameConventionOverrideAliasAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NameConventionOverrideAliasAttribute"/> class.
        /// </summary>
        /// <param name="targetType">The type to which the override applies.</param>
        /// <param name="suppressedSource">The convention source to suppress or prefer.</param>
        public NameConventionOverrideAliasAttribute(Type targetType, Type suppressedSource)
            : this(targetType, suppressedSource, NameConventionOverrideBehavior.Suppress)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NameConventionOverrideAliasAttribute"/> class.
        /// </summary>
        /// <param name="targetType">The type to which the override applies.</param>
        /// <param name="suppressedSource">The convention source to suppress or prefer.</param>
        /// <param name="behavior">The override behavior.</param>
        public NameConventionOverrideAliasAttribute(
            Type targetType,
            Type suppressedSource,
            NameConventionOverrideBehavior behavior)
        {
            TargetType = targetType ?? typeof(Attribute);
            SuppressedSource = suppressedSource ?? typeof(Attribute);
            Behavior = behavior;
        }

        /// <summary>
        /// Gets the type to which the override applies.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Gets the convention source to suppress or prefer.
        /// </summary>
        public Type SuppressedSource { get; }

        /// <summary>
        /// Gets the override behavior.
        /// </summary>
        public NameConventionOverrideBehavior Behavior { get; }

        /// <summary>
        /// Gets or sets the required human-readable reason for the override.
        /// </summary>
        public string Reason { get; set; }
    }
}
