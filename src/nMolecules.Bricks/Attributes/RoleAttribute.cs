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
        /// Initializes a new instance of the <see cref="RoleAttribute"/> class.
        /// </summary>
        /// <param name="id">The typed role identifier.</param>
        protected RoleAttribute(RoleId id)
        {
            Name = id.Value;
        }

        /// <summary>
        /// The role name.
        /// </summary>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// Gets the typed role identifier representation of <see cref="Name"/>.
        /// </summary>
        public RoleId Id => RoleId.From(Name);
    }
}
