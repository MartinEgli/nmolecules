using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Defines a policy rule between a source role and a target role.
    /// </summary>
    /// <remarks>
    /// Use <see cref="BrickRule"/> inside <see cref="BrickPolicy"/> to allow, deny or require dependencies for
    /// elements that have resolved Bricks roles. Rules are deterministic and are evaluated by
    /// <see cref="BrickRuleEvaluator"/> without mutating the policy or the observed dependencies.
    ///
    /// Example: see
    /// <c>../nmolecules.brick-examples/samples/bricks/implementation-samples/function-coverage/PolicyAndResolutionExamples.cs</c>.
    /// </remarks>
    public readonly struct BrickRule : IEquatable<BrickRule>
    {
        /// <summary>
        /// Creates a policy rule.
        /// </summary>
        /// <param name="ruleId">Stable rule identifier used by diagnostics and reports.</param>
        /// <param name="name">Human-readable rule name. <c>null</c> is normalised to an empty string.</param>
        /// <param name="sourceRole">Role required on the dependency source.</param>
        /// <param name="targetRole">Role required on the dependency target.</param>
        /// <param name="decision">Rule decision, for example allow, deny or require.</param>
        /// <param name="scope">Architectural scope in which the rule applies.</param>
        /// <param name="severity">Severity used when the rule creates a violation.</param>
        /// <param name="priority">Priority used to resolve competing permission rules.</param>
        /// <param name="reason">Optional explanation shown in documentation or reports.</param>
        public BrickRule(RuleId ruleId, string name, RoleId sourceRole, RoleId targetRole, BrickDecision decision, BrickScope scope = BrickScope.Type, BrickSeverity severity = BrickSeverity.Error, int priority = 0, string reason = null)
        {
            RuleId = ruleId;
            Name = name ?? string.Empty;
            SourceRole = sourceRole;
            TargetRole = targetRole;
            Decision = decision;
            Scope = scope;
            Severity = severity;
            Priority = priority;
            Reason = reason;
        }

        /// <summary>
        /// Stable rule identifier used by diagnostics and reports.
        /// </summary>
        public RuleId RuleId { get; }

        /// <summary>
        /// Human-readable rule name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Role required on the dependency source.
        /// </summary>
        public RoleId SourceRole { get; }

        /// <summary>
        /// Role required on the dependency target.
        /// </summary>
        public RoleId TargetRole { get; }

        /// <summary>
        /// Rule decision, for example allow, deny or require.
        /// </summary>
        public BrickDecision Decision { get; }

        /// <summary>
        /// Architectural scope in which the rule applies.
        /// </summary>
        public BrickScope Scope { get; }

        /// <summary>
        /// Severity used when the rule creates a violation.
        /// </summary>
        public BrickSeverity Severity { get; }

        /// <summary>
        /// Priority used to resolve competing permission rules.
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// Optional explanation shown in documentation or reports.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Determines whether this rule has the same value as another rule.
        /// </summary>
        /// <param name="other">Rule to compare with this instance.</param>
        /// <returns><c>true</c> when both rules carry the same values; otherwise <c>false</c>.</returns>
        public bool Equals(BrickRule other) => RuleId == other.RuleId && Name == other.Name && SourceRole == other.SourceRole && TargetRole == other.TargetRole && Decision == other.Decision && Scope == other.Scope && Severity == other.Severity && Priority == other.Priority && Reason == other.Reason;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is BrickRule other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => RuleId.GetHashCode();

        /// <summary>
        /// Determines whether two rules have the same value.
        /// </summary>
        public static bool operator ==(BrickRule left, BrickRule right) => left.Equals(right);

        /// <summary>
        /// Determines whether two rules have different values.
        /// </summary>
        public static bool operator !=(BrickRule left, BrickRule right) => !left.Equals(right);
    }
}
