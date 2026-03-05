using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Maps a custom attribute type to a role.
    /// Apply this to attribute classes to create custom role markers.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = true,
        Inherited = false)]
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
        /// The mapped role.
        /// </summary>
        public virtual string Role { get; protected set; }
    }
}
