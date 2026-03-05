using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Assigns a type to a named architectural role.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Struct,
        AllowMultiple = true)]
    public class RoleAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoleAttribute"/> class.
        /// Intended for specialized derived attributes.
        /// </summary>
        protected RoleAttribute()
        {
            Name = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleAttribute"/> class.
        /// </summary>
        /// <param name="name">The role name.</param>
        public RoleAttribute(string name)
        {
            Name = name ?? string.Empty;
        }

        /// <summary>
        /// The role name.
        /// </summary>
        public virtual string Name { get; set; }
    }
}
