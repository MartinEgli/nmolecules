using System;

namespace NMolecules.Architecture.Onion.Classic
{
    /// <summary>
    /// Identifies the domain service ring in a classic onion architecture.
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
    public class DomainServiceRingAttribute : Attribute
    {
    }
}
