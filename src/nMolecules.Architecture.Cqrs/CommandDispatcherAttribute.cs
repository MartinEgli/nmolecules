using System;

namespace NMolecules.Architecture.Cqrs
{
    /// <summary>
    /// Identifies a command dispatcher in the context of CQRS, i.e. logic that dispatches a command.
    ///
    /// <see href="http://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf">CQRS Documents by Greg Young - Commands</see>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandDispatcherAttribute : Attribute
    {
        public string Dispatches { get; set; } = string.Empty;
    }
}
