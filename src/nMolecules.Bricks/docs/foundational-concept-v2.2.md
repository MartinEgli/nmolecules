# Bricks Foundational Concept

> Archive note, June 27, 2026: This document is retained as a historical
> concept draft. The active Bricks baseline is `foundational-concept.md` plus
> the executable roundtrip in `docs/bricks-roundtrip.md`. Diagnostic ID ranges
> mentioned below may be superseded; shipped member-contract diagnostics use
> `XMoleculesBricks0003` to `XMoleculesBricks0009`.

Status baseline: March 15, 2026
Concept version: 2.2
Scope: merged foundational concept for `NMolecules.Bricks`

This document defines the foundational concept for `NMolecules.Bricks`.

It is intentionally stricter than a marketing description and broader than the
currently shipped API surface. It is not a replacement for existing nMolecules
concept packages. It is a generic semantic foundation that complements them.

Version 2.2 merges two concept lines:

- the current repository baseline, which is stronger in rule semantics,
  requirement evaluation, scope handling, role-combination validation, and
  assignment precedence
- concept v2.1, which is stronger in governance, public API boundaries,
  policy composition, configuration, evidence levels, baselines, suppressions,
  diagnostic ID governance, schema versioning, and roadmap staging

The goal is to provide one stable conceptual reference for future API design,
analyzer growth, testing, reporting, documentation, exports, and IDE
integrations.

## Purpose

`NMolecules.Bricks` is a semantic structural role-and-rule framework for .NET
codebases.

Its core job is to:

- classify .NET artifacts semantically
- assign roles explicitly or indirectly
- resolve roles deterministically
- define structural rules between resolved roles
- validate dependencies, requirements, and role combinations consistently
- surface violations in analyzers, builds, tests, reports, and exports
- support incremental adoption in existing systems

The framework exists to reduce recurring structural problems:

1. implicit architecture that only exists in team knowledge
2. dependency erosion across intended boundaries
3. poor semantic classification of legacy, external, generated,
   infrastructure, or test-only code
4. inconsistent rule expression across analyzers, tests, and documentation
5. missing generic foundations beneath specialised packs such as DDD, events,
   layered architecture, hexagonal architecture, CQRS, or future architecture
   profiles

## Boundary Statement

Bricks does not replace existing nMolecules concept packages.

Bricks is not:

- only a DDD library
- only a namespace-rule checker
- only a bag of marker attributes
- only an analyzer package
- only a documentation format
- only a dependency graph exporter

Bricks is:

> a semantic role, policy, and violation model for structural code boundaries
> in .NET

DDD, events, layered architecture, onion architecture, hexagonal architecture,
CQRS, and future role packs are domain-specific or architectural
specialisations on top of that foundation.

## Core Principle

The foundational principle is:

> Elements carry roles.
> Roles are resolved from one or more assignment sources.
> Rules evaluate relationships between resolved roles.
> Violations are emitted as normalized, traceable, and explainable findings.

This makes Bricks broader than a pure attribute library, while still allowing
attributes to remain the primary current implementation path.

## Core, Packs, Profiles, And Policies

Bricks separates four concerns.

### Core

The Core defines the generic semantic model:

- elements
- roles
- role dimensions
- assignments
- dependencies
- rules
- policies
- violations
- traces
- evidence
- baselines
- suppressions

The Core does not define one mandatory architecture style.

### Pack

A Pack contributes reusable role definitions and optional annotations.

Examples:

- DDD Pack
- Events Pack
- Architecture Pack
- CQRS Pack

A Pack describes concepts. It does not necessarily enforce rules.

### Profile

A Profile contributes recommended rule sets for an architectural style.

Examples:

- Layered Profile
- Onion Profile
- Hexagonal Profile
- CQRS Profile

A Profile expresses typical structural expectations but may be adapted by a
project policy.

### Policy

A Policy is the concrete activated rule set for a project.

A Policy may:

- import packs
- import profiles
- activate rules
- override selected rules
- configure severities
- define local exceptions
- declare baselines
- declare suppressions

Policies must be explicit. Bricks must not merge policies implicitly.

## Design Tenets

### Semantics Before Syntax

Namespaces, folders, file names, and solution layout may help with discovery,
but they are not the authoritative semantic model. Roles are.

### Existing Code Must Be Addressable

Bricks should not require a full refactor before a system becomes classifiable.
The model therefore supports external, indirect, conventional, imported, or
generated role assignment.

### .NET Reality Is Part Of The Model

Assemblies, `internal`, `InternalsVisibleTo`, DI registrations, generated code,
partial types, source generators, multi-targeting, conditional compilation, and
reflection are not edge cases. They influence structural truth and belong in the
concept model.

### Rules Must Be Deterministic

The same code and the same policy input must always yield the same structural
classification, the same resolved role set, and the same violations for the
same compilation context.

### Roles Are First-Class

Concepts such as business partitions, contracts, shared code, generated code,
test-only code, legacy areas, infrastructure areas, or specialised DDD roles
should be modelled as roles or role families, not as ad hoc special cases.

### Resolution Must Be Explainable

Resolved roles and emitted violations must be explainable through traces,
precedence decisions, matched rules, and evidence.

### Adoption Must Be Incremental

Real systems contain tolerated debt. Baselines, suppressions, controlled policy
rollout, and conformance levels are therefore part of the model, not
afterthoughts.

### Public API Must Stay Small

Not every conceptual type is public API. Analyzer-internal types may evolve
faster than public contracts. Serializable result contracts must be versioned.
Public attribute and ID types must remain stable.

## Current Baseline

The current Bricks implementation is intentionally smaller than the full target
model. Today the shipped baseline covers:

- direct role assignment via `RoleAttribute`
- alias-based role mapping via `RoleAliasAttribute`
- assembly- or symbol-level rule declarations via `RuleAttribute`
- rule narrowing via `RuleFilterAttribute` and built-in name filters
- typed identifiers via `RoleId` and `RuleId`
- rule message shaping via `RuleMessage`
- analyzer-backed dependency diagnostics:
  - `XMoleculesBricks0001`
  - `XMoleculesBricks0002`
- analyzer-backed member-cardinality contracts:
  - `RequireExactlyOneMemberAttribute`
  - `RequireAllMembersAttribute`
  - `RequireMemberCountAttribute`
  - `RequireExclusiveChoiceAttribute`
  - `XMoleculesBricks0003` to `XMoleculesBricks0009`

That means Bricks is already more than a marker package, but still narrower
than the full meta-model described below.

## Current Gaps

The following parts are conceptually important but not yet modelled as
first-class runtime or analyzer abstractions:

- explicit element records such as `BrickElement`
- explicit role dimensions
- traceable role resolution output
- formal role-resolution results with visible suppression and conflicts
- external role-assignment sources beyond attribute and alias evaluation
- formal provider extensibility for role assignment
- generalized dependency kinds such as DI registration, reflection access,
  visibility, or runtime wiring
- confidence and evidence levels for non-static dependencies
- explicit policy defaults and precedence rules
- explicit policy composition
- standardized violation records reusable outside the analyzer pipeline
- baseline and suppression models
- built-in role packs as formal packaged concepts
- export and reporting surfaces
- compatibility bridges to specialised nMolecules packs
- schema versioning for policy files and reports

These are expected growth areas from the current Bricks baseline toward a
stronger structural platform.

## Conceptual Layering

Bricks distinguishes three internal model layers.

### Concept Model

The Concept Model defines the stable semantic language:

- role
- role dimension
- assignment
- rule
- policy
- baseline
- suppression
- decision
- severity

This model should be stable and suitable for public API contracts where the API
pressure justifies it.

### Analysis Model

The Analysis Model represents discovered code structure:

- element
- dependency
- source location
- symbol identity
- compilation context
- resolution trace
- evidence

This model may be Roslyn-specific and can evolve more quickly. Not every part
should become public API.

### Result Model

The Result Model represents serializable and portable output:

- violation
- role map
- dependency graph
- rule matrix
- resolution trace export
- baseline report
- suppression report

This model must be schema-versioned if used outside the analyzer process.

## Public API Boundary

Bricks must explicitly distinguish stable public contracts from internal
implementation details.

### Stable Public API Candidates

The following are candidates for stable public API:

- role attributes
- rule attributes
- alias attributes
- typed IDs
- public policy definitions
- public severity and decision enums
- suppression attributes or configuration contracts
- result DTOs for exported reports

### Analyzer-Internal Candidates

The following should initially remain analyzer-internal:

- concrete Roslyn symbol adapters
- detailed dependency extraction objects
- low-level syntax and semantic model helpers
- transient resolution candidates
- performance caches
- compilation-specific graphs

### Versioned Export Contracts

Export and report models should be versioned explicitly.

Example:

```json
{
  "schema": "NMolecules.Bricks.Report/1.0"
}
```

Policy files should also carry a schema version.

Example:

```json
{
  "schema": "NMolecules.Bricks.Policy/1.0"
}
```

## Target Meta-Model

The long-term model is built from these conceptual building blocks.

### BrickElement

A `BrickElement` is an addressable structural artifact in the .NET system.

Possible kinds:

- assembly
- namespace
- type
- member
- attribute
- DI registration
- generated artifact
- external reference
- visibility relation
- runtime-access artifact

Concept sketch:

```csharp
public sealed class BrickElement
{
    public BrickElementId Id { get; init; }
    public BrickElementKind Kind { get; init; }
    public string DisplayName { get; init; }
    public string? AssemblyName { get; init; }
    public string? NamespaceName { get; init; }
    public string? FullName { get; init; }
    public BrickElementOrigin Origin { get; init; }
    public BrickElementSource Source { get; init; }
    public BrickCompilationContext CompilationContext { get; init; }
}
```

### Typed Identifiers

Strings should not be the dominant identity mechanism in the core model.

Concept sketch:

```csharp
public readonly record struct BrickRoleId(string Value);
public readonly record struct BrickDimensionId(string Value);
public readonly record struct BrickPolicyId(string Value);
public readonly record struct BrickDependencyKindId(string Value);
public readonly record struct BrickElementId(string Value);
```

Textual names are useful for configuration and reports, but core evaluation
should use typed identifiers wherever possible.

### BrickRole

A `BrickRole` is the semantic meaning assigned to one or more elements.

Examples:

- `Business.Sales`
- `Business.Support`
- `Contracts`
- `Shared`
- `Infrastructure`
- `Generated`
- `TestOnly`
- `Legacy`
- `DDD.Entity`
- `Architecture.Layer.Domain`

Concept sketch:

```csharp
public sealed class BrickRole
{
    public BrickRoleId Id { get; init; }
    public BrickDimensionId DimensionId { get; init; }
    public string DisplayName { get; init; }
    public string? Category { get; init; }
    public string? Description { get; init; }
    public bool IsBuiltin { get; init; }
}
```

### BrickRoleDimension

A `BrickRoleDimension` defines one orthogonal semantic axis.

Examples:

- `BusinessPartition`
- `TechnicalNature`
- `Lifecycle`
- `ArchitectureStyle`
- `DDD`
- `Eventing`
- `Exposure`

Concept sketch:

```csharp
public sealed class BrickRoleDimension
{
    public BrickDimensionId Id { get; init; }
    public string DisplayName { get; init; }
    public bool AllowsMultipleRoles { get; init; }
    public bool IsExclusiveByDefault { get; init; }
    public string? Description { get; init; }
}
```

### BrickRoleAssignment

A `BrickRoleAssignment` applies a role to an element through a concrete source.

Possible assignment modes:

- direct attribute
- external configuration
- convention
- inference
- alias
- imported specialised-pack mapping
- generated assignment

Concept sketch:

```csharp
public sealed class BrickRoleAssignment
{
    public BrickElementSelector Selector { get; init; }
    public BrickRoleId RoleId { get; init; }
    public BrickAssignmentMode Mode { get; init; }
    public BrickAssignmentSource Source { get; init; }
    public BrickAssignmentPrecedence Precedence { get; init; }
    public BrickAssignmentBehavior Behavior { get; init; }
}
```

`Precedence` records both structural specificity and declaration authority so
comparisons stay deterministic and explainable.

```csharp
public readonly record struct BrickAssignmentPrecedence(
    BrickAssignmentSpecificity Specificity,
    BrickAssignmentAuthority Authority);

public enum BrickAssignmentSpecificity
{
    Inference = 0,
    Convention = 1,
    Assembly = 2,
    Namespace = 3,
    Element = 4
}

public enum BrickAssignmentAuthority
{
    Derived = 0,
    Alias = 1,
    External = 2,
    Direct = 3
}
```

Assignments are compared lexicographically:

1. higher `Specificity` wins
2. if specificity is equal, higher `Authority` wins
3. if both are equal and the matched roles are exclusive or incompatible, the
   conflict remains explicit and no silent winner is chosen

### BrickAlias

A `BrickAlias` maps an existing symbol, annotation, or pattern to a canonical
role.

When `AppliesTo` is not set, the alias is treated as global and applies to all
matching elements regardless of their containing scope.

Concept sketch:

```csharp
public sealed class BrickAlias
{
    public string AliasName { get; init; }
    public BrickRoleId CanonicalRoleId { get; init; }
    public BrickElementSelector? AppliesTo { get; init; } // null = global
    public string? Reason { get; init; }
}
```

### BrickDependency

A `BrickDependency` models a concrete relationship between two elements.

Possible dependency kinds:

- type reference
- method call
- inheritance
- interface implementation
- object creation
- attribute usage
- generic constraint
- DI registration
- reflection access
- friend-assembly visibility
- public API exposure

Dependency detection is not limited to top-level type declarations. Type usage
must be detected anywhere it creates a structural relationship, including:

- field declarations and field initializers
- property types and property accessors
- method signatures and method bodies
- local functions and nested code blocks
- constructors
- destructors
- operators and conversions
- object creation expressions
- inheritance and interface implementation

Concept sketch:

```csharp
public sealed class BrickDependency
{
    public BrickElement Source { get; init; }
    public BrickElement Target { get; init; }
    public BrickDependencyKindId KindId { get; init; }
    public BrickScope Scope { get; init; }
    public BrickDependencyLayer Layer { get; init; }
    public BrickDependencyStrength Strength { get; init; }
    public BrickEvidenceLevel EvidenceLevel { get; init; }
    public BrickDependencyEvidence Evidence { get; init; }
    public BrickSourceLocation? Location { get; init; }
    public string? Detail { get; init; }
}
```

### BrickEvidenceLevel

Not all dependencies are equally certain.

Concept sketch:

```csharp
public enum BrickEvidenceLevel
{
    CompilerConfirmed,
    AnalyzerInferred,
    ConfigurationDeclared,
    RuntimeInferred,
    Unknown
}
```

Examples:

- type reference: `CompilerConfirmed`
- method call: `CompilerConfirmed`
- DI registration: `AnalyzerInferred`
- external policy declaration: `ConfigurationDeclared`
- reflection access: `RuntimeInferred`
- unresolved dynamic access: `Unknown`

Policy decisions may treat low-confidence findings differently.

### BrickRule

A `BrickRule` evaluates a role relationship or dependency expectation.

Rules carry either a permission decision (`Allow` or `Deny`) or a requirement
decision (`Require`). These decision kinds are evaluated through separate engine
paths.

Concept sketch:

```csharp
public sealed class BrickRule
{
    public RuleId RuleId { get; init; }
    public string Name { get; init; }
    public BrickRoleSelector SourceRoles { get; init; }
    public BrickRoleSelector TargetRoles { get; init; }
    public BrickDependencySelector Dependencies { get; init; }
    public BrickScope Scope { get; init; }
    public BrickDecision Decision { get; init; }
    public BrickRuleExceptionSet Exceptions { get; init; }
    public BrickSeverity Severity { get; init; }
    public int Priority { get; init; }
    public string? Reason { get; init; }
}
```

### BrickPolicy

A `BrickPolicy` groups rules into a coherent enforcement unit.

Concept sketch:

```csharp
public sealed class BrickPolicy
{
    public BrickPolicyId Id { get; init; }
    public string Name { get; init; }
    public IReadOnlyList<BrickPolicyImport> Imports { get; init; }
    public IReadOnlyList<BrickRule> Rules { get; init; }
    public IReadOnlyList<BrickRoleCombinationRule> CombinationRules { get; init; }
    public BrickPermissionDefault DefaultDecision { get; init; }
    public BrickEnforcementMode Enforcement { get; init; }
}
```

`DefaultDecision` applies only to unmatched permission evaluation. `Require`
rules have no fallback default.

### BrickPolicyImport

Policy composition must be explicit.

Concept sketch:

```csharp
public sealed class BrickPolicyImport
{
    public BrickPolicyId ImportedPolicyId { get; init; }
    public BrickPolicyImportMode Mode { get; init; }
}
```

Possible import modes:

- `Import`
- `Extend`
- `Override`
- `Disable`
- `Narrow`

Policies must not be implicitly merged.

### BrickViolation

A `BrickViolation` is the normalized output of rule evaluation.

Not every violation is tied to a concrete dependency. Combination and
resolution violations may have no target element or dependency kind.

Concept sketch:

```csharp
public sealed class BrickViolation
{
    public BrickViolationKind Kind { get; init; }
    public RuleId? RuleId { get; init; }
    public string? RuleName { get; init; }
    public BrickElement Source { get; init; }
    public BrickElement? Target { get; init; }
    public IReadOnlyList<BrickRoleId> ResolvedSourceRoles { get; init; }
    public IReadOnlyList<BrickRoleId> ResolvedTargetRoles { get; init; }
    public BrickDependencyKindId? DependencyKindId { get; init; }
    public BrickScope Scope { get; init; }
    public BrickDependencyLayer? DependencyLayer { get; init; }
    public BrickSeverity Severity { get; init; }
    public string Message { get; init; }
    public BrickEvidence Evidence { get; init; }
    public BrickViolationState State { get; init; }
    public string? StateReason { get; init; }
}

public enum BrickViolationKind
{
    Dependency,
    Requirement,
    RoleCombination,
    RoleResolution,
    Configuration,
    Baseline,
    Suppression
}
```

### BrickViolationState

Baseline and suppression are not the same.

Concept sketch:

```csharp
public enum BrickViolationState
{
    Active,
    Suppressed,
    Baselined,
    ExpiredSuppression,
    ExpiredBaseline
}
```

### BrickResolvedRoles

Role resolution should produce an explicit result, not just an internal
temporary set.

Concept sketch:

```csharp
public sealed class BrickResolvedRoles
{
    public BrickElement Element { get; init; }
    public IReadOnlyList<BrickRoleAssignment> CandidateAssignments { get; init; }
    public IReadOnlyList<BrickRoleId> EffectiveRoles { get; init; }
    public IReadOnlyList<BrickRoleAssignment> AppliedAssignments { get; init; }
    public IReadOnlyList<BrickRoleAssignment> SuppressedAssignments { get; init; }
    public IReadOnlyList<BrickRoleConflict> Conflicts { get; init; }
}
```

### BrickResolutionTrace

A `BrickResolutionTrace` records how roles were resolved for an element.

Concept sketch:

```csharp
public sealed class BrickResolutionTrace
{
    public BrickElement Element { get; init; }
    public IReadOnlyList<BrickRoleAssignment> Candidates { get; init; }
    public IReadOnlyList<BrickRoleId> ResolvedRoles { get; init; }
    public IReadOnlyList<string> Decisions { get; init; }
    public bool HasConflict { get; init; }
}
```

### BrickEvidence

A `BrickEvidence` captures the explanation material behind a violation.

Concept sketch:

```csharp
public sealed class BrickEvidence
{
    public BrickEvidenceLevel Level { get; init; }
    public string? PrimaryLocation { get; init; }
    public IReadOnlyList<string> SecondaryLocations { get; init; }
    public IReadOnlyList<string> MatchedRules { get; init; }
    public IReadOnlyList<string> ResolutionFacts { get; init; }
}
```

### BrickBaselineEntry

A `BrickBaselineEntry` records a known accepted existing violation.

Concept sketch:

```csharp
public sealed class BrickBaselineEntry
{
    public RuleId RuleId { get; init; }
    public string SourcePattern { get; init; }
    public string TargetPattern { get; init; }
    public string? Justification { get; init; }
    public string? Owner { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}
```

### BrickSuppression

A `BrickSuppression` records an intentional local exception.

Concept sketch:

```csharp
public sealed class BrickSuppression
{
    public RuleId RuleId { get; init; }
    public BrickElementSelector Selector { get; init; }
    public string Justification { get; init; }
    public string? Owner { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}
```

## Scope Model

`BrickScope` defines the structural granularity at which a rule is evaluated.
It constrains the set of source and target evaluation units that participate in
rule evaluation.

Scope is orthogonal to role assignment. Role assignments at broader scopes
(for example assembly) apply to contained elements unless a more specific
assignment overrides or suppresses them. A rule evaluated at `Type` scope may
therefore still use roles declared at `Assembly` scope.

Concept sketch:

```csharp
public enum BrickScope
{
    Global,
    Assembly,
    Namespace,
    Type,
    Member
}
```

Evaluation units by scope:

- `Global`: one unit representing the entire evaluated system
- `Assembly`: each source assembly
- `Namespace`: each source namespace
- `Type`: each source type
- `Member`: each source member

This enum is a required field on `BrickRule` and appears on `BrickViolation`
to document at which granularity the violation was detected.

## Role System

The role model should support these invariants.

### Roles Are Multi-Valued Across Dimensions

An element may carry more than one role when those roles belong to compatible
dimensions.

Examples:

- `Business.Sales` plus `Contracts`
- `TestOnly` plus `FriendConsumer`
- `DDD.Repository` plus `Architecture.Layer.Infrastructure`

### Roles Are Usually Exclusive Within One Dimension

Some role dimensions are exclusive by default.

Examples:

- `Business.Sales` versus `Business.Support`
- `Architecture.Layer.Domain` versus `Architecture.Layer.Infrastructure`

This exclusivity should be explicit through role dimensions and combination
rules, not hard-coded through naming conventions.

### Duplicate Role Instances Must Not Accumulate

The same effective role must not appear more than once on the same element just
because multiple assignment paths resolved to the same role name.

Examples:

- direct `Contracts` plus external `Contracts` -> one effective `Contracts`
- namespace `Shared` plus assembly `Shared` -> one effective `Shared`

An exception is allowed when the role model treats the assignments as distinct
parameterized role instances rather than the same plain role.

Conceptually, role identity is:

```text
RoleName + optional parameter identity
```

### Roles Can Be Hierarchical

Role families should support hierarchical naming and grouping.

Examples:

- `Business`
- `Business.Sales`
- `Business.Support`
- `Business.Billing`

### Roles Can Be Categorized

Roles may belong to categories such as:

- business
- boundary
- technical
- lifecycle
- architecture
- eventing
- quality

### Role Combinations Must Be Validatable

Not every multi-role assignment is meaningful. The model defines explicit
combination rules that govern whether roles may coexist on the same element.

Three combination classes are supported:

| Kind | Meaning | Example |
|---|---|---|
| `Additive` | Both role sets may coexist; combination is valid | `Contracts` + `Shared` |
| `Exclusive` | Only one active role from the matched sets may remain on the element | `Business.Sales` + `Business.Support` |
| `Incompatible` | The combination is structurally invalid regardless of context | `Generated` + `Business.Sales` |

Concept sketch:

```csharp
public sealed class BrickRoleCombinationRule
{
    public string Name { get; init; }
    public BrickRoleSelector LeftRoles { get; init; }
    public BrickRoleSelector RightRoles { get; init; }
    public BrickCombinationKind Kind { get; init; }
    public string? Reason { get; init; }
}

public enum BrickCombinationKind
{
    Additive,
    Exclusive,
    Incompatible
}
```

Combination rules participate in role resolution and also define which
post-resolution violations may be emitted.

### Dimensions Must Be First-Class

A pure multi-role model without dimensions is not precise enough for reliable
resolution and validation. Bricks therefore treats dimensions as an explicit
part of the model.

## Role Resolution

Role resolution is a formal part of the concept, not an incidental detail.

### Role Sources

Roles may come from:

1. direct attribute assignment
2. explicit alias mapping
3. external configuration
4. namespace-based assignment
5. assembly-based assignment
6. convention
7. inference
8. imported mappings from specialised packs
9. generated assignments

### Recommended Precedence

Recommended effective precedence from strongest to weakest:

1. direct element assignment
2. explicit external element assignment
3. explicit alias on a concrete element
4. namespace-based assignment
5. assembly-based assignment
6. convention
7. inference

Imported specialised-pack mappings should behave as direct or alias-level
assignments depending on their semantic certainty.

### Resolution Is Per Dimension

Resolution should occur independently per role dimension. The final resolved
role set is the union of the dimension results.

### Specificity Wins

More specific assignments should override broader ones where the policy or
dimension demands it.

Example:

- assembly role = `Business.Sales`
- namespace role = `Contracts`
- types inside the contracts namespace should resolve as `Contracts`, not just
  the broad assembly role

### Additive And Replacing Assignments

Role resolution is not a blanket merge.

- assignments may accumulate when their combination is `Additive`
- a stronger assignment may suppress a weaker one when their combination is
  `Exclusive`
- `Incompatible` matches are never silently merged into one effective role
- suppressed assignments must remain inspectable in the resolution result

### Conflicts Must Be Visible

Conflicting assignments should surface explicitly instead of being silently
merged.

Typical examples:

- two assignments in the same exclusive role family
- a direct role that conflicts with an imported external policy
- a namespace assignment that narrows an assembly assignment but leaves an
  incompatible lifecycle role in place

### Unknown Roles Must Not Slip Through Silently

Assignments that reference unknown or non-normalizable roles should produce a
visible configuration or model error.

## Rule Model

Each rule should evaluate:

- source roles
- target roles
- dependency kind
- scope
- dependency layer
- evidence level
- exceptions
- decision
- severity
- priority

### Decisions

Rule decisions express structural intent, not diagnostic presentation.

Concept sketch:

```csharp
public enum BrickDecision
{
    Allow,
    Deny,
    Require
}
```

`Allow` and `Deny` operate on a concrete dependency instance.

`Require` is quantified differently: for each source evaluation unit selected
by the rule scope whose effective roles match `SourceRoles`, there must exist at
least one dependency of the selected kind to at least one target evaluation unit
within that scope whose effective roles match `TargetRoles`.

Examples:

- at `Type` scope: every matching source type must depend on at least one
  matching target type
- at `Namespace` scope: every matching source namespace must contain at least
  one qualifying dependency to a matching target namespace
- at `Assembly` scope: every matching source assembly must contain at least one
  qualifying dependency to a matching target assembly
- at `Global` scope: the evaluated system must contain at least one qualifying
  dependency across all matching elements

`Warn` and `Error` belong to severity. `Ignore` should be represented either by
explicit exceptions, disabled rules, suppressions, baselines, or by the policy
default for unmatched cases, not as a third semantic axis mixed into rule
intent.

### Default Policy Decision Must Be Explicit

A policy should define its default permission decision explicitly.

Recommended modes:

- strict profiles: default `Deny`
- permissive profiles: default `Allow`
- documentation-only profiles: default `Allow` with diagnostics disabled or
  informational

The default applies only to unmatched permission evaluation. It does not apply
to requirement evaluation.

### Rule Precedence Must Be Deterministic

When multiple rules match, the implementation must choose the governing outcome
deterministically.

Recommended precedence:

1. higher selector specificity
2. higher explicit rule priority
3. stronger decision precedence
4. stable policy order
5. stable declaration order

If two equally strong applicable permission rules still conflict after
specificity and priority, `Deny` should win over `Allow`.

### Exceptions Are Part Of Applicability

Exceptions are not general tie-breakers. They are part of whether a rule applies
to a case at all.

### Evaluation Pipeline

The evaluation pipeline runs in this order:

```text
1. Candidate assignment collection
2. Combination-aware role resolution
3. Resolution and role-combination violation emission
4. Permission evaluation
5. Requirement evaluation
6. Baseline and suppression state projection
7. Reporting and export projection
```

Combination rules influence step 2 and may also produce violations in step 3.
They are therefore both resolution inputs and reportable outcomes.

### Canonical Rule Families

The baseline rule families should include:

- role isolation
- role direction
- contract-only access
- ownership rules
- visibility control
- runtime-wiring control
- explicit exception rules
- role-combination rules
- friend-assembly rules
- requirement rules

## Matrix Model

The conceptual evaluation core should be matrix-based.

### Primary Matrix

The long-term primary matrix is:

```text
Role x Role x DependencyKind x Scope x DependencyLayer x EvidenceLevel
```

It answers:

> Does role A permit, deny, or require a dependency of kind X at scope S on
> layer L with evidence level E to role B?

For V1, `Scope` and static dependency kinds are mandatory. Dependency layers and
evidence levels are staged extensions.

### Permission Evaluation

Permission rules should follow a stable aggregation strategy:

- evaluate the full cross-product of effective source roles and effective
  target roles
- remove matches excluded by explicit rule exceptions
- if any remaining match yields `Deny`, the dependency violates the policy
- otherwise, if at least one remaining match yields `Allow`, the dependency is
  allowed
- otherwise, the policy default applies and that default must be explicit

This makes multi-role evaluation deterministic and inspectable.

### Requirement Evaluation

`Require` rules are evaluated separately from permission checks.

- they operate on the same resolved role sets and scope model
- they are universal over source evaluation units and existential over matching
  targets
- they assert that each matching source evaluation unit has at least one
  dependency matching the rule selector within the evaluated scope
- missing required dependencies produce violations even when no forbidden
  dependency exists
- the policy `DefaultDecision` does not apply to requirement evaluation

### Additional Matrices

The concept should also leave room for:

- role-combination matrix
- dimension-exclusivity matrix
- visibility matrix
- exception matrix
- severity matrix
- enforcement matrix
- suppression matrix
- baseline matrix
- evidence confidence matrix

## Configuration Model

Bricks may be configured through multiple sources.

Possible configuration sources:

- source-level attributes
- `.editorconfig`
- MSBuild properties and items
- JSON or YAML policy files
- package-provided packs
- package-provided profiles
- generated configuration

Recommended configuration precedence:

1. explicit source-level annotation
2. project policy file
3. MSBuild configuration
4. `.editorconfig`
5. package defaults
6. inference

Configuration precedence must be deterministic.

## Policy File Direction

A future policy file may look like this:

```json
{
  "schema": "NMolecules.Bricks.Policy/1.0",
  "id": "DefaultArchitecturePolicy",
  "name": "Default Architecture Policy",
  "defaultDecision": "Deny",
  "roles": [
    {
      "id": "Business.Sales",
      "dimension": "BusinessPartition"
    },
    {
      "id": "Contracts",
      "dimension": "TechnicalNature"
    }
  ],
  "rules": [
    {
      "id": "XMoleculesBricks0001",
      "source": "Business.*",
      "target": "Contracts",
      "dependencyKinds": ["TypeReference", "MethodCall"],
      "scope": "Type",
      "dependencyLayers": ["Static"],
      "minEvidenceLevel": "CompilerConfirmed",
      "decision": "Allow",
      "severity": "Info"
    }
  ]
}
```

The exact format is not a V1 requirement, but schema versioning should be
planned from the start.

## Policy Composition

Policy composition must be explicit.

Supported conceptual modes:

- `Import`: use another policy as-is
- `Extend`: add additional rules
- `Override`: replace selected rules
- `Disable`: turn selected rules off
- `Narrow`: make selected rules stricter

Bricks must not silently merge multiple policies.

If multiple policies apply to the same scope, the composition order must be
visible and deterministic.

## Built-In Role Packs

Bricks should remain generic at the core, but practical reuse benefits from
formal built-in packs and profiles.

### Structural Core Pack

- `Business.*`
- `Contracts`
- `Shared`
- `Infrastructure`
- `Platform`
- `Legacy`
- `Generated`
- `TestOnly`
- `FriendConsumer`

### DDD Pack

- `DDD.Entity`
- `DDD.ValueObject`
- `DDD.AggregateRoot`
- `DDD.Repository`
- `DDD.Factory`
- `DDD.Service`
- `DDD.Identity`
- `DDD.BoundedContext`
- `DDD.Module`

### Events Pack

- `Events.DomainEvent`
- `Events.DomainEventHandler`
- `Events.DomainEventPublisher`

### Architecture Pack

- `Architecture.Layer.Domain`
- `Architecture.Layer.Application`
- `Architecture.Layer.Infrastructure`
- `Architecture.Layer.Interface`

### Advanced Packs

Advanced packs can later cover:

- Onion
- Hexagonal
- CQRS
- Saga
- ReadModel
- Adapter
- Port
- AntiCorruptionLayer

## Compatibility With Existing nMolecules Packages

Bricks should complement specialised packages and normalize their semantics
into a common role model.

Examples of conceptual normalization:

- `[Entity]` -> `DDD.Entity`
- `[AggregateRoot]` -> `DDD.AggregateRoot`
- `[ValueObject]` -> `DDD.ValueObject`
- `[Repository]` -> `DDD.Repository`
- `[DomainEvent]` -> `Events.DomainEvent`
- `[DomainLayer]` -> `Architecture.Layer.Domain`
- `[ApplicationLayer]` -> `Architecture.Layer.Application`
- `[InfrastructureLayer]` -> `Architecture.Layer.Infrastructure`
- `[InterfaceLayer]` -> `Architecture.Layer.Interface`

This allows specialised packages to remain independent while still
contributing to one common structural evaluation model.

## Assignment Providers

Indirect role assignment should be a formal extension mechanism.

Concept sketch:

```csharp
public interface IBrickRoleAssignmentProvider
{
    IEnumerable<BrickRoleAssignment> GetAssignments(BrickModelContext context);
}
```

Possible provider types:

- attribute provider
- alias provider
- convention provider
- external JSON or YAML provider
- MSBuild-backed provider
- specialised pack provider
- source-generator provider

Provider composition must be deterministic. Provider ordering must be explicit
or derived from stable precedence rules.

## .NET-Specific Model Requirements

### Assembly Is A Primary Scope

Assemblies are not just packaging artifacts. They are structural, visibility,
and versioning boundaries.

### Roslyn Symbol Identity

Bricks should prefer Roslyn symbol identity over textual name matching wherever
possible.

Textual names are suitable for reports and configuration, but analyzer
decisions should be based on stable compiler symbols.

### Multi-Targeting

A project may produce different structural models for different target
frameworks.

Bricks should treat target framework context as part of the analysis scope
where multi-targeting changes dependencies, visibility, or generated code.

### Conditional Compilation

Conditional compilation may alter the effective structural model.

Analyzer results should be tied to the compilation context in which they were
produced.

### `InternalsVisibleTo`

Friend-assembly exposure should be treated as an explicit structural opening,
not hidden infrastructure noise.

### Dependency Injection

DI registration represents a real structural relationship and deserves its own
dependency kind in a stronger future model.

For V1, only statically reliable dependency extraction should be mandatory.

### Generated Code

Generated code needs explicit origin handling and often a dedicated role.

### Partial Types

Evaluation must stay symbol-based, not file-based.

### Reflection

Reflection can bypass structural intent and should be modelled explicitly when
Bricks grows beyond the current baseline.

Reflection findings should carry lower confidence unless compiler or
configuration evidence is strong.

### Source Generators

Generated source should be attributable to its generator origin where possible.

### Tests

Tests need formal relaxation options such as `TestOnly`, `FriendConsumer`, and
optional friend-consumption rules.

## Violation, Baseline, And Suppression Model

Strict structural adoption requires more than immediate hard-fail diagnostics.

### Violations Must Be Reusable Outside Analyzers

A `BrickViolation` should be portable enough for:

- analyzer output
- architecture tests
- JSON export
- report generation
- IDE explanations

### Baselines Support Incremental Adoption

A baseline entry records a known accepted current violation.

Purpose:

- enable migration
- avoid blocking adoption
- keep existing debt visible

Baseline does not mean the violation is architecturally correct.

### Suppressions Are Intentional Exceptions

A suppression records a deliberate exception.

Suppressions should be:

- explicit
- attributable
- explainable
- optionally time-bounded

### Baseline And Suppression Must Stay Separate

Baseline and suppression are different concepts.

```text
Baseline
  Existing known violation.
  Goal: migration support.

Suppression
  Intentional exception.
  Goal: controlled local deviation.
```

Reports should distinguish:

- active violations
- suppressed violations
- baselined violations
- expired suppressions
- expired baseline entries

### Suppression Must Not Destroy Truth

Suppression changes output treatment, not the underlying structural reality.
Internally, the model should still be able to retain the original violation.

## Diagnostic ID Governance

Diagnostic IDs must follow a stable governance model.

Recommended ranges:

```text
XMoleculesBricks0001-0099  Role and dependency rules
XMoleculesBricks0100-0199  Role resolution conflicts
XMoleculesBricks0200-0299  Policy configuration errors
XMoleculesBricks0300-0399  Member cardinality contracts
XMoleculesBricks0400-0499  Suppression and baseline issues
XMoleculesBricks0500-0599  Export and reporting issues
XMoleculesBricks0600-0699  Pack and profile bridge issues
XMoleculesBricks0700-0799  Runtime and wiring analysis issues
```

Existing IDs should remain stable.

## Enforcement Model

Bricks should work across multiple enforcement levels.

### Analyzer

Primary enforcement path:

- IDE diagnostics
- build diagnostics
- deterministic structural feedback

### Testing

Optional architecture-test API for rule verification outside the compiler
pipeline.

### Reporting

Exports for:

- role maps
- dependency graphs
- rule matrices
- resolution traces
- violation reports
- suppression and baseline reports

### Documentation

Future export targets may include:

- Mermaid
- PlantUML
- JSON
- HTML reports
- matrix reports

### Augmentation And Integration

Future expansion may include technical integrations or generated conventions
based on resolved role models, but this is beyond the V1 baseline.

## Conformance Levels

Bricks should define progressive conformance levels.

### Level 0: Marking

Roles can be expressed in code.

Includes:

- role attributes
- alias attributes
- typed identifiers

### Level 1: Static Validation

Static compiler-visible dependencies are analysed.

Includes:

- type references
- inheritance
- interface implementation
- object creation
- basic rule validation
- analyzer diagnostics

### Level 2: Explainability

Resolution and violations become explainable.

Includes:

- resolution traces
- evidence
- normalized violations
- structured diagnostic messages

### Level 3: Policy Files

External policy configuration becomes supported.

Includes:

- schema-versioned policy files
- baseline files
- suppression files
- policy imports

### Level 4: Runtime-Aware Analysis

Runtime-relevant dependencies become analysable.

Includes:

- DI registration
- `InternalsVisibleTo`
- reflection
- runtime activation
- evidence confidence levels

### Level 5: Integration And Augmentation

Bricks can support richer integrations.

Includes:

- IDE visualisation
- report generation
- generated architecture documentation
- possible technical integrations

## Packaging Direction

This is a conceptual target shape, not the current package split.

Possible long-term slices:

```text
NMolecules.Bricks.Abstractions
NMolecules.Bricks.Roles
NMolecules.Bricks.Policies
NMolecules.Bricks.Analyzers
NMolecules.Bricks.Runtime
NMolecules.Bricks.Testing
NMolecules.Bricks.Export
NMolecules.Bricks.Integrations
NMolecules.Bricks.Packs.DDD
NMolecules.Bricks.Packs.Events
NMolecules.Bricks.Packs.Architecture
```

Today, these concerns can remain more compact. The package split should grow
only when the API pressure justifies it.

## V1 Boundary

The effective V1 baseline should be deliberately smaller than the full concept.

V1 should cover:

- direct role assignment
- alias-based role mapping
- typed role and rule identifiers
- minimal role dimensions
- deterministic role resolution
- basic resolution conflict diagnostics
- deterministic rule evaluation
- explicit policy defaults
- static role-based dependency validation
- `Allow`, `Deny`, and `Require` decision semantics
- `Scope` support for at least type-level evaluation
- member-cardinality contract validation
- analyzer diagnostics
- minimal suppression support

V1 should not require:

- full external policy files
- DI registration analysis
- reflection analysis
- runtime wiring analysis
- rich export reporting
- broad IDE visualisation
- complete pack bridge ecosystem

## V1.1 Boundary

V1.1 should cover:

- external policy file prototype
- schema versioning for policy files
- resolution trace export
- baseline file support
- first report output
- stricter diagnostic governance
- basic pack bridge for DDD

## V1.2 Boundary

V1.2 should cover:

- architecture testing API
- JSON report export
- role map export
- dependency graph export
- suppression and baseline reports
- additional pack bridges for Events and Architecture

## V2 Boundary

V2 should cover:

- DI registration dependency analysis
- `InternalsVisibleTo` analysis
- visibility dependency layer
- runtime and wiring dependency layer
- reflection modelling with confidence levels
- advanced policy composition
- profile packages for Layered, Onion, Hexagonal, and CQRS
- richer IDE support

## Operational Open Points

The conceptual core is intentionally stricter than the current implementation.
Some important open points now sit less in the semantic model itself and more
in operability, adoption, and governance.

These are the main remaining non-model gaps:

- performance budgets for IDE and build analysis
- rollout strategy for legacy systems, including baseline and net-new gating
- coverage transparency for dependency kinds that are only partially observable
  in V1, especially DI registration and reflection
- versioned export contracts for reports and machine-to-machine consumption
  such as JSON or SARIF
- governance for policy ownership, exception handling, role-pack evolution, and
  compatibility expectations

These concerns do not weaken the conceptual model, but they do determine how
effectively Bricks can be adopted as a real enforcement system.

## Open Design Questions

The following questions remain architecturally relevant:

- should dimensions be exclusive by default or only when configured
- how far should inference be allowed to go before it becomes too implicit
- how should weak runtime evidence affect severity and decision behaviour
- how should overlapping assignment providers be composed
- should policy composition support inheritance in addition to imports
- where should ownership for pack bridges live: in Bricks or in specialised
  packs
- which conceptual types should become stable public API and which should
  remain analyzer-internal
- which result contracts should be versioned first: violation reports, role
  maps, dependency graphs, or resolution traces

## One-Sentence Summary

`NMolecules.Bricks` models .NET artifacts as structural elements, resolves
semantic roles for them from direct and indirect sources, and evaluates their
relationships through deterministic policies across static, scoped, visibility,
and runtime-relevant boundaries so architectural limits become explicit,
traceable, analyzable, documentable, and incrementally enforceable.

## Version 2.2 Merge Result

Version 2.2 keeps the current repository baseline's stronger semantic engine:

- scope-aware rule evaluation
- `Allow`, `Deny`, and `Require` as structural decisions
- separate permission and requirement evaluation
- explicit role-combination classes
- structured assignment precedence
- explicit violation kinds
- combination-aware resolution output

Version 2.2 adopts v2.1's stronger governance layer:

- Core, Pack, Profile, and Policy separation
- Concept, Analysis, and Result model layering
- public API boundary
- configuration model
- policy composition
- evidence levels
- baselines and suppressions
- diagnostic ID governance
- conformance levels
- staged V1/V1.1/V1.2/V2 roadmap

The next useful document is a V1 API surface specification with concrete
namespaces, attributes, enums, diagnostics, and analyzer contracts.
