using System;

namespace NMolecules.Architecture.Cqrs
{
    /// <summary>
    /// Identifies a query in the context of CQRS, i.e. a read-side request for information.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Struct)]
    public class QueryAttribute : Attribute
    {
        public string Namespace { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
