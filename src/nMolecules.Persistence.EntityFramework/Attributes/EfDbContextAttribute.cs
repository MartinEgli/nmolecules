using System;

namespace NMolecules.Persistence.EntityFramework
{
    /// <summary>
    /// Identifies a DbContext boundary and links it to domain metadata.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EfDbContextAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EfDbContextAttribute"/> class.
        /// </summary>
        public EfDbContextAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EfDbContextAttribute"/> class with a context name.
        /// </summary>
        /// <param name="name">The context name used in diagnostics and mapping catalogs.</param>
        public EfDbContextAttribute(string name)
        {
            Name = name ?? string.Empty;
        }

        /// <summary>
        /// Human-readable DbContext name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional bounded-context identifier this DbContext belongs to.
        /// </summary>
        public string BoundedContextId { get; set; } = string.Empty;

        /// <summary>
        /// Optional module identifier this DbContext belongs to.
        /// </summary>
        public string ModuleId { get; set; } = string.Empty;
    }
}
