using System;

namespace NMolecules.Persistence.EntityFramework
{
    /// <summary>
    /// Declares an EF value-converter type for a mapped member.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field)]
    public class EfValueConverterAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EfValueConverterAttribute"/> class.
        /// </summary>
        /// <param name="converterType">The converter type.</param>
        public EfValueConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
        }

        /// <summary>
        /// EF converter type.
        /// </summary>
        public Type ConverterType { get; }
    }
}
