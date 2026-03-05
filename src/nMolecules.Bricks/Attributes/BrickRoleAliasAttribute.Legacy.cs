using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Backward-compatible alias for <see cref="RoleAliasAttribute"/>.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = true,
        Inherited = false)]
    public class BrickRoleAliasAttribute : RoleAliasAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickRoleAliasAttribute"/> class.
        /// Intended for specialized derived attributes.
        /// </summary>
        protected BrickRoleAliasAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrickRoleAliasAttribute"/> class.
        /// </summary>
        /// <param name="role">The target role.</param>
        public BrickRoleAliasAttribute(string role) : base(role)
        {
        }
    }
}
