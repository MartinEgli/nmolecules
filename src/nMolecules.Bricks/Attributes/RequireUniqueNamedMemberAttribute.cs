using System;

namespace NMolecules.Bricks
{
/// <summary>
    /// Declares that a type annotated with a custom marker attribute must not
    /// expose more than one member for the same marker name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class RequireUniqueNamedMemberAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequireUniqueNamedMemberAttribute"/> class.
        /// </summary>
        /// <param name="memberAttributeType">The named member marker attribute type.</param>
        public RequireUniqueNamedMemberAttribute(Type memberAttributeType)
            : this(memberAttributeType, "Name")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequireUniqueNamedMemberAttribute"/> class.
        /// </summary>
        /// <param name="memberAttributeType">The named member marker attribute type.</param>
        /// <param name="nameArgument">The string constructor parameter or named property that carries the marker name.</param>
        public RequireUniqueNamedMemberAttribute(Type memberAttributeType, string nameArgument)
        {
            MemberAttributeType = memberAttributeType ?? typeof(Attribute);
            NameArgument = string.IsNullOrWhiteSpace(nameArgument) ? "Name" : nameArgument;
        }

        /// <summary>
        /// Gets the named member marker attribute type.
        /// </summary>
        public Type MemberAttributeType { get; }

        /// <summary>
        /// Gets the constructor parameter or named property that carries the marker name.
        /// </summary>
        public string NameArgument { get; }
    }
}
