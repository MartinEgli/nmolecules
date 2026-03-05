using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Defines a generic dependency rule between two roles.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class,
        AllowMultiple = true)]
    public class RuleAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleAttribute"/> class.
        /// Intended for specialized derived attributes.
        /// </summary>
        protected RuleAttribute()
        {
            Id = string.Empty;
            SourceRole = string.Empty;
            TargetRole = string.Empty;
            Mode = RuleMode.ForbidDependency;
            Message = string.Empty;
            ExcludedSourceNameContains = string.Empty;
            ExcludedTargetNameContains = string.Empty;
            ExcludedMemberNameContains = string.Empty;
            RequiredSourceNameContains = string.Empty;
            RequiredTargetNameContains = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleAttribute"/> class.
        /// </summary>
        /// <param name="id">Consumer-defined rule key used in diagnostics.</param>
        /// <param name="sourceRole">Source role.</param>
        /// <param name="targetRole">Target role.</param>
        /// <param name="mode">Rule mode: forbid or require dependency.</param>
        /// <param name="message">Custom diagnostic message template.</param>
        /// <param name="excludedSourceNameContains">Pipe-separated source-name exclusion tokens.</param>
        /// <param name="excludedTargetNameContains">Pipe-separated target-name exclusion tokens.</param>
        /// <param name="excludedMemberNameContains">Pipe-separated member-name exclusion tokens.</param>
        /// <param name="requiredSourceNameContains">Pipe-separated source-name condition tokens.</param>
        /// <param name="requiredTargetNameContains">Pipe-separated target-name condition tokens.</param>
        public RuleAttribute(
            string id,
            string sourceRole,
            string targetRole,
            RuleMode mode = RuleMode.ForbidDependency,
            string message = "",
            string excludedSourceNameContains = "",
            string excludedTargetNameContains = "",
            string excludedMemberNameContains = "",
            string requiredSourceNameContains = "",
            string requiredTargetNameContains = "")
        {
            Id = id ?? string.Empty;
            SourceRole = sourceRole ?? string.Empty;
            TargetRole = targetRole ?? string.Empty;
            Mode = mode;
            Message = message ?? string.Empty;
            ExcludedSourceNameContains = excludedSourceNameContains ?? string.Empty;
            ExcludedTargetNameContains = excludedTargetNameContains ?? string.Empty;
            ExcludedMemberNameContains = excludedMemberNameContains ?? string.Empty;
            RequiredSourceNameContains = requiredSourceNameContains ?? string.Empty;
            RequiredTargetNameContains = requiredTargetNameContains ?? string.Empty;
        }

        /// <summary>
        /// Consumer-defined rule key used in diagnostics.
        /// </summary>
        public virtual string Id { get; protected set; }

        /// <summary>
        /// Source role name.
        /// </summary>
        public virtual string SourceRole { get; protected set; }

        /// <summary>
        /// Target role name.
        /// </summary>
        public virtual string TargetRole { get; protected set; }

        /// <summary>
        /// Rule mode.
        /// </summary>
        public virtual RuleMode Mode { get; protected set; }

        /// <summary>
        /// Optional custom message template.
        /// Supported placeholders: {rule}, {source}, {target}, {member}.
        /// </summary>
        public virtual string Message { get; protected set; }

        /// <summary>
        /// Pipe-separated source-name exclusion tokens.
        /// </summary>
        public virtual string ExcludedSourceNameContains { get; protected set; }

        /// <summary>
        /// Pipe-separated target-name exclusion tokens.
        /// </summary>
        public virtual string ExcludedTargetNameContains { get; protected set; }

        /// <summary>
        /// Pipe-separated member-name exclusion tokens.
        /// </summary>
        public virtual string ExcludedMemberNameContains { get; protected set; }

        /// <summary>
        /// Pipe-separated source-name condition tokens.
        /// </summary>
        public virtual string RequiredSourceNameContains { get; protected set; }

        /// <summary>
        /// Pipe-separated target-name condition tokens.
        /// </summary>
        public virtual string RequiredTargetNameContains { get; protected set; }
    }
}
