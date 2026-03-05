using System;

namespace NMolecules.Persistence.EntityFramework
{
    /// <summary>
    /// Marks a member as intentionally excluded from EF persistence mapping.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field)]
    public class EfIgnoreAttribute : Attribute
    {
        /// <summary>
        /// Optional exclusion reason for diagnostics and documentation.
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }
}
