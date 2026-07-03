using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Assigns a Bricks role to one exact type by runtime type reference or full type name.
    /// </summary>
    /// <remarks>
    /// Use the <see cref="TypeRoleAttribute(Type, string, string)"/> constructor when the target
    /// package is referenced directly. Use the string constructor for optional or external packages
    /// where a hard assembly reference is not desirable.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module, AllowMultiple = true)]
    public sealed class TypeRoleAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeRoleAttribute"/> class.
        /// </summary>
        /// <param name="type">The exact type that receives the role.</param>
        /// <param name="role">Role assigned to the selected type.</param>
        /// <param name="policyId">Optional owning policy identifier for multi-policy attribute scopes.</param>
        public TypeRoleAttribute(Type type, string role, string policyId = "")
        {
            Type = type;
            TypeName = type == null ? string.Empty : type.FullName ?? type.Name;
            Role = role ?? string.Empty;
            Policy = policyId ?? string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeRoleAttribute"/> class.
        /// </summary>
        /// <param name="typeName">Full type name or type-display name that receives the role.</param>
        /// <param name="role">Role assigned to the selected type.</param>
        /// <param name="policyId">Optional owning policy identifier for multi-policy attribute scopes.</param>
        public TypeRoleAttribute(string typeName, string role, string policyId = "")
        {
            Type = null;
            TypeName = typeName ?? string.Empty;
            Role = role ?? string.Empty;
            Policy = policyId ?? string.Empty;
        }

        /// <summary>
        /// Gets the exact runtime type when the assignment was declared with <c>typeof</c>.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the full type name or configured type-display name.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Role assigned to the selected type.
        /// </summary>
        public string Role { get; }

        /// <summary>
        /// Gets the typed role identifier representation of <see cref="Role"/>.
        /// </summary>
        public RoleId RoleId => RoleId.From(Role);

        /// <summary>
        /// Optional owning policy identifier for multi-policy attribute scopes.
        /// Empty values mean that consumers may group the type role by owner context.
        /// </summary>
        public string Policy { get; }

        /// <summary>
        /// Gets the typed owning policy identifier representation of <see cref="Policy"/>.
        /// </summary>
        public BrickPolicyId PolicyId => BrickPolicyId.From(Policy);
    }
}
