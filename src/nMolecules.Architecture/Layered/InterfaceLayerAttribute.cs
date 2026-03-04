using System;

namespace NMolecules.Architecture.Layered
{
    /// <summary>
    /// Identifies the interface layer in a layered architecture. This is the jMolecules-compatible name for the
    /// outer layer that handles requests from users or other systems.
    ///
    /// <see href="https://domainlanguage.com/wp-content/uploads/2016/05/DDD_Reference_2015-03.pdf">Domain-Driven Design
    ///      Reference (Evans) - Layered Architecture</see>
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Struct)]
    public class InterfaceLayerAttribute : Attribute
    {
    }
}
