using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Backward-compatible alias for <see cref="RoleAttribute"/>.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Struct,
        AllowMultiple = true)]
    public class BrickRoleAttribute : RoleAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickRoleAttribute"/> class.
        /// Intended for specialized derived attributes.
        /// </summary>
        protected BrickRoleAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrickRoleAttribute"/> class.
        /// </summary>
        /// <param name="name">The role name.</param>
        public BrickRoleAttribute(string name) : base(name)
        {
        }
    }
}
