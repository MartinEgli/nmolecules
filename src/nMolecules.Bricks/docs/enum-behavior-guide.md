# Bricks Enum Behavior Guide

Status: June 27, 2026

This guide documents every public enum in `NMolecules.Bricks` at value level.
It complements `api-catalog.md`: the catalog lists public types, while this
guide explains what each enum value means and whether it changes analyzer
behavior today.

Analyzer behavior labels:

- `Analyzer`: the Roslyn analyzer package reads the value and can change
  diagnostics.
- `Runtime`: deterministic Bricks runtime evaluators read the value and can
  change violations, reports, or gates.
- `Tooling`: the value is preserved for reports, exports, AI comments,
  governance, or future analyzer adapters but does not change current Roslyn
  diagnostics by itself.

## Analyzer-Backed Snippets

`RuleMode` is the enum that directly changes current package-analyzer behavior
for attribute-authored rules.

```csharp
using NMolecules.Bricks;

[assembly: Rule(
    "BRK-001",
    "ApplicationService",
    "SqlAdapter",
    RuleMode.ForbidDependency)]

[assembly: Rule(
    "BRK-002",
    "AggregateRoot",
    "DomainEvent",
    RuleMode.RequireDependency)]
```

`RuleMode.ForbidDependency` reports `XMoleculesBricks0001` when the analyzer
observes a source role depending on a target role. `RuleMode.RequireDependency`
reports `XMoleculesBricks0001` when a matching source role has no dependency to
the required target role.

Most other enums are runtime or reporting metadata today. They still matter to
analyzers because they define the stable model that analyzer adapters must
preserve when they project dependencies, roles, or violations.

```csharp
var policy = new BrickPolicy(
    BrickPolicyId.From("billing"),
    "Billing architecture",
    imports: null,
    rules: new[]
    {
        new BrickRule(
            RuleId.From("BRK-003"),
            "Application may call contracts",
            RoleId.From("ApplicationService"),
            RoleId.From("ApplicationContract"),
            BrickDecision.Allow,
            BrickScope.Type,
            BrickSeverity.Info),
        new BrickRule(
            RuleId.From("BRK-004"),
            "Application must not call SQL",
            RoleId.From("ApplicationService"),
            RoleId.From("SqlAdapter"),
            BrickDecision.Deny,
            BrickScope.Type,
            BrickSeverity.Error)
    },
    BrickPermissionDefault.Deny,
    BrickEnforcementMode.Analyze);
```

`BrickRuleEvaluator` treats `BrickDecision.Allow`, `BrickDecision.Deny`,
`BrickDecision.Require`, `BrickScope`, `BrickPermissionDefault`, and
`BrickEnforcementMode` as executable policy data. The current Roslyn analyzer
has a smaller attribute-backed surface, so unsupported enum dimensions are
documented here as runtime-backed rather than silently implied analyzer
behavior.

```csharp
var dependency = new BrickDependency(
    source,
    target,
    BrickDependencyKindId.From(BrickDependencyKinds.TypeReference),
    BrickScope.Member,
    BrickDependencyLayer.Static,
    BrickDependencyStrength.Direct,
    BrickEvidenceLevel.CompilerConfirmed);
```

Dependency evidence enums make diagnostics explainable: an analyzer can say
whether a finding came from a compiler-confirmed type reference, an inferred
runtime dependency, configuration, or lower-confidence metadata.

```csharp
var assignment = new BrickRoleAssignment(
    new BrickElementSelector(BrickElementKind.Type, "Billing.*"),
    RoleId.From("Domain"),
    BrickAssignmentMode.Convention,
    BrickAssignmentSource.Convention,
    new BrickAssignmentPrecedence(
        BrickAssignmentSpecificity.Namespace,
        BrickAssignmentAuthority.External),
    BrickAssignmentBehavior.Apply);
```

Role-assignment enums drive deterministic role resolution in the runtime model.
Analyzer adapters that emit role assignments should use the same provenance,
specificity, authority, and behavior vocabulary.

## AI

| Value | Meaning | Analyzer and tooling behavior |
| --- | --- | --- |
| `BrickAiCommentFormat.Markdown` | Emit human-readable Markdown comments. | Tooling: changes comment rendering only; deterministic analyzers do not depend on it. |
| `BrickAiCommentFormat.Json` | Emit machine-readable JSON comments. | Tooling: changes export shape for CI/IDE/agent consumers. |
| `BrickAiCommentFormat.Both` | Emit Markdown and JSON. | Tooling: produces both output channels for the same deterministic violations. |
| `BrickAiMode.Off` | Produce deterministic Bricks output only. | Tooling: AI helpers stay inactive; analyzers and runtime evaluation remain deterministic. |
| `BrickAiMode.Explain` | Create AI-readable explanations for deterministic violations. | Tooling: adds advisory explanation output, not new analyzer authority. |
| `BrickAiMode.SuggestRules` | Allow advisory rule proposals that require review. | Tooling: proposal queues may be created; promotion still requires deterministic review. |
| `BrickRemediationKind.IntroduceContract` | Add or reuse an abstraction between source and target. | Tooling: AI comments can recommend contract extraction for dependency-rule violations. |
| `BrickRemediationKind.MoveElement` | Move an element to a role, namespace, or assembly that matches policy. | Tooling: used to explain ownership or placement fixes. |
| `BrickRemediationKind.SplitRole` | Split an overloaded role into more precise roles. | Tooling: used when role modeling is too broad for a useful rule. |
| `BrickRemediationKind.AddPortAndAdapter` | Add an explicit port/adapter boundary. | Tooling: common remediation for Clean/Hexagonal boundary violations. |
| `BrickRemediationKind.ChangeDependencyDirection` | Reverse or redirect dependency direction. | Tooling: used when dependency flow violates the intended architecture direction. |
| `BrickRemediationKind.MoveToCompositionRoot` | Move wiring or construction to the composition root. | Tooling: used for runtime activation or dependency-registration findings. |
| `BrickRemediationKind.AdjustPolicy` | Change policy through explicit review. | Tooling: marks policy correction as a governed option, not an automatic fix. |
| `BrickRemediationKind.AddSuppression` | Add a reviewed suppression. | Tooling: points to exception handling, usually with owner and expiry. |
| `BrickRemediationKind.AddBaselineEntry` | Add a reviewed baseline entry. | Tooling: accepts known existing debt without hiding new violations. |
| `BrickRemediationKind.RenameOrReclassifyElement` | Rename or reassign a misleading element role. | Tooling: used when the model role, not the dependency, is wrong. |
| `BrickRemediationRisk.Low` | Local, reversible remediation. | Tooling: can be shown as low-risk guidance. |
| `BrickRemediationRisk.Medium` | Affects ownership, API, or several areas. | Tooling: signals human review before acting. |
| `BrickRemediationRisk.High` | May weaken governance or require broad migration. | Tooling: should not be auto-applied; analyzers remain advisory here. |

## Attributes

| Value | Meaning | Analyzer and tooling behavior |
| --- | --- | --- |
| `RuleMode.ForbidDependency` | A source role must not depend on a target role. | Analyzer: `BrickDependencyRuleAnalyzer` emits `XMoleculesBricks0001` when matching dependency evidence exists. |
| `RuleMode.RequireDependency` | A source role must have a dependency to a target role. | Analyzer: `BrickDependencyRuleAnalyzer` emits `XMoleculesBricks0001` when the required dependency is missing. |

## Benchmarking

| Value | Meaning | Analyzer and tooling behavior |
| --- | --- | --- |
| `BrickBenchmarkComparisonStatus.Stable` | Current result is within the configured comparison threshold. | Tooling: benchmark reports stay green; no Roslyn diagnostic changes. |
| `BrickBenchmarkComparisonStatus.Improved` | Current result is faster than baseline. | Tooling: reports a performance improvement. |
| `BrickBenchmarkComparisonStatus.Regressed` | Current result is slower beyond the allowed threshold. | Tooling: CI can fail or warn based on benchmark policy. |
| `BrickBenchmarkComparisonStatus.NoBaseline` | No baseline result exists. | Tooling: avoids false regression claims. |
| `BrickBenchmarkStatus.WithinBudget` | Measured cost is within budget. | Tooling: validates analyzer/runtime performance budgets. |
| `BrickBenchmarkStatus.OverBudget` | Measured cost exceeds budget. | Tooling: can gate performance-sensitive analyzer changes. |
| `BrickBenchmarkStatus.NotBudgeted` | No budget was configured. | Tooling: records measurement without pass/fail semantics. |
| `BrickBenchmarkSubject.RuleEvaluation` | Measures deterministic rule evaluation. | Tooling: tracks `BrickRuleEvaluator` hot paths. |
| `BrickBenchmarkSubject.RoleResolution` | Measures role-resolution behavior. | Tooling: tracks role collector/resolver cost. |
| `BrickBenchmarkSubject.PolicyComposition` | Measures policy import/composition. | Tooling: tracks policy loading and merge behavior. |
| `BrickBenchmarkSubject.ViolationProjection` | Measures violation state projection. | Tooling: tracks baseline/suppression projection cost. |
| `BrickBenchmarkSubject.RuntimeDependencyEvaluation` | Measures runtime dependency evaluators. | Tooling: tracks reflection, activation, and registration surfaces. |
| `BrickBenchmarkSubject.ReportSerialization` | Measures report serialization. | Tooling: tracks JSON/SARIF export cost. |

## Configuration

| Value | Meaning | Analyzer and tooling behavior |
| --- | --- | --- |
| `BrickConfigurationSourceKind.Generated` | Generated defaults supplied the setting. | Runtime: lowest precedence in `BrickConfigurationResolver`. |
| `BrickConfigurationSourceKind.Package` | A package supplied the setting. | Runtime: package defaults can be overridden by project settings. |
| `BrickConfigurationSourceKind.PolicyFile` | A policy file supplied the setting. | Runtime: policy files override package defaults. |
| `BrickConfigurationSourceKind.MSBuild` | MSBuild properties or items supplied the setting. | Runtime/tooling: build configuration can override policy defaults. |
| `BrickConfigurationSourceKind.AnalyzerConfig` | `.editorconfig` or analyzer config supplied the setting. | Runtime/tooling: analyzer-host settings outrank MSBuild in the resolver. |
| `BrickConfigurationSourceKind.SourceAnnotation` | Source annotations supplied the setting. | Runtime: strongest configured source in the resolver. |

## Conformance

| Value | Meaning | Analyzer and tooling behavior |
| --- | --- | --- |
| `BrickConformanceCapabilityStatus.Satisfied` | A capability is present. | Tooling: contributes to achieved conformance. |
| `BrickConformanceCapabilityStatus.Missing` | A capability is absent. | Tooling: highlights work needed before trusting diagnostics. |
| `BrickConformanceCapabilityStatus.NotApplicable` | A capability is not relevant. | Tooling: excludes a capability from gap pressure. |
| `BrickConformanceLevel.Marking` | Code can carry Bricks markers. | Tooling: first maturity level for explicit roles. |
| `BrickConformanceLevel.StaticValidation` | Static analyzer validation exists. | Tooling: indicates analyzer-backed checks are available. |
| `BrickConformanceLevel.Explainability` | Findings can be explained. | Tooling: indicates diagnostics can be traced and described. |
| `BrickConformanceLevel.PolicyFiles` | External policy files are supported. | Tooling: indicates policy can move beyond source attributes. |
| `BrickConformanceLevel.RuntimeAwareAnalysis` | Runtime evidence can participate. | Tooling: indicates reflection/DI/runtime facts can be modeled. |
| `BrickConformanceLevel.IntegrationAndAugmentation` | IDE, CI, and advisory augmentation are integrated. | Tooling: highest maturity stage in the current model. |
| `BrickConformanceLevelStatus.Achieved` | A conformance level is complete. | Tooling: included in highest contiguous achieved level. |
| `BrickConformanceLevelStatus.Partial` | A conformance level is partly complete. | Tooling: shows progress without claiming full support. |
| `BrickConformanceLevelStatus.NotStarted` | A conformance level has not begun. | Tooling: documents missing maturity. |

## Core

| Value | Meaning | Analyzer and tooling behavior |
| --- | --- | --- |
| `BrickCombinationKind.Additive` | Roles may coexist and reinforce each other. | Runtime: role resolver keeps compatible roles. |
| `BrickCombinationKind.Exclusive` | Only one role should win. | Runtime: role precedence decides the effective role. |
| `BrickCombinationKind.Incompatible` | Roles must not be combined. | Runtime: role resolver can emit role-combination violations. |
| `BrickDecision.Allow` | Matching dependency is allowed. | Runtime: permits matching dependency when it has highest priority. |
| `BrickDecision.Deny` | Matching dependency is forbidden. | Runtime: emits dependency-rule violations for matching dependencies. |
| `BrickDecision.Require` | Matching dependency is required. | Runtime: emits required-dependency violations when evidence is missing. |
| `BrickDependencyLayer.Static` | Compile-time or source-code dependency. | Tooling: preferred layer for compiler/analyzer evidence. |
| `BrickDependencyLayer.Visibility` | Visibility or friend-assembly dependency. | Runtime/tooling: used by visibility evaluator and reports. |
| `BrickDependencyLayer.Runtime` | Runtime activation, reflection, plugin, or DI dependency. | Runtime: used by runtime, activation, and reflection evaluators. |
| `BrickDependencyLayer.Configuration` | Dependency declared through configuration or wiring. | Runtime/tooling: keeps configuration evidence separate from code evidence. |
| `BrickDependencyStrength.Direct` | Source directly references target. | Tooling: strongest simple dependency evidence. |
| `BrickDependencyStrength.Indirect` | Dependency exists through an intermediate element. | Tooling: useful for transitive or visibility evidence. |
| `BrickDependencyStrength.Inferred` | Dependency was inferred. | Tooling: should usually carry supporting evidence. |
| `BrickElementKind.Unknown` | Element kind could not be mapped. | Runtime/tooling: selector wildcard or fallback value. |
| `BrickElementKind.Assembly` | Assembly or package boundary. | Runtime/tooling: used for assembly-level selection and reports. |
| `BrickElementKind.Namespace` | Namespace or logical grouping. | Runtime/tooling: used for namespace-level selection and policy data. |
| `BrickElementKind.Type` | Class, interface, struct, record, or enum. | Runtime/analyzer projection: main current static-analysis element kind. |
| `BrickElementKind.Member` | Method, constructor, property, field, or event. | Runtime/tooling: used for member-level facts and future analyzer projection. |
| `BrickElementKind.Attribute` | Attribute type or usage. | Runtime/tooling: used for metadata and role/rule attribute modeling. |
| `BrickElementKind.DependencyRegistration` | DI or composition-root registration element. | Runtime: used for dependency-registration evidence. |
| `BrickElementKind.ExternalReference` | External package, service, framework type, or system. | Runtime/tooling: keeps outside-world dependencies explicit. |
| `BrickElementOrigin.Unknown` | Origin is not known. | Tooling: fallback for incomplete evidence. |
| `BrickElementOrigin.Source` | Element was declared in source code. | Tooling: suitable for compiler/analyzer-backed reports. |
| `BrickElementOrigin.Generated` | Element was generated. | Tooling: can help analyzers explain generated-code handling. |
| `BrickElementOrigin.External` | Element belongs to an external dependency. | Tooling: supports package/service boundary reporting. |
| `BrickElementOrigin.Metadata` | Element came from metadata rather than source. | Tooling: useful for binary/reference-only analysis. |
| `BrickElementSource.Unknown` | Source channel is unknown. | Tooling: fallback provenance. |
| `BrickElementSource.Code` | Fact came from code. | Tooling: typical source for analyzer-produced facts. |
| `BrickElementSource.Configuration` | Fact came from configuration. | Tooling: separates policy/config facts from code facts. |
| `BrickElementSource.Convention` | Fact came from naming or structure convention. | Tooling: useful for convention-backed role assignment. |
| `BrickElementSource.Inference` | Fact was inferred. | Tooling: signals lower confidence or explanatory need. |
| `BrickElementSource.Import` | Fact came from imported package/profile/role pack. | Tooling: distinguishes profile-provided facts from local facts. |
| `BrickEnforcementMode.Disabled` | Policy is present but inactive. | Runtime: `BrickRuleEvaluator` returns no violations. |
| `BrickEnforcementMode.Document` | Policy documents intent only. | Runtime/tooling: can be shown without active findings. |
| `BrickEnforcementMode.Analyze` | Policy produces findings. | Runtime: deterministic evaluation runs. |
| `BrickEnforcementMode.Enforce` | Policy may break quality gates. | Runtime/tooling: findings are build/CI blocking by host policy. |
| `BrickEvidenceLevel.CompilerConfirmed` | Compiler or semantic model confirms evidence. | Tooling: strongest static evidence, suitable for analyzer diagnostics. |
| `BrickEvidenceLevel.AnalyzerInferred` | Analyzer inferred evidence from structure. | Tooling: analyzer-produced but less direct than compiler-confirmed evidence. |
| `BrickEvidenceLevel.ConfigurationDeclared` | Configuration explicitly declared evidence. | Tooling: trusted as declared policy/config input. |
| `BrickEvidenceLevel.RuntimeInferred` | Runtime observation inferred evidence. | Runtime/tooling: lower confidence than static compiler evidence. |
| `BrickEvidenceLevel.Unknown` | Evidence level is unknown. | Tooling: should be treated carefully and often reviewed. |
| `BrickPermissionDefault.Allow` | Unmatched dependencies are allowed. | Runtime: open policy, no violation when no rule matches. |
| `BrickPermissionDefault.Deny` | Unmatched dependencies are denied. | Runtime: closed policy, unmatched dependencies become violations. |
| `BrickRuleLifecycleState.Candidate` | AI observed a possible rule. | Tooling: advisory proposal cannot enforce. |
| `BrickRuleLifecycleState.Draft` | Human marked proposal for shaping. | Tooling: still non-enforcing. |
| `BrickRuleLifecycleState.Observing` | Rule may run in reporting mode. | Tooling: evidence gathering before warning/enforcement. |
| `BrickRuleLifecycleState.Warning` | Rule may emit warnings after review. | Tooling: advisory enforcement stage. |
| `BrickRuleLifecycleState.Enforced` | Rule can break builds after promotion. | Tooling/runtime: promoted rule can become active policy. |
| `BrickRuleLifecycleState.Rejected` | Proposal was discarded. | Tooling: proposal must not be enforced. |
| `BrickRuleLifecycleState.Deprecated` | Proposal or rule is no longer recommended. | Tooling: signals migration away from a rule. |
| `BrickScope.Type` | Check applies at type level. | Runtime: rule and dependency scopes must match. |
| `BrickScope.Member` | Check applies at member level. | Runtime/tooling: member-level dependencies are distinguished from type-level checks. |
| `BrickScope.Namespace` | Check applies at namespace level. | Runtime/tooling: supports namespace policy modeling. |
| `BrickScope.Assembly` | Check applies at assembly level. | Runtime/tooling: supports assembly/package boundary policies. |
| `BrickScope.Global` | Check applies to the whole model. | Runtime/tooling: used for cross-model policy facts. |
| `BrickSeverity.Info` | Informational finding. | Runtime/tooling: maps to report or SARIF note. |
| `BrickSeverity.Warning` | Review-worthy finding. | Runtime/tooling: maps to warning severity. |
| `BrickSeverity.Error` | Broken rule or build-blocking issue. | Runtime/tooling: maps to error severity. |
| `BrickViolationKind.DependencyRule` | A denied dependency was found. | Runtime/analyzer projection: maps to dependency-rule diagnostics. |
| `BrickViolationKind.RequiredDependency` | A required dependency is missing. | Runtime/analyzer projection: maps to missing-dependency diagnostics. |
| `BrickViolationKind.RoleResolution` | Role assignment/resolution produced an issue. | Runtime/tooling: emitted by role resolver. |
| `BrickViolationKind.RoleCombination` | Role-combination rule was violated. | Runtime/tooling: emitted for incompatible role combinations. |
| `BrickViolationKind.PolicyConfiguration` | Policy is invalid or unsupported. | Runtime/tooling: emitted by validators. |
| `BrickViolationKind.Baseline` | Finding relates to a baseline entry. | Runtime/tooling: adoption state projection. |
| `BrickViolationKind.Suppression` | Finding relates to a suppression entry. | Runtime/tooling: adoption state projection. |
| `BrickViolationKind.MemberCardinality` | Member-cardinality contract was violated. | Runtime/analyzer projection: maps to member-contract diagnostics. |
| `BrickViolationState.Active` | Violation still needs action. | Runtime/tooling: report as active debt. |
| `BrickViolationState.Suppressed` | Reviewed suppression applies. | Runtime/tooling: hide from active counts but keep traceability. |
| `BrickViolationState.Baselined` | Existing violation accepted as baseline. | Runtime/tooling: separates existing debt from new debt. |
| `BrickViolationState.ExpiredSuppression` | Suppression is past expiry. | Runtime/tooling: bring finding back for review. |
| `BrickViolationState.ExpiredBaseline` | Baseline entry is past expiry. | Runtime/tooling: bring accepted debt back for review. |

## Dependencies

| Value | Meaning | Analyzer and tooling behavior |
| --- | --- | --- |
| `BrickDependencyCoverageStatus.Covered` | Dependency evidence is fully observable. | Tooling: coverage reports can trust this target. |
| `BrickDependencyCoverageStatus.PartiallyObservable` | Some evidence can be seen, but not all. | Tooling: reports a known analyzer blind spot. |
| `BrickDependencyCoverageStatus.NotObservable` | Current tooling cannot observe the dependency. | Tooling: documents unsupported or external evidence. |
| `BrickDependencyCoverageStatus.InsufficientEvidence` | Evidence exists but does not meet the required level. | Tooling: asks for stronger evidence before enforcement. |

## Governance

| Value | Meaning | Analyzer and tooling behavior |
| --- | --- | --- |
| `BrickGovernanceArea.PolicyOwnership` | Ownership of policies and rules. | Tooling: governance reports track accountable owners. |
| `BrickGovernanceArea.ExceptionHandling` | Suppression and baseline discipline. | Tooling: reports exception-process readiness. |
| `BrickGovernanceArea.RolePackEvolution` | Evolution of role packs and profiles. | Tooling: reports compatibility and migration readiness. |
| `BrickGovernanceArea.CompatibilityExpectations` | Compatibility promises for consumers. | Tooling: reports API/package stability discipline. |
| `BrickGovernanceAreaStatus.Compliant` | Area satisfies governance expectations. | Tooling: green governance area. |
| `BrickGovernanceAreaStatus.Partial` | Area partially satisfies expectations. | Tooling: shows incomplete but started governance. |
| `BrickGovernanceAreaStatus.NonCompliant` | Area misses governance expectations. | Tooling: highlights operational risk. |
| `BrickGovernanceRequirementStatus.Satisfied` | Requirement is met. | Tooling: contributes to compliant area status. |
| `BrickGovernanceRequirementStatus.Missing` | Requirement is not met. | Tooling: creates governance gap evidence. |
| `BrickGovernanceRequirementStatus.NotApplicable` | Requirement does not apply. | Tooling: excludes a requirement from failure counts. |

## Policies

| Value | Meaning | Analyzer and tooling behavior |
| --- | --- | --- |
| `BrickPolicyImportMode.Import` | Bring imported policy in as-is. | Runtime: policy composer includes imported rules. |
| `BrickPolicyImportMode.Extend` | Add imported policy to the root policy. | Runtime: local policy extends imported defaults. |
| `BrickPolicyImportMode.Override` | Root policy replaces overlapping imported decisions. | Runtime: local rules win for overlapping decisions. |
| `BrickPolicyImportMode.Disable` | Disable the imported policy. | Runtime: composer records the import as disabled. |
| `BrickPolicyImportMode.Narrow` | Keep stricter or narrower imported behavior. | Runtime: composer favors safer policy output. |

## Reflection

| Value | Meaning | Analyzer and tooling behavior |
| --- | --- | --- |
| `BrickReflectionConfidence.Low` | Reflection evidence is weak. | Runtime/tooling: usually requires justification or review. |
| `BrickReflectionConfidence.Medium` | Reflection evidence is plausible. | Runtime/tooling: default minimum for many reflection policies. |
| `BrickReflectionConfidence.High` | Reflection evidence is strong. | Runtime/tooling: acceptable for regular reporting. |

## Roadmap

| Value | Meaning | Analyzer and tooling behavior |
| --- | --- | --- |
| `BrickRoadmapItemStatus.Completed` | Roadmap item is complete. | Tooling: counts as implemented capability. |
| `BrickRoadmapItemStatus.Missing` | Roadmap item is missing. | Tooling: shows implementation gap. |
| `BrickRoadmapItemStatus.NotRequired` | Roadmap item is intentionally not required. | Tooling: prevents false gap reporting. |
| `BrickRoadmapStage.V1` | Baseline Bricks stage. | Tooling: first delivery milestone. |
| `BrickRoadmapStage.V1_1` | Incremental V1 extension. | Tooling: staged follow-up milestone. |
| `BrickRoadmapStage.V1_2` | Second V1 extension. | Tooling: staged follow-up milestone. |
| `BrickRoadmapStage.V2` | Larger future Bricks model. | Tooling: long-range milestone. |
| `BrickRoadmapStageStatus.Complete` | Stage is complete. | Tooling: contributes to highest contiguous completed stage. |
| `BrickRoadmapStageStatus.Partial` | Stage is partially complete. | Tooling: shows progress without closure. |
| `BrickRoadmapStageStatus.NotStarted` | Stage has not started. | Tooling: marks future work. |

## Roles

| Value | Meaning | Analyzer and tooling behavior |
| --- | --- | --- |
| `BrickAssignmentAuthority.Derived` | Lowest authority, derived assignment. | Runtime: loses against alias, external, and direct assignments. |
| `BrickAssignmentAuthority.Alias` | Assignment came through alias mapping. | Runtime: stronger than derived, weaker than external/direct. |
| `BrickAssignmentAuthority.External` | Assignment came from policy/configuration. | Runtime: stronger than alias, weaker than direct. |
| `BrickAssignmentAuthority.Direct` | Assignment came from explicit source metadata. | Runtime: strongest authority. |
| `BrickAssignmentBehavior.Apply` | Assignment contributes a role. | Runtime: role resolver includes the role. |
| `BrickAssignmentBehavior.Suppress` | Assignment suppresses a matching role. | Runtime: role resolver removes or blocks the role according to precedence. |
| `BrickAssignmentMode.DirectAttribute` | Role came from a direct role attribute. | Runtime/analyzer projection: current analyzer role source maps here conceptually. |
| `BrickAssignmentMode.ExternalConfiguration` | Role came from external configuration. | Runtime/tooling: policy-file role assignment. |
| `BrickAssignmentMode.Convention` | Role came from convention. | Runtime/tooling: naming/structure-based assignment. |
| `BrickAssignmentMode.Inference` | Role was inferred by analysis. | Runtime/tooling: lower-confidence role source. |
| `BrickAssignmentMode.AliasMapping` | Role came from alias mapping. | Runtime/tooling: role bridge or alias import. |
| `BrickAssignmentMode.ImportedPack` | Role came from imported role pack. | Runtime/tooling: profile/package-provided role. |
| `BrickAssignmentMode.Generated` | Role was generated by tooling. | Runtime/tooling: generated assignment provenance. |
| `BrickAssignmentSource.SourceAttribute` | Source-code attribute supplied the role. | Runtime/analyzer projection: direct annotation provenance. |
| `BrickAssignmentSource.PolicyFile` | Policy file supplied the role. | Runtime/tooling: external policy provenance. |
| `BrickAssignmentSource.Convention` | Convention supplied the role. | Runtime/tooling: convention provenance. |
| `BrickAssignmentSource.Inference` | Analysis inferred the role. | Runtime/tooling: inferred provenance. |
| `BrickAssignmentSource.AliasMapping` | Alias mapping supplied the role. | Runtime/tooling: alias provenance. |
| `BrickAssignmentSource.Package` | Package supplied the role. | Runtime/tooling: role-pack/profile provenance. |
| `BrickAssignmentSource.Generator` | Generator supplied the role. | Runtime/tooling: source-generator provenance. |
| `BrickAssignmentSpecificity.Inference` | Broad inferred assignment. | Runtime: weakest specificity. |
| `BrickAssignmentSpecificity.Convention` | Convention-level assignment. | Runtime: stronger than inference. |
| `BrickAssignmentSpecificity.Assembly` | Assembly-level assignment. | Runtime: stronger than convention. |
| `BrickAssignmentSpecificity.Namespace` | Namespace-level assignment. | Runtime: stronger than assembly. |
| `BrickAssignmentSpecificity.Element` | Direct element assignment. | Runtime: strongest specificity. |

