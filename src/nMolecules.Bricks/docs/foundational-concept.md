# Bricks Foundational Concept

Status baseline: March 7, 2026

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
  - `XMoleculesBricks0001`
  - `XMoleculesBricks0002`
- analyzer-backed member-cardinality contracts:
  - `RequireExactlyOneMemberAttribute`
  - `RequireAllMembersAttribute`
  - `RequireMemberCountAttribute`
  - `RequireExclusiveChoiceAttribute`
  - `XMoleculesBricks0003` to `XMoleculesBricks0006`

That means Bricks is already more than a marker package, but still narrower than
the full meta-model described below.

## Current Gaps

The following parts are conceptually important but not yet modeled as first-class
runtime or analyzer abstractions:

- explicit element records such as `BrickElement`
- external role-assignment sources beyond attribute and alias evaluation
- generalized dependency kinds such as DI registration or reflection access
- explicit policy bundles and matrix models
- standardized violation records reusable outside the analyzer pipeline
- built-in role packs as a formal packaged concept
- export and reporting surfaces

These are not contradictions. They are the expected growth areas from the
current Bricks baseline toward a stronger structural platform.

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
    public int Priority { get; init; }
}
```

### BrickAlias

A `BrickAlias` maps an existing symbol or pattern to a canonical role.

Concept sketch:

```csharp
public sealed class BrickAlias
{
    public string AliasName { get; init; }
    public string CanonicalRoleName { get; init; }
    public BrickElementSelector? AppliesTo { get; init; }
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

Concept sketch:

```csharp
public sealed class BrickDependency
{
    public BrickElement Source { get; init; }
    public BrickElement Target { get; init; }
    public BrickDependencyKind Kind { get; init; }
    public BrickDependencyStrength Strength { get; init; }
}
```

### BrickRule

A `BrickRule` evaluates whether a role relationship is allowed.

Concept sketch:

```csharp
public sealed class BrickRule
{
    public string Name { get; init; }
    public BrickRoleSelector SourceRoles { get; init; }
    public BrickRoleSelector TargetRoles { get; init; }
    public BrickDependencySelector Dependencies { get; init; }
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
    public BrickEnforcementMode Enforcement { get; init; }
}
```

### BrickViolation

A `BrickViolation` is the normalized output of rule evaluation.

Concept sketch:

```csharp
public sealed class BrickViolation
{
    public string RuleName { get; init; }
    public BrickElement Source { get; init; }
    public BrickElement Target { get; init; }
    public BrickDependencyKind DependencyKind { get; init; }
    public BrickSeverity Severity { get; init; }
    public string Message { get; init; }
}
```

## Role System

The role model should support these invariants.

### Roles Are Multi-Valued

One element may carry multiple roles when the policy allows it.

Examples:

- `Contracts` plus `Shared`
- `TestOnly` plus `FriendConsumer`

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

Not every multi-role assignment is meaningful. The concept must allow explicit
combination validation, for example:

- `Contracts` plus `Shared`: plausible
- `Business.Sales` plus `Business.Support`: usually suspicious unless allowed

## Role Resolution

Role resolution is a formal part of the concept, not an incidental detail.

### Role Sources

Roles may come from:

1. direct attribute assignment
2. external configuration
3. convention
4. inference
5. alias mapping

### Recommended Priority

Recommended resolution priority:

1. direct element assignment
2. explicit alias on a concrete element
3. namespace-based assignment
4. assembly-based assignment
5. convention
6. inference

### Specificity Wins

More specific assignments should override broader ones where the policy demands
it.

Example:

- assembly role = `Business.Sales`
- namespace role = `Contracts`
- types inside the contracts namespace should resolve as `Contracts`, not just
  the broad assembly role

### Conflicts Must Be Visible

Conflicting assignments should surface explicitly instead of being silently
merged.

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

The concept should support at least:

- `Allow`
- `Deny`
- `Warn`
- `Ignore`

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

`Role × Role × DependencyKind`

It answers:

Does role A permit a dependency of kind X to role B?

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
dependency kind in a stronger future model.

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
