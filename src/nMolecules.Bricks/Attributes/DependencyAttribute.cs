using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Attribute used to declare dependency metadata for attribute-based role, rule, dependency, and contract
    /// configuration.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class,
        AllowMultiple = true)]
    public class DependencyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyAttribute"/> class.
        /// Intended for specialized derived attributes.
        /// </summary>
        protected DependencyAttribute()
        {
            Id = string.Empty;
            Source = string.Empty;
            Target = string.Empty;
            Policy = string.Empty;
            Kind = string.Empty;
            Scope = BrickScope.Type;
            Layer = BrickDependencyLayer.Static;
            Strength = BrickDependencyStrength.Direct;
            EvidenceLevel = BrickEvidenceLevel.CompilerConfirmed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyAttribute"/> class.
        /// </summary>
        /// <param name="id">Consumer-defined dependency identifier.</param>
        /// <param name="source">Source element identifier or display name.</param>
        /// <param name="target">Target element identifier or display name.</param>
        /// <param name="kind">Dependency kind identifier.</param>
        /// <param name="scope">Dependency scope.</param>
        /// <param name="layer">Dependency observation layer.</param>
        /// <param name="strength">Dependency strength.</param>
        /// <param name="evidenceLevel">Dependency evidence level.</param>
        /// <param name="policyId">Optional owning policy identifier for multi-policy attribute scopes.</param>
        public DependencyAttribute(
            string id,
            string source,
            string target,
            string kind = BrickDependencyKinds.TypeReference,
            BrickScope scope = BrickScope.Type,
            BrickDependencyLayer layer = BrickDependencyLayer.Static,
            BrickDependencyStrength strength = BrickDependencyStrength.Direct,
            BrickEvidenceLevel evidenceLevel = BrickEvidenceLevel.CompilerConfirmed,
            string policyId = "")
        {
            Id = id ?? string.Empty;
            Source = source ?? string.Empty;
            Target = target ?? string.Empty;
            Policy = policyId ?? string.Empty;
            Kind = kind ?? string.Empty;
            Scope = scope;
            Layer = layer;
            Strength = strength;
            EvidenceLevel = evidenceLevel;
        }

        /// <summary>
        /// Consumer-defined dependency identifier.
        /// </summary>
        public virtual string Id { get; protected set; }

        /// <summary>
        /// Source element identifier or display name.
        /// </summary>
        public virtual string Source { get; protected set; }

        /// <summary>
        /// Target element identifier or display name.
        /// </summary>
        public virtual string Target { get; protected set; }

        /// <summary>
        /// Optional owning policy identifier for multi-policy attribute scopes.
        /// Empty values mean that consumers may group the dependency by owner
        /// context or by a documented ID convention.
        /// </summary>
        public virtual string Policy { get; protected set; }

        /// <summary>
        /// Gets the typed owning policy identifier representation of <see cref="Policy"/>.
        /// </summary>
        public BrickPolicyId PolicyId => BrickPolicyId.From(Policy);

        /// <summary>
        /// Dependency kind identifier.
        /// </summary>
        public virtual string Kind { get; protected set; }

        /// <summary>
        /// Gets the typed dependency kind identifier.
        /// </summary>
        public BrickDependencyKindId KindId => BrickDependencyKindId.From(Kind);

        /// <summary>
        /// Dependency scope.
        /// </summary>
        public virtual BrickScope Scope { get; protected set; }

        /// <summary>
        /// Dependency observation layer.
        /// </summary>
        public virtual BrickDependencyLayer Layer { get; protected set; }

        /// <summary>
        /// Dependency strength.
        /// </summary>
        public virtual BrickDependencyStrength Strength { get; protected set; }

        /// <summary>
        /// Dependency evidence level.
        /// </summary>
        public virtual BrickEvidenceLevel EvidenceLevel { get; protected set; }
    }
}
