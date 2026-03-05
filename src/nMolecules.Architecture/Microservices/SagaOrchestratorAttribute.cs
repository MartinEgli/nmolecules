using System;

namespace NMolecules.Architecture.Microservices
{
    /// <summary>
    /// Identifies a saga orchestrator in a distributed workflow.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Struct)]
    public class SagaOrchestratorAttribute : Attribute
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
