using System;

namespace NMolecules.Architecture.Cqrs
{
    /// <summary>
    /// Identifies a query model in the context of CQRS, i.e. a read-optimized representation of current state.
    ///
    /// <see href="http://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf">CQRS Documents by Greg Young</see>
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Struct)]
    public class QueryModelAttribute : Attribute
    {
    }
}
