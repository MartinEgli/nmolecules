using System;

namespace NMolecules.Architecture.EventStorming
{
    /// <summary>
    /// Identifies a policy in an Event Storming model.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Struct)]
    public class PolicyAttribute : Attribute
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
