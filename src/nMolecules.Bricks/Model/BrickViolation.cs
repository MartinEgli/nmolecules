using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickViolation
    {
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

        public BrickViolationKind Kind { get; }
        public RuleId? RuleId { get; }
        public string RuleName { get; }
        public BrickElement Source { get; }
        public BrickElement Target { get; }
        public IReadOnlyList<RoleId> ResolvedSourceRoles { get; }
        public IReadOnlyList<RoleId> ResolvedTargetRoles { get; }
        public BrickDependencyKindId? DependencyKindId { get; }
        public BrickScope Scope { get; }
        public BrickDependencyLayer? DependencyLayer { get; }
        public BrickSeverity Severity { get; }
        public string Message { get; }
        public BrickEvidenceLevel EvidenceLevel { get; }
        public BrickViolationState State { get; }
        public string StateReason { get; }
    }
}
