using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    public sealed class BrickRoleDimension
    {
        public BrickRoleDimension(BrickDimensionId id, string displayName, bool allowsMultipleRoles, bool isExclusiveByDefault, string description = null)
        {
            Id = id;
            DisplayName = displayName ?? string.Empty;
            AllowsMultipleRoles = allowsMultipleRoles;
            IsExclusiveByDefault = isExclusiveByDefault;
            Description = description;
        }

        public BrickDimensionId Id { get; }
        public string DisplayName { get; }
        public bool AllowsMultipleRoles { get; }
        public bool IsExclusiveByDefault { get; }
        public string Description { get; }
    }

    public sealed class BrickRole
    {
        public BrickRole(RoleId id, BrickDimensionId dimensionId, string displayName, string category = null, string description = null, bool isBuiltin = false)
        {
            Id = id;
            DimensionId = dimensionId;
            DisplayName = displayName ?? string.Empty;
            Category = category;
            Description = description;
            IsBuiltin = isBuiltin;
        }

        public RoleId Id { get; }
        public BrickDimensionId DimensionId { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public string Description { get; }
        public bool IsBuiltin { get; }
    }

    public sealed class BrickElement
    {
        public BrickElement(
            BrickElementId id,
            BrickElementKind kind,
            string displayName,
            string assemblyName = null,
            string namespaceName = null,
            string fullName = null,
            BrickElementOrigin origin = BrickElementOrigin.Unknown,
            BrickElementSource source = BrickElementSource.Unknown)
        {
            Id = id;
            Kind = kind;
            DisplayName = displayName ?? string.Empty;
            AssemblyName = assemblyName;
            NamespaceName = namespaceName;
            FullName = fullName;
            Origin = origin;
            Source = source;
        }

        public BrickElementId Id { get; }
        public BrickElementKind Kind { get; }
        public string DisplayName { get; }
        public string AssemblyName { get; }
        public string NamespaceName { get; }
        public string FullName { get; }
        public BrickElementOrigin Origin { get; }
        public BrickElementSource Source { get; }
    }

    public readonly struct BrickSourceLocation : IEquatable<BrickSourceLocation>
    {
        public BrickSourceLocation(string path, int line, int column)
        {
            Path = path ?? string.Empty;
            Line = line;
            Column = column;
        }

        public string Path { get; }
        public int Line { get; }
        public int Column { get; }
        public bool Equals(BrickSourceLocation other) => string.Equals(Path, other.Path, StringComparison.Ordinal) && Line == other.Line && Column == other.Column;
        public override bool Equals(object obj) => obj is BrickSourceLocation other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Path ?? string.Empty) ^ Line.GetHashCode() ^ Column.GetHashCode();
        public static bool operator ==(BrickSourceLocation left, BrickSourceLocation right) => left.Equals(right);
        public static bool operator !=(BrickSourceLocation left, BrickSourceLocation right) => !left.Equals(right);
    }

    public sealed class BrickDependency
    {
        public BrickDependency(
            BrickElement source,
            BrickElement target,
            BrickDependencyKindId kindId,
            BrickScope scope,
            BrickDependencyLayer layer,
            BrickDependencyStrength strength,
            BrickEvidenceLevel evidenceLevel,
            BrickSourceLocation? location = null,
            string detail = null)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            KindId = kindId;
            Scope = scope;
            Layer = layer;
            Strength = strength;
            EvidenceLevel = evidenceLevel;
            Location = location;
            Detail = detail;
        }

        public BrickElement Source { get; }
        public BrickElement Target { get; }
        public BrickDependencyKindId KindId { get; }
        public BrickScope Scope { get; }
        public BrickDependencyLayer Layer { get; }
        public BrickDependencyStrength Strength { get; }
        public BrickEvidenceLevel EvidenceLevel { get; }
        public BrickSourceLocation? Location { get; }
        public string Detail { get; }
    }

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

    public readonly struct BrickPolicyImport : IEquatable<BrickPolicyImport>
    {
        public BrickPolicyImport(BrickPolicyId importedPolicyId, BrickPolicyImportMode mode)
        {
            ImportedPolicyId = importedPolicyId;
            Mode = mode;
        }

        public BrickPolicyId ImportedPolicyId { get; }
        public BrickPolicyImportMode Mode { get; }
        public bool Equals(BrickPolicyImport other) => ImportedPolicyId == other.ImportedPolicyId && Mode == other.Mode;
        public override bool Equals(object obj) => obj is BrickPolicyImport other && Equals(other);
        public override int GetHashCode() => ImportedPolicyId.GetHashCode() ^ Mode.GetHashCode();
        public static bool operator ==(BrickPolicyImport left, BrickPolicyImport right) => left.Equals(right);
        public static bool operator !=(BrickPolicyImport left, BrickPolicyImport right) => !left.Equals(right);
    }

    public sealed class BrickPolicy
    {
        public BrickPolicy(BrickPolicyId id, string name, IEnumerable<BrickPolicyImport> imports, IEnumerable<BrickRule> rules, BrickPermissionDefault defaultDecision, BrickEnforcementMode enforcement)
        {
            Id = id;
            Name = name ?? string.Empty;
            Imports = (imports ?? Enumerable.Empty<BrickPolicyImport>()).ToArray();
            Rules = (rules ?? Enumerable.Empty<BrickRule>()).ToArray();
            DefaultDecision = defaultDecision;
            Enforcement = enforcement;
        }

        public BrickPolicyId Id { get; }
        public string Name { get; }
        public IReadOnlyList<BrickPolicyImport> Imports { get; }
        public IReadOnlyList<BrickRule> Rules { get; }
        public BrickPermissionDefault DefaultDecision { get; }
        public BrickEnforcementMode Enforcement { get; }
    }

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
