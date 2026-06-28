using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Declares that a type annotated with a custom marker attribute must not
    /// expose members carrying the configured marker attribute type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ForbidMemberAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForbidMemberAttribute"/> class.
        /// </summary>
        /// <param name="memberAttributeType">The forbidden marker attribute type.</param>
        public ForbidMemberAttribute(Type memberAttributeType)
        {
            MemberAttributeType = memberAttributeType ?? typeof(Attribute);
        }

        /// <summary>
        /// Gets the forbidden marker attribute type.
        /// </summary>
        public Type MemberAttributeType { get; }
    }
}
