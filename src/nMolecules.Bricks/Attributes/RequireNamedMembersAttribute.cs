using System;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Declares that a type annotated with a custom marker attribute must
    /// expose members for each configured marker name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class RequireNamedMembersAttribute : Attribute
    {
        private string nameArgument = "Name";

        /// <summary>
        /// Initializes a new instance of the <see cref="RequireNamedMembersAttribute"/> class.
        /// </summary>
        /// <param name="memberAttributeType">The named member marker attribute type.</param>
        /// <param name="requiredNames">The marker names that must be present.</param>
        public RequireNamedMembersAttribute(Type memberAttributeType, params string[] requiredNames)
        {
            MemberAttributeType = memberAttributeType ?? typeof(Attribute);
            RequiredNames = (requiredNames ?? Array.Empty<string>())
                .Select(NormalizeName)
                .ToArray();
        }

        /// <summary>
        /// Gets the named member marker attribute type.
        /// </summary>
        public Type MemberAttributeType { get; }

        /// <summary>
        /// Gets the marker names that must be present.
        /// </summary>
        public string[] RequiredNames { get; }

        /// <summary>
        /// Gets or sets the constructor parameter or named property that carries the marker name.
        /// </summary>
        public string NameArgument
        {
            get => nameArgument;
            set => nameArgument = string.IsNullOrWhiteSpace(value) ? "Name" : value;
        }

        private static string NormalizeName(string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value;
    }
}
