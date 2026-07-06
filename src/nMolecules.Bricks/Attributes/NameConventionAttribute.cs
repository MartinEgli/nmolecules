using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Declares a naming convention for types that implement or inherit the annotated type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class NameConventionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NameConventionAttribute"/> class.
        /// </summary>
        /// <param name="pattern">The required name fragment.</param>
        /// <param name="position">The position where <paramref name="pattern"/> must appear.</param>
        public NameConventionAttribute(string pattern, NamePosition position)
        {
            Pattern = pattern ?? string.Empty;
            Position = position;
        }

        /// <summary>
        /// Gets the required name fragment.
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// Gets the position where <see cref="Pattern"/> must appear.
        /// </summary>
        public NamePosition Position { get; }

        /// <summary>
        /// Gets or sets whether this convention is required by itself or is one accepted alternative.
        /// </summary>
        public NameConventionRequirement Requirement { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the convention applies only to direct descendants.
        /// </summary>
        public bool DirectOnly { get; set; }

        /// <summary>
        /// Gets or sets the optional reason surfaced in analyzer diagnostics.
        /// </summary>
        public string Reason { get; set; }
    }
}
