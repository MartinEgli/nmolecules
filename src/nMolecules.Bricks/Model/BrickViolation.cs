using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Represents a deterministic Bricks policy or model violation.
    /// </summary>
    /// <remarks>
    /// Use <see cref="BrickViolation"/> as the framework-neutral result model for analyzers, CI checks, reports
    /// and optional AI remediation comments. A violation records what was found; suppressions, baselines and AI
    /// advice are layered on top by extension packages.
    ///
    /// Example: see
    /// <c>../nmolecules.brick-examples/samples/bricks/violations/SelfDependencyViolationExample.cs</c>.
    /// </remarks>
    public sealed class BrickViolation
    {
        /// <summary>
        /// Creates a violation result.
        /// </summary>
        /// <param name="kind">Violation category.</param>
        /// <param name="source">Source element that caused or owns the violation.</param>
        /// <param name="message">Human-readable diagnostic message. <c>null</c> is normalised to an empty string.</param>
        /// <param name="severity">Severity used by analyzers, CI and reports.</param>
        /// <param name="state">Lifecycle state of the violation.</param>
        /// <param name="ruleId">Optional rule identifier that produced the violation.</param>
        /// <param name="ruleName">Optional rule name that produced the violation.</param>
        /// <param name="target">Optional target element for dependency violations.</param>
        /// <param name="resolvedSourceRoles">Roles resolved for the source element. <c>null</c> is treated as an empty collection.</param>
        /// <param name="resolvedTargetRoles">Roles resolved for the target element. <c>null</c> is treated as an empty collection.</param>
        /// <param name="dependencyKindId">Optional dependency kind that produced the violation.</param>
        /// <param name="scope">Architectural scope of the violation.</param>
        /// <param name="dependencyLayer">Optional layer that produced the dependency evidence.</param>
        /// <param name="evidenceLevel">Confidence level of the evidence behind the violation.</param>
        /// <param name="stateReason">Optional reason for the current lifecycle state.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <c>null</c>.</exception>
        public BrickViolation(
            BrickViolationKind kind,
            BrickElement source,
            string message,
            BrickSeverity severity,
            BrickViolationState state,
            RuleId? ruleId = null,
            string ruleName = null,
            BrickElement target = null,
            IEnumerable<RoleId> resolvedSourceRoles = null,
            IEnumerable<RoleId> resolvedTargetRoles = null,
            BrickDependencyKindId? dependencyKindId = null,
            BrickScope scope = BrickScope.Type,
            BrickDependencyLayer? dependencyLayer = null,
            BrickEvidenceLevel evidenceLevel = BrickEvidenceLevel.Unknown,
            string stateReason = null)
        {
            Kind = kind;
            RuleId = ruleId;
            RuleName = ruleName;
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target;
            ResolvedSourceRoles = (resolvedSourceRoles ?? Enumerable.Empty<RoleId>()).ToArray();
            ResolvedTargetRoles = (resolvedTargetRoles ?? Enumerable.Empty<RoleId>()).ToArray();
            DependencyKindId = dependencyKindId;
            Scope = scope;
            DependencyLayer = dependencyLayer;
            Severity = severity;
            Message = message ?? string.Empty;
            EvidenceLevel = evidenceLevel;
            State = state;
            StateReason = stateReason;
        }

        /// <summary>
        /// Violation category.
        /// </summary>
        public BrickViolationKind Kind { get; }

        /// <summary>
        /// Optional rule identifier that produced the violation.
        /// </summary>
        public RuleId? RuleId { get; }

        /// <summary>
        /// Optional rule name that produced the violation.
        /// </summary>
        public string RuleName { get; }

        /// <summary>
        /// Source element that caused or owns the violation.
        /// </summary>
        public BrickElement Source { get; }

        /// <summary>
        /// Optional target element, primarily used by dependency violations.
        /// </summary>
        public BrickElement Target { get; }

        /// <summary>
        /// Immutable snapshot of roles resolved for the source element.
        /// </summary>
        public IReadOnlyList<RoleId> ResolvedSourceRoles { get; }

        /// <summary>
        /// Immutable snapshot of roles resolved for the target element.
        /// </summary>
        public IReadOnlyList<RoleId> ResolvedTargetRoles { get; }

        /// <summary>
        /// Optional dependency kind that produced the violation.
        /// </summary>
        public BrickDependencyKindId? DependencyKindId { get; }

        /// <summary>
        /// Architectural scope of the violation.
        /// </summary>
        public BrickScope Scope { get; }

        /// <summary>
        /// Optional layer that produced the dependency evidence.
        /// </summary>
        public BrickDependencyLayer? DependencyLayer { get; }

        /// <summary>
        /// Severity used by analyzers, CI and reports.
        /// </summary>
        public BrickSeverity Severity { get; }

        /// <summary>
        /// Human-readable diagnostic message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Confidence level of the evidence behind the violation.
        /// </summary>
        public BrickEvidenceLevel EvidenceLevel { get; }

        /// <summary>
        /// Lifecycle state of the violation.
        /// </summary>
        public BrickViolationState State { get; }

        /// <summary>
        /// Optional reason for the current lifecycle state.
        /// </summary>
        public string StateReason { get; }
    }
}
