using System;

namespace NMolecules.Persistence.EntityFramework
{
    /// <summary>
    /// Marks a member as EF optimistic-concurrency token.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field)]
    public class EfConcurrencyTokenAttribute : Attribute
    {
        /// <summary>
        /// Optional token strategy hint (for example: rowversion).
        /// </summary>
        public string Strategy { get; set; } = string.Empty;
    }
}
