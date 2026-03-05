using System;

namespace NMolecules.Persistence.EntityFramework
{
    /// <summary>
    /// Declares relational mapping metadata for an EF entity type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EfEntityTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EfEntityTypeAttribute"/> class.
        /// </summary>
        public EfEntityTypeAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EfEntityTypeAttribute"/> class with a table name.
        /// </summary>
        /// <param name="table">The primary table name.</param>
        public EfEntityTypeAttribute(string table)
        {
            Table = table ?? string.Empty;
        }

        /// <summary>
        /// Table name for the entity type.
        /// </summary>
        public string Table { get; set; } = string.Empty;

        /// <summary>
        /// Optional schema for the entity table.
        /// </summary>
        public string Schema { get; set; } = string.Empty;

        /// <summary>
        /// Marks the entity as keyless in EF mapping.
        /// </summary>
        public bool Keyless { get; set; }
    }
}
