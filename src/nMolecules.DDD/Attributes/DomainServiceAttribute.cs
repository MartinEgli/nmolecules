using System;

namespace NMolecules.DDD
{
    /// <summary>
    /// Identifies a domain service. Domain services model domain operations that do not naturally belong to an
    /// entity, aggregate or value object but are still part of the domain itself.
    /// </summary>
    [AttributeUsage(
            AttributeTargets.Class |
            AttributeTargets.Interface)]
    public class DomainServiceAttribute : Attribute
    {
    }
}
