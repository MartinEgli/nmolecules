using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Declares that a policy imports another policy so attribute-only
    /// configurations can express policy composition beside their rules.
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
            Mode = BrickPolicyImportMode.Import;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyImportAttribute"/> class.
        /// </summary>
        /// <param name="id">Identifier of the imported policy.</param>
        /// <param name="mode">Composition mode for the imported policy.</param>
        public PolicyImportAttribute(
            string id,
            BrickPolicyImportMode mode = BrickPolicyImportMode.Import)
        {
            Id = id ?? string.Empty;
            Mode = mode;
        }

        /// <summary>
        /// Identifier of the imported policy.
        /// </summary>
        public virtual string Id { get; protected set; }

        /// <summary>
        /// Gets the typed imported policy identifier representation of <see cref="Id"/>.
        /// </summary>
        public BrickPolicyId PolicyId => BrickPolicyId.From(Id);

        /// <summary>
        /// Composition mode for the imported policy.
        /// </summary>
        public virtual BrickPolicyImportMode Mode { get; protected set; }
    }
}
