

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes which kind of architecture element a Bricks fact refers to.
    /// Use it when selecting, reporting, or evaluating dependencies at assembly,
    /// namespace, type, member, or infrastructure-registration level.
    /// </summary>
    public enum BrickElementKind
    {
        /// <summary>The element kind is not known or could not be mapped.</summary>
        Unknown = 0,
        /// <summary>A .NET assembly or comparable package boundary.</summary>
        Assembly = 1,
        /// <summary>A namespace or logical namespace-like grouping.</summary>
        Namespace = 2,
        /// <summary>A type such as a class, interface, struct, record, or enum.</summary>
        Type = 3,
        /// <summary>A member such as a method, constructor, property, field, or event.</summary>
        Member = 4,
        /// <summary>An attribute type or attribute usage that participates in role or rule metadata.</summary>
        Attribute = 5,
        /// <summary>A dependency-injection, composition-root, or service-registration element.</summary>
        DependencyRegistration = 6,
        /// <summary>An external package, service, framework type, or system outside the analyzed codebase.</summary>
        ExternalReference = 7
    }
}
