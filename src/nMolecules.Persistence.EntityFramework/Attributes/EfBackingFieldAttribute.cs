using System;

namespace NMolecules.Persistence.EntityFramework
{
    /// <summary>
    /// Declares an explicit backing field name for an EF-mapped property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EfBackingFieldAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EfBackingFieldAttribute"/> class.
        /// </summary>
        /// <param name="fieldName">The backing field name.</param>
        public EfBackingFieldAttribute(string fieldName)
        {
            FieldName = fieldName ?? string.Empty;
        }

        /// <summary>
        /// Backing field name.
        /// </summary>
        public string FieldName { get; }
    }
}
