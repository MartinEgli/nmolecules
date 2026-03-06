using System;

namespace NMolecules.Architecture.Hexagonal
{
    /// <summary>
    /// Identifies a primary port in a hexagonal architecture.
    ///
    /// <see href="https://alistair.cockburn.us/hexagonal-architecture/">Hexagonal Architecture</see>
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Struct)]
    public class PrimaryPortAttribute : Attribute
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
