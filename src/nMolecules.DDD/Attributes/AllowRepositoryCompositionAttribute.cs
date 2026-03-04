using System;

namespace NMolecules.DDD
{
    /// <summary>
    /// Explicitly approves repository-to-repository composition for a specific type or member.
    /// This marker is intended for technical composition cases that are consciously accepted.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Method |
        AttributeTargets.Property |
        AttributeTargets.Field |
        AttributeTargets.Parameter)]
    public class AllowRepositoryCompositionAttribute : Attribute
    {
    }
}
