using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Declares that a type annotated with a custom marker attribute must expose
    /// exactly one member carrying the configured marker attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class RequireExactlyOneMemberAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequireExactlyOneMemberAttribute"/> class.
        /// </summary>
        /// <param name="memberAttributeType">The required marker attribute type.</param>
        public RequireExactlyOneMemberAttribute(Type memberAttributeType)
        {
            MemberAttributeType = memberAttributeType ?? typeof(Attribute);
        }

        /// <summary>
        /// Gets the required marker attribute type.
        /// </summary>
        public Type MemberAttributeType { get; }
    }
}
