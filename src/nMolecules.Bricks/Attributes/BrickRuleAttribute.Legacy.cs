using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Backward-compatible alias for <see cref="RuleAttribute"/>.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class,
        AllowMultiple = true)]
    public class BrickRuleAttribute : RuleAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickRuleAttribute"/> class.
        /// Intended for specialized derived attributes.
        /// </summary>
        protected BrickRuleAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrickRuleAttribute"/> class.
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
        public BrickRuleAttribute(
            string id,
            string sourceRole,
            string targetRole,
            BrickRuleMode mode = BrickRuleMode.ForbidDependency,
            string message = "",
            string excludedSourceNameContains = "",
            string excludedTargetNameContains = "",
            string excludedMemberNameContains = "",
            string requiredSourceNameContains = "",
            string requiredTargetNameContains = "")
            : base(
                id,
                sourceRole,
                targetRole,
                (RuleMode)(int)mode,
                message,
                excludedSourceNameContains,
                excludedTargetNameContains,
                excludedMemberNameContains,
                requiredSourceNameContains,
                requiredTargetNameContains)
        {
        }

        /// <summary>
        /// Backward-compatible mode accessor typed as <see cref="BrickRuleMode"/>.
        /// </summary>
        public new virtual BrickRuleMode Mode
        {
            get => (BrickRuleMode)(int)base.Mode;
            protected set => base.Mode = (RuleMode)(int)value;
        }
    }
}
