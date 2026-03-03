using System;

namespace NMolecules.DDD
{
    /// <summary>
    /// Identifies an application service. Application services coordinate use cases and orchestrate domain objects
    /// without becoming part of the domain model itself.
    /// </summary>
    [AttributeUsage(
            AttributeTargets.Class |
            AttributeTargets.Interface)]
    public class ApplicationServiceAttribute : Attribute
    {
    }
}
