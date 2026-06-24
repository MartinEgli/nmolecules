using System;

namespace NMolecules.Bricks
{
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class,
        AllowMultiple = true)]
    public class PolicyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyAttribute"/> class.
        /// Intended for specialized derived attributes.
        /// </summary>
        protected PolicyAttribute()
        {
            Id = string.Empty;
            Name = string.Empty;
            DefaultDecision = BrickPermissionDefault.Allow;
            Enforcement = BrickEnforcementMode.Analyze;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyAttribute"/> class.
        /// </summary>
        /// <param name="id">Consumer-defined policy identifier.</param>
        /// <param name="name">Human-readable policy name.</param>
        /// <param name="defaultDecision">Default decision for dependencies not covered by rules.</param>
        /// <param name="enforcement">Policy enforcement mode.</param>
        public PolicyAttribute(
            string id,
            string name = "",
            BrickPermissionDefault defaultDecision = BrickPermissionDefault.Allow,
            BrickEnforcementMode enforcement = BrickEnforcementMode.Analyze)
        {
            Id = id ?? string.Empty;
            Name = name ?? string.Empty;
            DefaultDecision = defaultDecision;
            Enforcement = enforcement;
        }

        /// <summary>
        /// Consumer-defined policy identifier.
        /// </summary>
        public virtual string Id { get; protected set; }

        /// <summary>
        /// Gets the typed policy identifier representation of <see cref="Id"/>.
        /// </summary>
        public BrickPolicyId PolicyId => BrickPolicyId.From(Id);

        /// <summary>
        /// Human-readable policy name.
        /// </summary>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// Default decision for dependencies not covered by rules.
        /// </summary>
        public virtual BrickPermissionDefault DefaultDecision { get; protected set; }

        /// <summary>
        /// Policy enforcement mode.
        /// </summary>
        public virtual BrickEnforcementMode Enforcement { get; protected set; }
    }
}
