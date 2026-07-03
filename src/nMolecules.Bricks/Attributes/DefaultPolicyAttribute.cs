using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Attribute used to declare the default owning policy for attribute metadata in the same scope.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class,
        AllowMultiple = false)]
    public class DefaultPolicyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPolicyAttribute"/> class.
        /// Intended for specialized derived attributes.
        /// </summary>
        protected DefaultPolicyAttribute()
        {
            Policy = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPolicyAttribute"/> class.
        /// </summary>
        /// <param name="policyId">Default owning policy identifier for attributes without an explicit policy id.</param>
        public DefaultPolicyAttribute(string policyId)
        {
            Policy = policyId ?? string.Empty;
        }

        /// <summary>
        /// Default owning policy identifier for attributes without an explicit policy id in the same scope.
        /// </summary>
        public virtual string Policy { get; protected set; }

        /// <summary>
        /// Gets the typed default owning policy identifier representation of <see cref="Policy"/>.
        /// </summary>
        public BrickPolicyId PolicyId => BrickPolicyId.From(Policy);
    }
}
