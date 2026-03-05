using System;

namespace NMolecules.Persistence.EntityFramework
{
    /// <summary>
    /// Marks a value object as EF-owned/complex mapping candidate.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Struct)]
    public class EfOwnedValueObjectAttribute : Attribute
    {
        /// <summary>
        /// Optional owner navigation/property name used for documentation and tooling.
        /// </summary>
        public string Owner { get; set; } = string.Empty;
    }
}
