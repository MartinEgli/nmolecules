using System;

namespace NMolecules.Bricks
{
/// <summary>
    /// Declares that a type annotated with a custom marker attribute must expose
    /// at least one member for each configured marker attribute type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class RequireAllMembersAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequireAllMembersAttribute"/> class.
        /// </summary>
        /// <param name="memberAttributeTypes">The required marker attribute types.</param>
        public RequireAllMembersAttribute(params Type[] memberAttributeTypes)
        {
            MemberAttributeTypes = memberAttributeTypes ?? Array.Empty<Type>();
        }

        /// <summary>
        /// Gets the required marker attribute types.
        /// </summary>
        public Type[] MemberAttributeTypes { get; }
    }
}
