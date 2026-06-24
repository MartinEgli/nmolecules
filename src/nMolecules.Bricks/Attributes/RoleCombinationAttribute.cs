using System;

namespace NMolecules.Bricks
{
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class,
        AllowMultiple = true)]
    public class RoleCombinationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoleCombinationAttribute"/> class.
        /// Intended for specialized derived attributes.
        /// </summary>
        protected RoleCombinationAttribute()
        {
            Name = string.Empty;
            LeftRoles = string.Empty;
            RightRoles = string.Empty;
            Kind = BrickCombinationKind.Incompatible;
            Reason = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleCombinationAttribute"/> class.
        /// </summary>
        /// <param name="name">Human-readable combination rule name.</param>
        /// <param name="leftRoles">Left role selector pattern.</param>
        /// <param name="rightRoles">Right role selector pattern.</param>
        /// <param name="kind">Combination kind.</param>
        /// <param name="reason">Reason shown when the combination causes a warning or conflict.</param>
        public RoleCombinationAttribute(
            string name,
            string leftRoles,
            string rightRoles,
            BrickCombinationKind kind = BrickCombinationKind.Incompatible,
            string reason = "")
        {
            Name = name ?? string.Empty;
            LeftRoles = leftRoles ?? string.Empty;
            RightRoles = rightRoles ?? string.Empty;
            Kind = kind;
            Reason = reason ?? string.Empty;
        }

        /// <summary>
        /// Human-readable combination rule name.
        /// </summary>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// Left role selector pattern.
        /// </summary>
        public virtual string LeftRoles { get; protected set; }

        /// <summary>
        /// Gets the typed selector representation of <see cref="LeftRoles"/>.
        /// </summary>
        public BrickRoleSelector LeftRoleSelector => BrickRoleSelector.From(LeftRoles);

        /// <summary>
        /// Right role selector pattern.
        /// </summary>
        public virtual string RightRoles { get; protected set; }

        /// <summary>
        /// Gets the typed selector representation of <see cref="RightRoles"/>.
        /// </summary>
        public BrickRoleSelector RightRoleSelector => BrickRoleSelector.From(RightRoles);

        /// <summary>
        /// Combination kind.
        /// </summary>
        public virtual BrickCombinationKind Kind { get; protected set; }

        /// <summary>
        /// Reason shown when the combination causes a warning or conflict.
        /// </summary>
        public virtual string Reason { get; protected set; }
    }
}
