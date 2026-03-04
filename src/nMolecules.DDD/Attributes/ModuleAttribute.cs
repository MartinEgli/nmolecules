using System;

namespace NMolecules.DDD
{
    /// <summary>
    /// Identifies a DDD module.
    ///
    /// <see href="https://domainlanguage.com/wp-content/uploads/2016/05/DDD_Reference_2015-03.pdf">Domain-Driven Design
    ///      Reference (Evans) - Modules</see>
    /// </summary>
    [AttributeUsage(
            AttributeTargets.Assembly |
            AttributeTargets.Module)]
    public class ModuleAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleAttribute"/> class.
        /// </summary>
        public ModuleAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleAttribute"/> class with a module name.
        /// </summary>
        /// <param name="name">The module name.</param>
        public ModuleAttribute(string name)
        {
            Name = name ?? string.Empty;
            Value = name ?? string.Empty;
        }

        /// <summary>
        /// A stable identifier for the module that tooling can use in reports and catalogs.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// A human-readable name for the module.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Alias for <see cref="Name"/> for concise usage.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Optional bounded-context identifier this module belongs to.
        /// </summary>
        public string BoundedContextId { get; set; } = string.Empty;

        /// <summary>
        /// A human-readable module description.
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
