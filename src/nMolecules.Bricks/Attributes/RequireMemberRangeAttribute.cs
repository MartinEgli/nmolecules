using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Declares that a type annotated with a custom marker attribute must expose
    /// a number of members carrying the configured marker attribute type within
    /// the configured inclusive range.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class RequireMemberRangeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequireMemberRangeAttribute"/> class.
        /// </summary>
        /// <param name="memberAttributeType">The counted marker attribute type.</param>
        /// <param name="minimumCount">The inclusive minimum number of members.</param>
        /// <param name="maximumCount">The inclusive maximum number of members.</param>
        public RequireMemberRangeAttribute(Type memberAttributeType, int minimumCount, int maximumCount)
        {
            MemberAttributeType = memberAttributeType ?? typeof(Attribute);
            MinimumCount = minimumCount;
            MaximumCount = maximumCount;
        }

        /// <summary>
        /// Gets the counted marker attribute type.
        /// </summary>
        public Type MemberAttributeType { get; }

        /// <summary>
        /// Gets the inclusive minimum number of members.
        /// </summary>
        public int MinimumCount { get; }

        /// <summary>
        /// Gets the inclusive maximum number of members.
        /// </summary>
        public int MaximumCount { get; }
    }
}
