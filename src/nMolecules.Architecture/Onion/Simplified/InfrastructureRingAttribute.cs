using System;

namespace NMolecules.Architecture.Onion.Simplified
{
    /// <summary>
    /// Identifies the infrastructure ring in a simplified onion architecture.
    ///
    /// <see href="https://jeffreypalermo.com/2008/07/the-onion-architecture-part-1/">The Onion Architecture : part 1
    ///      (Palermo)</see>
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Struct)]
    public class InfrastructureRingAttribute : Attribute
    {
    }
}
