using System;

namespace NMolecules.Architecture.Cqrs
{
    /// <summary>
    /// Identifies a projection in the context of CQRS, i.e. logic that updates read models from events or other inputs.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Struct)]
    public class ProjectionAttribute : Attribute
    {
    }
}
