using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Attribute used to declare policy import metadata for attribute-based role, rule, dependency, and contract
    /// configuration.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class,
        AllowMultiple = true)]
    public class PolicyImportAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyImportAttribute"/> class.
        /// Intended for specialized derived attributes.
        /// </summary>
        protected PolicyImportAttribute()
        {
            Id = string.Empty;
            Policy = string.Empty;
            Mode = BrickPolicyImportMode.Import;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyImportAttribute"/> class.
        /// </summary>
        /// <param name="id">Identifier of the imported policy.</param>
        /// <param name="policy">Identifier of the policy that owns this import.</param>
        /// <param name="mode">Composition mode for the imported policy.</param>
        public PolicyImportAttribute(
            string id,
            string policy,
            BrickPolicyImportMode mode = BrickPolicyImportMode.Import)
        {
            Id = id ?? string.Empty;
            Policy = policy ?? string.Empty;
            Mode = mode;
        }

        /// <summary>
        /// Identifier of the imported policy.
        /// </summary>
        public virtual string Id { get; protected set; }

        /// <summary>
        /// Identifier of the policy that owns this import.
        /// </summary>
        /// <remarks>
        /// This value links the import to the <see cref="PolicyAttribute"/> that owns it.
        /// </remarks>
        public virtual string Policy { get; protected set; }

        /// <summary>
        /// Gets the typed imported policy identifier representation of <see cref="Id"/>.
        /// </summary>
        public BrickPolicyId ImportedPolicyId => BrickPolicyId.From(Id);

        /// <summary>
        /// Gets the typed owner policy identifier representation of <see cref="Policy"/>.
        /// </summary>
        public BrickPolicyId OwnerPolicyId => BrickPolicyId.From(Policy);

        /// <summary>
        /// Composition mode for the imported policy.
        /// </summary>
        public virtual BrickPolicyImportMode Mode { get; protected set; }
    }
}
