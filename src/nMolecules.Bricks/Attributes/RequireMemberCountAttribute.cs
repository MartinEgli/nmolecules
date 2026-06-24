using System;

namespace NMolecules.Bricks
{
/// <summary>
    /// Declares that a type annotated with a custom marker attribute must expose
    /// exactly the configured number of members carrying the configured marker
    /// attribute type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class RequireMemberCountAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequireMemberCountAttribute"/> class.
        /// </summary>
        /// <param name="memberAttributeType">The counted marker attribute type.</param>
        /// <param name="count">The exact number of required members.</param>
        public RequireMemberCountAttribute(Type memberAttributeType, int count)
        {
            MemberAttributeType = memberAttributeType ?? typeof(Attribute);
            Count = count;
        }

        /// <summary>
        /// Gets the counted marker attribute type.
        /// </summary>
        public Type MemberAttributeType { get; }

        /// <summary>
        /// Gets the exact number of required members.
        /// </summary>
        public int Count { get; }
    }
}
