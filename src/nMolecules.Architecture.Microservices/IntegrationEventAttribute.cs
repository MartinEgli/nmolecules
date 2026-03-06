using System;

namespace NMolecules.Architecture.Microservices
{
    /// <summary>
    /// Identifies an integration event exchanged between microservices.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Struct)]
    public class IntegrationEventAttribute : Attribute
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
