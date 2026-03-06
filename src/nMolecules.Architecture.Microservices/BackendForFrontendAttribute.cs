using System;

namespace NMolecules.Architecture.Microservices
{
    /// <summary>
    /// Identifies a Backend-for-Frontend (BFF) component.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Struct)]
    public class BackendForFrontendAttribute : Attribute
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
