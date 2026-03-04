using System;
namespace NMolecules.DDD
{
    /// <summary>
    /// Identifies a bounded context. A description of a boundary (typically a subsystem, or the work of a particular team)
    /// within which a particular model is defined and applicable.A bounded context has an architectural style and contains
    /// domain logic and technical logic.
    ///
    /// <see href="https://domainlanguage.com/wp-content/uploads/2016/05/DDD_Reference_2015-03.pdf">Domain-Driven Design
    ///      Reference (Evans) - Bounded Contexts</see>
    /// </summary>
    [AttributeUsage(
            AttributeTargets.Assembly |
            AttributeTargets.Module)]
    public class BoundedContextAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedContextAttribute"/> class.
        /// </summary>
        public BoundedContextAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedContextAttribute"/> class with a context name.
        /// </summary>
        /// <param name="name">The bounded-context name.</param>
        public BoundedContextAttribute(string name)
        {
            Name = name ?? string.Empty;
            Value = name ?? string.Empty;
        }

        /// <summary>
        /// A stable identifier for the bounded context that tooling can use to derive externalized metadata.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// A human-readable name for the bounded context.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// An alias for <see cref="Name"/> for concise usage.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// A human-readable description for the bounded context.
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
