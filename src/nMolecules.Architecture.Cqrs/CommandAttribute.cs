using System;

namespace NMolecules.Architecture.Cqrs
{
    /// <summary>
    /// Identifies a command in the context of CQRS, i.e. a request to the system to change state.
    ///
    /// <see href="http://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf">CQRS Documents by Greg Young - Commands</see>
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Struct)]
    public class CommandAttribute : Attribute
    {
        public string Namespace { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
