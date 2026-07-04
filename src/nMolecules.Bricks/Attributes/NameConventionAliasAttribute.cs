using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Imports naming conventions from another type that carries <see cref="NameConventionAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class NameConventionAliasAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NameConventionAliasAttribute"/> class.
        /// </summary>
        /// <param name="sourceType">The type whose naming conventions are imported.</param>
        public NameConventionAliasAttribute(Type sourceType)
        {
            SourceType = sourceType ?? typeof(Attribute);
        }

        /// <summary>
        /// Gets the type whose naming conventions are imported.
        /// </summary>
        public Type SourceType { get; }

        /// <summary>
        /// Gets or sets the imported position. <see cref="NamePosition.Any"/> imports all positions.
        /// </summary>
        public NamePosition RestrictToPosition { get; set; }

        /// <summary>
        /// Gets or sets the optional reason surfaced in analyzer diagnostics.
        /// </summary>
        public string Reason { get; set; }
    }
}
