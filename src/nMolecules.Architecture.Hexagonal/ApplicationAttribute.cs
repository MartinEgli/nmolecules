using System;

namespace NMolecules.Architecture.Hexagonal
{
    /// <summary>
    /// Identifies the application core in a hexagonal architecture.
    ///
    /// <see href="https://alistair.cockburn.us/hexagonal-architecture/">Hexagonal Architecture</see>
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Struct)]
    public class ApplicationAttribute : Attribute
    {
    }
}
