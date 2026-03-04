using System;

namespace NMolecules.Architecture.Cqrs
{
    /// <summary>
    /// Identifies a query handler in the context of CQRS, i.e. logic that resolves read-side requests.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method |
        AttributeTargets.Constructor)]
    public class QueryHandlerAttribute : Attribute
    {
        public string Namespace { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
