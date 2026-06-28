# Bricks Foundational Concept

Status baseline: March 7, 2026
Revision: March 14, 2026 - incorporated review changes (scope definition,
require semantics, role-combination validation, specificity model)

This document defines the foundational concept for `NMolecules.Bricks`.
It is intentionally stricter than a marketing description and broader than the
currently shipped API surface.

The goal is to provide one stable conceptual reference for future API design,
analyzer growth, testing, reporting, and IDE integrations.

## Purpose

`NMolecules.Bricks` is a structural role-and-rule framework for .NET codebases.

Its core job is to:

- classify code elements semantically
- assign roles explicitly
- define structural rules between those roles
- validate relationships deterministically
- surface violations consistently in analyzers, builds, tests, and reports

The framework exists to reduce three recurring problems:

1. implicit structure that only exists in team knowledge
2. dependency erosion across intended boundaries
3. poor semantic classification of legacy, external, generated, or test-only code

## Core Principle

The foundational principle is:

> Elements carry roles.  
> Rules evaluate relationships between roles.  
> Roles can be assigned directly or mapped indirectly.

This makes Bricks broader than a pure attribute library, while still allowing
attributes to remain the primary current implementation path.

## Design Tenets

### Semantics Before Syntax

Namespaces, folders, and file names may help with discovery, but they are not
the authoritative semantic model. Roles are.

### Existing Code Must Be Addressable

Bricks should not require a full refactor before a system becomes classifiable.
The model therefore needs room for external or indirect role assignment.

### .NET Reality Is Part Of The Model

Assemblies, `internal`, `InternalsVisibleTo`, DI registrations, generated code,
partial types, and reflection are not edge cases. They influence structural
truth and belong in the concept model.

### Rules Must Be Deterministic

The same code and the same policy input must always yield the same structural
classification and the same set of violations.

### Roles Are First-Class

Concepts such as business partitions, contracts, shared code, generated code,
test-only code, or legacy areas should be modeled as roles or role families, not
as ad hoc special cases.

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
  - `XMoleculesBricks0001` for dependency-rule violations
  - `XMoleculesBricks0002` for invalid Bricks configuration metadata
- analyzer-backed dependency evidence from:
  - field, property, method return and parameter types
  - local declarations and object creation in member bodies
  - direct `RoleAttribute` and `RoleAliasAttribute` role assignment
- analyzer-backed member-cardinality contracts:
  - `RequireExactlyOneMemberAttribute`
  - `RequireAllMembersAttribute`
  - `RequireMemberCountAttribute`
  - `RequireExclusiveChoiceAttribute`
  - direct use of these contract attributes
  - custom attributes annotated with these contract attributes
  - `XMoleculesBricks0003` to `XMoleculesBricks0010`

That means Bricks is already more than a marker package, but still narrower than
the full meta-model described below.

## Current Gaps

The following parts are conceptually important but not yet modeled as first-class
runtime or analyzer abstractions:

- explicit element records such as `BrickElement`
- external role-assignment sources beyond attribute and alias evaluation
- formal role-resolution results with visible suppression and conflicts
- generalized dependency kinds such as DI registration or reflection access
- explicit policy bundles and matrix models
- standardized violation records reusable outside the analyzer pipeline
- built-in role packs as a formal packaged concept
- export and reporting surfaces

These are not contradictions. They are the expected growth areas from the
current Bricks baseline toward a stronger structural platform.

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

Note: DI registrations are often not visible at compile time. Their role as
structural elements is conceptually important but may require runtime or
configuration-based resolution. This is an expected gap for V1.

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
}
```

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

Concept sketch:

```csharp
public sealed class BrickRole
{
    public string Name { get; init; }
    public string? Category { get; init; }
    public string? Description { get; init; }
    public bool IsBuiltin { get; init; }
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

Concept sketch:

```csharp
public sealed class BrickRoleAssignment
{
    public BrickElementSelector Selector { get; init; }
    public string RoleName { get; init; }
    public BrickAssignmentMode Mode { get; init; }
    public BrickAssignmentSource Source { get; init; }
    public BrickAssignmentPrecedence Precedence { get; init; }
}
```

`Precedence` replaces a numeric priority field. It records both structural
specificity and declaration authority so comparisons stay deterministic and
explainable.

### BrickAlias

A `BrickAlias` maps an existing symbol or pattern to a canonical role.

When `AppliesTo` is not set, the alias is treated as global and applies to all
matching elements regardless of their containing scope.

Concept sketch:

```csharp
public sealed class BrickAlias
{
    public string AliasName { get; init; }
    public string CanonicalRoleName { get; init; }
    public BrickElementSelector? AppliesTo { get; init; }  // null = global scope
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

Dependency detection is not limited to top-level type declarations.
Type usage must be detected anywhere it creates a structural relationship,
including:

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
    public BrickDependencyKind Kind { get; init; }
    public BrickDependencyStrength Strength { get; init; }
    public BrickDependencyEvidence Evidence { get; init; }
    public BrickSourceLocation? Location { get; init; }
    public string? Detail { get; init; }
}
```

### BrickRule

A `BrickRule` evaluates a role relationship or dependency expectation.

Rules carry either a permission decision (`Allow` / `Deny`) or a requirement
decision (`Require`). These decision kinds are evaluated through separate engine
paths.

Concept sketch:

```csharp
public sealed class BrickRule
{
    public string Name { get; init; }
    public BrickRoleSelector SourceRoles { get; init; }
    public BrickRoleSelector TargetRoles { get; init; }
    public BrickDependencySelector Dependencies { get; init; }
    public BrickScope Scope { get; init; }
    public BrickDecision Decision { get; init; }
    public BrickRuleExceptionSet Exceptions { get; init; }
    public BrickSeverity Severity { get; init; }
}
```

### BrickPolicy

A `BrickPolicy` groups rules into a coherent enforcement unit.

Concept sketch:

```csharp
public sealed class BrickPolicy
{
    public string Name { get; init; }
    public IReadOnlyList<BrickRule> Rules { get; init; }
    public IReadOnlyList<BrickRoleCombinationRule> CombinationRules { get; init; }
    public BrickPermissionDefault DefaultDecision { get; init; }
    public BrickEnforcementMode Enforcement { get; init; }
}
```

`DefaultDecision` applies only to unmatched permission evaluation.
`Require` rules have no fallback default.

### BrickViolation

A `BrickViolation` is the normalized output of rule evaluation.

Not every violation is tied to a concrete dependency. Combination and
resolution violations may have no target element or dependency kind.

Concept sketch:

```csharp
public sealed class BrickViolation
{
    public BrickViolationKind Kind { get; init; }
    public string? RuleName { get; init; }
    public BrickElement Source { get; init; }
    public BrickElement? Target { get; init; }
    public BrickDependencyKind? DependencyKind { get; init; }
    public BrickScope Scope { get; init; }
    public BrickSeverity Severity { get; init; }
    public string Message { get; init; }
    public IReadOnlyList<string> EffectiveSourceRoles { get; init; }
    public IReadOnlyList<string> EffectiveTargetRoles { get; init; }
    public IReadOnlyList<string> RelatedRoles { get; init; }
    public BrickSourceLocation? Location { get; init; }
    public string? Evidence { get; init; }
}
```

```csharp
public enum BrickViolationKind
{
    Dependency,
    Requirement,
    RoleCombination,
    RoleResolution
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
    Global,     // the full evaluated system or policy input
    Assembly,   // each source assembly is an evaluation unit
    Namespace,  // each source namespace is an evaluation unit
    Type,       // each source type is an evaluation unit
    Member      // each source member is an evaluation unit
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

### Roles Are Multi-Valued

One element may carry multiple roles when the policy allows it.

Examples:

- `Contracts` plus `Shared`
- `TestOnly` plus `FriendConsumer`

### Duplicate Role Instances Must Not Accumulate

The same effective role must not appear more than once on the same element
just because multiple assignment paths resolved to the same role name.

Examples:

- direct `Contracts` plus external `Contracts` -> one effective `Contracts`
- namespace `Shared` plus assembly `Shared` -> one effective `Shared`

An exception is allowed when the role model treats the assignments as distinct
parameterized role instances rather than the same plain role.

Examples:

- `Adapter(Direction=Inbound)`
- `Adapter(Direction=Outbound)`

Conceptually, role identity is therefore:

`RoleName + optional parameter identity`

That means:

- unparameterized duplicates collapse into one effective role
- parameterized instances may coexist if their parameter identity differs
- duplicate elimination happens before rule evaluation

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

Selectors may target exact roles, hierarchical families such as `Business.*`,
or any other selector form supported by the policy model. This avoids exploding
family-wide rules into manual pairwise declarations.

Combination rules participate in role resolution and also define which
post-resolution violations may be emitted. They are not an isolated late-stage
check.

## Role Resolution

Role resolution is a formal part of the concept, not an incidental detail.

### Role Sources

Roles may come from:

1. direct attribute assignment
2. external configuration
3. convention
4. inference
5. alias mapping

### Assignment Precedence

Assignment strength is expressed through an explicit precedence model rather
than a single overloaded integer.

Concept sketch:

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
    Derived = 0,   // inferred or convention-derived
    Alias = 1,     // applied through alias resolution
    External = 2,  // declared in explicit external policy/config
    Direct = 3     // declared directly on the target element
}
```

Assignments are compared lexicographically:

1. higher `Specificity` wins
2. if specificity is equal, higher `Authority` wins
3. if both are equal and the matched roles are `Exclusive` or `Incompatible`,
   the conflict remains explicit and no silent winner is chosen

The assignment source is recorded separately for provenance and diagnostics:

```csharp
public enum BrickAssignmentSource
{
    Attribute,
    ExternalConfig,
    AliasMapping,
    Convention,
    Inference
}
```

### Recommended Precedence

Recommended effective precedence from strongest to weakest:

1. direct element assignment
2. explicit external element assignment
3. explicit alias on a concrete element
4. namespace-based assignment
5. assembly-based assignment
6. convention
7. inference

### Resolution Output

Role resolution should produce an explicit result, not just an internal
temporary set.

Concept sketch:

```csharp
public sealed class BrickResolvedRoles
{
    public BrickElement Element { get; init; }
    public IReadOnlyList<BrickRoleAssignment> CandidateAssignments { get; init; }
    public IReadOnlyList<string> EffectiveRoles { get; init; }
    public IReadOnlyList<BrickRoleAssignment> AppliedAssignments { get; init; }
    public IReadOnlyList<BrickRoleAssignment> SuppressedAssignments { get; init; }
    public IReadOnlyList<BrickRoleConflict> Conflicts { get; init; }
}
```

Role resolution is combination-aware:

- candidate assignments are collected first
- combination rules determine whether matched roles are additive, exclusive, or
  incompatible
- precedence then decides whether a broader role is kept, suppressed, or left in
  conflict
- conflict records remain available even when later evaluation continues

### Specificity Wins

More specific assignments should override broader ones where the policy demands
it.

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

## Rule Model

Each rule should evaluate:

- source role
- target role
- dependency kind
- scope
- exceptions
- decision
- severity

### Decisions

Rule decisions express structural intent, not diagnostic presentation.

Concept sketch:

```csharp
public enum BrickDecision
{
    Allow,    // evaluated per concrete dependency
    Deny,     // evaluated per concrete dependency
    Require   // evaluated per source evaluation unit
}
```

`Allow` and `Deny` operate on a concrete dependency instance.

`Require` is quantified differently: for each source evaluation unit selected by
the rule scope whose effective roles match `SourceRoles`, there must exist at
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

`Warn` and `Error` belong to severity.
`Ignore` should be represented either by explicit exceptions or by the policy
default for unmatched cases, not as a third semantic axis mixed into rule
intent.

### Evaluation Pipeline

The evaluation pipeline runs in this order:

```text
1. Candidate assignment collection
2. Combination-aware role resolution
3. Resolution and role-combination violation emission
4. Permission evaluation
5. Requirement evaluation
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
- explicit exception rules

## Matrix Model

The conceptual evaluation core should be matrix-based.

### Primary Matrix

The primary matrix is:

`Role × Role × DependencyKind × Scope`

It answers:

Does role A permit a dependency of kind X at scope S to role B?

Evaluation starts from resolved effective role sets, not from raw assignments.
For one concrete dependency, the engine evaluates the effective source-role set
against the effective target-role set across the selected dependency kind and
scope.

### Permission Evaluation

Permission rules should follow a stable aggregation strategy:

- evaluate the full cross-product of effective source roles and effective target
  roles
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
- visibility matrix
- exception matrix
- severity matrix
- enforcement matrix

## Built-In Role Packs

Bricks should remain generic at the core, but practical reuse benefits from
formal built-in packs.

### Structural Core Pack

- `Business.*`
- `Contracts`
- `Shared`
- `Infrastructure`
- `Platform`
- `Legacy`
- `Generated`
- `TestOnly`

### DDD Pack

- `Entity`
- `ValueObject`
- `AggregateRoot`
- `Repository`
- `Factory`
- `Service`
- `Identity`
- `BoundedContext`
- `Module`

### Events Pack

- `DomainEvent`
- `DomainEventHandler`
- `DomainEventPublisher`

### Architecture Pack

- `DomainLayer`
- `ApplicationLayer`
- `InfrastructureLayer`
- `InterfaceLayer`

Advanced packs can later cover:

- Onion
- Hexagonal
- CQRS
- Saga
- ReadModel
- Adapter
- Port

## .NET-Specific Model Requirements

### Assembly Is A Primary Scope

Assemblies are not just packaging artifacts. They are structural and visibility
boundaries.

### `InternalsVisibleTo`

Friend-assembly exposure should be treated as an explicit structural opening,
not hidden infrastructure noise.

### Dependency Injection

DI registration represents a real structural relationship and deserves its own
dependency kind in a stronger future model. DI registrations are often not
visible at compile time; their inclusion in the element model may require
runtime or configuration-based resolution. This is an expected gap for V1.

### Generated Code

Generated code needs explicit origin handling and often a dedicated role.

### Partial Types

Evaluation must stay symbol-based, not file-based.

### Reflection

Reflection can bypass structural intent and should be modeled explicitly when
Bricks grows beyond the current baseline.

### Tests

Tests need formal relaxation options such as `TestOnly` and optional friend
consumption rules.

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
- violation reports

### Documentation

Future export targets may include:

- Mermaid
- PlantUML
- JSON
- matrix reports

## Packaging Direction

This is a conceptual target shape, not the current package split.

Possible long-term slices:

- `nMolecules.Bricks.Abstractions`
- `nMolecules.Bricks.Roles`
- `nMolecules.Bricks.Analyzers`
- `nMolecules.Bricks.Runtime`
- `nMolecules.Bricks.Testing`
- `nMolecules.Bricks.Export`
- `nMolecules.Bricks.Integrations`

Today, these concerns are intentionally much more compact.

## V1 Boundary

The effective V1 baseline should cover:

- role assignment
- alias-based role mapping
- deterministic rule evaluation
- role-based dependency validation
- member-cardinality contract validation
- analyzer diagnostics

Near-term expansion areas:

- test API
- friend-assembly rules
- richer dependency kinds
- reporting and export
- external assignment models

## Boundary Statement

Bricks is not:

- only a DDD library
- only a namespace-rule checker
- only a bag of marker attributes

Bricks is:

> a semantic role and rule model for structural code boundaries in .NET

DDD, events, layered architecture, hexagonal architecture, CQRS, and future
role packs should all be understood as domain-specific specializations on top of
that foundation.

## One-Sentence Summary

`NMolecules.Bricks` models .NET artifacts as structural elements, assigns them
semantic roles directly or indirectly, and evaluates their relationships through
declarative rules so architectural boundaries become explicit, analyzable, and
documentable.
