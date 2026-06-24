using System;

namespace NMolecules.Bricks
{
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
