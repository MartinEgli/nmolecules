using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Assigns a Bricks role to all source types whose namespace matches a configured pattern.
    /// </summary>
    /// <remarks>
    /// Use this attribute when a project uses namespace conventions as architecture boundaries.
    /// Exact namespace names match one namespace. Patterns ending in <c>*</c> match the configured
    /// namespace prefix and all nested namespaces.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module, AllowMultiple = true)]
    public sealed class NamespaceRoleAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamespaceRoleAttribute"/> class.
        /// </summary>
        /// <param name="namespacePattern">Exact namespace name or prefix pattern ending in <c>*</c>.</param>
        /// <param name="role">Role assigned to matching types.</param>
        public NamespaceRoleAttribute(string namespacePattern, string role)
        {
            NamespacePattern = namespacePattern ?? string.Empty;
            Role = role ?? string.Empty;
        }

        /// <summary>
        /// Exact namespace name or prefix pattern ending in <c>*</c>.
        /// </summary>
        public string NamespacePattern { get; }

        /// <summary>
        /// Role assigned to matching types.
        /// </summary>
        public string Role { get; }

        /// <summary>
        /// Gets the typed role identifier representation of <see cref="Role"/>.
        /// </summary>
        public RoleId RoleId => RoleId.From(Role);
    }
}
