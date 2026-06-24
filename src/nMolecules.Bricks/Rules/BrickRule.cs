using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public readonly struct BrickRule : IEquatable<BrickRule>
    {
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

        public RuleId RuleId { get; }
        public string Name { get; }
        public RoleId SourceRole { get; }
        public RoleId TargetRole { get; }
        public BrickDecision Decision { get; }
        public BrickScope Scope { get; }
        public BrickSeverity Severity { get; }
        public int Priority { get; }
        public string Reason { get; }
        public bool Equals(BrickRule other) => RuleId == other.RuleId && Name == other.Name && SourceRole == other.SourceRole && TargetRole == other.TargetRole && Decision == other.Decision && Scope == other.Scope && Severity == other.Severity && Priority == other.Priority && Reason == other.Reason;
        public override bool Equals(object obj) => obj is BrickRule other && Equals(other);
        public override int GetHashCode() => RuleId.GetHashCode();
        public static bool operator ==(BrickRule left, BrickRule right) => left.Equals(right);
        public static bool operator !=(BrickRule left, BrickRule right) => !left.Equals(right);
    }
}
