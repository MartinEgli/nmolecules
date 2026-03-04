using System;

namespace NMolecules.Architecture.Cqrs
{
    /// <summary>
    /// Identifies a command handler in the context of CQRS, i.e. logic that processes a command and may reject it.
    ///
    /// <see href="http://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf">CQRS Documents by Greg Young - Commands</see>
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method |
        AttributeTargets.Constructor)]
    public class CommandHandlerAttribute : Attribute
    {
        public string Namespace { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
