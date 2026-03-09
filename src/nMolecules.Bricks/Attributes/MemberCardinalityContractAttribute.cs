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

    /// <summary>
    /// Declares that a type annotated with a custom marker attribute must expose
    /// members for exactly one of two configured marker attribute types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class RequireExclusiveChoiceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequireExclusiveChoiceAttribute"/> class.
        /// </summary>
        /// <param name="leftMemberAttributeType">The first mutually exclusive marker type.</param>
        /// <param name="rightMemberAttributeType">The second mutually exclusive marker type.</param>
        public RequireExclusiveChoiceAttribute(Type leftMemberAttributeType, Type rightMemberAttributeType)
        {
            LeftMemberAttributeType = leftMemberAttributeType ?? typeof(Attribute);
            RightMemberAttributeType = rightMemberAttributeType ?? typeof(Attribute);
        }

        /// <summary>
        /// Gets the first mutually exclusive marker type.
        /// </summary>
        public Type LeftMemberAttributeType { get; }

        /// <summary>
        /// Gets the second mutually exclusive marker type.
        /// </summary>
        public Type RightMemberAttributeType { get; }
    }
}
