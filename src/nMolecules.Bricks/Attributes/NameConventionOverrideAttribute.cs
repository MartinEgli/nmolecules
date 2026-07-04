using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Resolves or suppresses an active naming convention on the annotated type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
    public sealed class NameConventionOverrideAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NameConventionOverrideAttribute"/> class.
        /// </summary>
        /// <param name="suppressedSource">The convention source to suppress or prefer.</param>
        public NameConventionOverrideAttribute(Type suppressedSource)
            : this(suppressedSource, NameConventionOverrideBehavior.Suppress)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NameConventionOverrideAttribute"/> class.
        /// </summary>
        /// <param name="suppressedSource">The convention source to suppress or prefer.</param>
        /// <param name="behavior">The override behavior.</param>
        public NameConventionOverrideAttribute(Type suppressedSource, NameConventionOverrideBehavior behavior)
        {
            SuppressedSource = suppressedSource ?? typeof(Attribute);
            Behavior = behavior;
        }

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
