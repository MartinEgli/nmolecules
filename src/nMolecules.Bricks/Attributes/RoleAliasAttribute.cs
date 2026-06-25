using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Attribute used to declare role alias metadata for attribute-based role, rule, dependency, and contract
    /// configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RoleAliasAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoleAliasAttribute"/> class.
        /// Intended for specialized derived attributes.
        /// </summary>
        protected RoleAliasAttribute()
        {
            Role = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleAliasAttribute"/> class.
        /// </summary>
        /// <param name="role">The target role.</param>
        public RoleAliasAttribute(string role)
        {
            Role = role ?? string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleAliasAttribute"/> class.
        /// </summary>
        /// <param name="role">The typed target role identifier.</param>
        protected RoleAliasAttribute(RoleId role)
        {
            Role = role.Value;
        }

        /// <summary>
        /// The mapped role.
        /// </summary>
        public virtual string Role { get; protected set; }

        /// <summary>
        /// Gets the typed role identifier representation of <see cref="Role"/>.
        /// </summary>
        public RoleId RoleId => RoleId.From(Role);
    }
}
