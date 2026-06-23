# NMolecules.Bricks — Core Concept

Layer: 1 — Core  
Status baseline: March 7, 2026  
Revision: March 15, 2026 — sharpened: all types defined, selectors explicit,
Layer 2 leakage removed, BrickAssignmentMode enum added, BrickTypeSelector
sketched, invariants tightened

Authoritative model reference: `../foundational-concept.md`.
This Layer 1 document expands the same target model in more detail; if a
statement conflicts with the foundational concept, the foundational concept
wins.

This document defines the **generic foundation** of `NMolecules.Bricks`.

It contains no domain-specific content, no built-in role packs, and no concrete
building blocks. Layer 2 (Building Blocks) provides those. Layer 3 (Use Cases)
provides concrete scenarios.

**Layer contract:** every concept in this document must be expressible without
any Layer 2 content. If removing all Layer 2 packages leaves this document
invalid, something here belongs in Layer 2.

---

## Layer Overview

```
┌─────────────────────────────────────────────────────┐
│  Layer 3 — Use Cases                                │
│  Concrete scenarios built from Layer 2 components  │
├─────────────────────────────────────────────────────┤
│  Layer 2 — Building Blocks                          │
│  Role Packs, NameConvention, Member Cardinality,    │
│  Architecture Packs, Library Adapter Packs, …       │
├─────────────────────────────────────────────────────┤
│  Layer 1 — Core  ◄ this document                   │
│  Generic types, resolution engine, rule model,      │
│  violation model, scope model, evaluation pipeline  │
└─────────────────────────────────────────────────────┘
```

Layer 2 is defined entirely in terms of Layer 1 concepts.
Layer 3 is defined entirely in terms of Layer 2 building blocks.
No layer reaches two levels down.

---

## Purpose

`NMolecules.Bricks` is a structural role-and-rule framework for .NET codebases.

Its core job is to:

- classify code elements semantically via roles
- assign roles explicitly or indirectly
- define structural rules between roles
- validate relationships deterministically
- surface violations consistently in analyzers, builds, tests, and reports

The framework reduces three recurring problems:

1. implicit structure that exists only in team knowledge
2. dependency erosion across intended boundaries
3. poor classification of legacy, external, generated, or test-only code

---

## Core Principle

> Elements carry roles.  
> Rules evaluate relationships between roles.  
> Roles can be assigned directly or mapped indirectly.

---

## Design Tenets

### Semantics Before Syntax

Namespaces, folders, and file names may help with discovery, but they are not
the authoritative semantic model. Roles are the authoritative semantic model.

### Existing Code Must Be Addressable

Bricks must not require a full refactor before a system becomes classifiable.
The model must accommodate external or indirect role assignment so that
unmodified existing code can receive roles without touching its source.

### .NET Reality Is Part Of The Model

Assemblies, `internal`, `InternalsVisibleTo`, DI registrations, generated code,
partial types, and reflection are not edge cases. They are structural facts that
influence the truth about a codebase and must be representable in the model.

### Rules Must Be Deterministic

The same code and the same policy input must always yield the same structural
classification and the same set of violations. Non-determinism in evaluation is
a correctness defect, not a configuration option.

### Roles Are First-Class

Every semantic classification is a role. There are no built-in special cases.
The core does not privilege any role name. All domain semantics live in Layer 2.

### Building Blocks Extend, Not Replace

Layer 2 adds instances of Layer 1 concepts: concrete roles, rule templates,
constraint implementations. It never redefines Layer 1 machinery. The core is
valid and complete with no Layer 2 package present.

### Violations Are Normalized

All evaluation stages produce `BrickViolation` records. A violation is the
only output unit. Diagnostics, test assertions, and reports all consume the
same violation type.

---

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

The member-cardinality contracts and naming convention system are Layer 2
concerns documented in the Building Blocks document.

---

## Current Gaps

The following parts are conceptually required but not yet modeled as first-class
runtime or analyzer abstractions:

- explicit element records (`BrickElement` as a runtime type)
- external role-assignment sources beyond attribute and alias
- formal role-resolution results with visible suppression and conflicts
- generalized dependency kinds: DI registration, reflection access
- explicit policy bundles and matrix models
- standardized violation records reusable outside the analyzer pipeline
- export and reporting surfaces
- `BrickTypeSelector` — direct type reference as rule target without role
  assignment; for external types too generic to carry a role
- `BrickAssignmentMode.AttributePresence` — role assigned when a specific
  attribute is present on the target element
- `BrickRoleAlias` type-parameter constraint — for generic aliases where
  the role depends on a type argument

---

## Type Catalogue

This section defines every type that the core model uses. No type is left
implicit. Supporting value types and enums are defined before the types that
use them.

---

### Supporting: Identifiers

All typed identifiers are `readonly record struct` — value types with
structural equality, no heap allocation per comparison.

```csharp
public readonly record struct BrickElementId(string Value);
public readonly record struct RoleId(string Value);
public readonly record struct RuleId(string Value);
```

---

### Supporting: `BrickElementKind`

The structural kind of an addressable element.

```csharp
public enum BrickElementKind
{
    Assembly,
    Namespace,
    Type,
    Member,
    Attribute,
    DiRegistration,    // conceptual; often not compile-time visible
    GeneratedArtifact,
    ExternalReference
}
```

---

### Supporting: `BrickElementOrigin`

How the element entered the evaluated system.

```csharp
public enum BrickElementOrigin
{
    Source,      // in-project source code
    Generated,   // emitted by a code generator
    External,    // from a referenced assembly outside the project
    Test         // in a test assembly
}
```

---

### Supporting: `BrickSourceLocation`

A source position for a violation or dependency. All fields optional because
not every element or violation has a source location.

```csharp
public readonly record struct BrickSourceLocation(
    string? FilePath,
    int? Line,
    int? Column,
    string? AssemblyName);
```

---

### Supporting: `BrickSeverity`

The diagnostic weight of a violation. Severity belongs to the violation, not
to the rule decision.

```csharp
public enum BrickSeverity
{
    Info,
    Warning,
    Error
}
```

---

### Supporting: `BrickScope`

The structural granularity at which a rule is evaluated. Scope determines the
set of evaluation units fed into the rule engine, not where roles are sourced.

```csharp
public enum BrickScope
{
    Global,     // the full evaluated system is one evaluation unit
    Assembly,   // each source assembly is an evaluation unit
    Namespace,  // each source namespace is an evaluation unit
    Type,       // each source type is an evaluation unit
    Member      // each source member is an evaluation unit
}
```

Scope is orthogonal to role assignment. A role declared at `Assembly` scope
applies to all contained elements unless a more specific assignment overrides
it. A rule at `Type` scope may use roles declared at `Assembly` scope.

**Global scope and Require:** At finer scopes (`Assembly` through `Member`),
a `Require` rule checks every matching evaluation unit independently. At
`Global` scope there is only one evaluation unit; the rule is satisfied if any
qualifying dependency exists anywhere in the system. Use `Global` only for
system-wide existence checks. For per-element requirements use `Type` or
`Member`.

---

### Supporting: `BrickDecision`

The structural intent of a rule. Distinct from diagnostic severity.

```csharp
public enum BrickDecision
{
    Allow,    // evaluated per concrete dependency
    Deny,     // evaluated per concrete dependency
    Require   // evaluated per source evaluation unit (aggregate check)
}
```

`Allow` and `Deny` evaluate one concrete dependency: does this dependency
that exists conform to policy?

`Require` evaluates a set: for each source evaluation unit whose effective
roles match `SourceRoles`, at least one dependency matching the rule selector
must exist within the scope. A missing required dependency produces a violation.

`Warn` and `Error` belong to `BrickSeverity`, not to `BrickDecision`.
`Ignore` is expressed via `BrickRuleExceptionSet` or the policy default, not
as a third decision value.

**Migration note:** `Allow`, `Deny`, and `Require` have fundamentally different
evaluation semantics. If `Require` grows to carry payload (e.g. `MinCount`),
the migration path is an abstract base class hierarchy:

```csharp
public abstract class BrickDecision { }
public sealed class Allow : BrickDecision { }
public sealed class Deny : BrickDecision { }
public sealed class Require : BrickDecision { public int MinCount { get; init; } = 1; }
```

The current enum is a deliberate simplification valid while `Require` carries
no payload.

---

### Supporting: `BrickPermissionDefault`

The fallback decision for dependencies that match no `Allow` or `Deny` rule.
Applies only to permission evaluation. `Require` rules have no default.

```csharp
public enum BrickPermissionDefault
{
    Allow,  // open policy: unaddressed dependencies are permitted
    Deny    // closed policy: unaddressed dependencies are forbidden
}
```

`BrickPermissionDefault` is a security-relevant choice and must be set
explicitly on every `BrickPolicy`. There is no framework default.

---

### Supporting: `BrickEnforcementMode`

How strictly the policy is applied in a given context.

```csharp
public enum BrickEnforcementMode
{
    Warn,   // violations produce warnings; build does not fail
    Error,  // violations produce errors; build fails
    Audit   // violations are recorded but not surfaced as diagnostics
}
```

---

### Supporting: `BrickAssignmentMode`

How a role was assigned to an element.

```csharp
public enum BrickAssignmentMode
{
    DirectAttribute,      // [Role(...)] on the element itself
    AliasMapping,         // via RoleAlias on a base type or interface
    ExternalConfiguration,// from external JSON/YAML policy config
    Convention,           // from a namespace or name pattern
    Inference,            // derived from context without explicit declaration
    AttributePresence     // near-term: role assigned when a specific attribute
                          // is present on the element (e.g. [ApiController])
}
```

`AttributePresence` is a near-term expansion. The infrastructure for
evaluating it is not yet part of the shipped baseline.

---

### Supporting: `BrickAssignmentSource`

The concrete source that provided an assignment. Recorded for provenance and
diagnostics. Source and Mode are independent: the same Mode may come from
different sources.

```csharp
public enum BrickAssignmentSource
{
    Attribute,
    ExternalConfig,
    AliasMapping,
    Convention,
    Inference,
    SourceGenerator  // emitted by a Roslyn Source Generator
}
```

---

### Supporting: `BrickAssignmentPrecedence`

A composite precedence value. Assignments are compared lexicographically:
higher `Specificity` wins; on a tie, higher `Authority` wins.

```csharp
public readonly record struct BrickAssignmentPrecedence(
    BrickAssignmentSpecificity Specificity,
    BrickAssignmentAuthority Authority);

public enum BrickAssignmentSpecificity
{
    Inference  = 0,  // derived without explicit declaration
    Convention = 1,  // pattern-based, implicit
    Assembly   = 2,  // declared at assembly scope
    Namespace  = 3,  // declared at namespace scope
    Element    = 4   // declared directly on the element
}

public enum BrickAssignmentAuthority
{
    Derived  = 0,  // inferred or convention-derived
    Alias    = 1,  // applied via alias resolution
    External = 2,  // from explicit external config
    Direct   = 3   // declared directly on the element
}
```

Equal precedence + `Exclusive` combination → conflict recorded, no silent
winner. Equal precedence + `Additive` combination → both assignments
accumulate.

Recommended effective precedence, strongest to weakest:
1. direct element assignment (`Element` + `Direct`)
2. explicit external element assignment (`Element` + `External`)
3. explicit alias on a concrete element (`Element` + `Alias`)
4. namespace-based assignment (`Namespace` + any)
5. assembly-based assignment (`Assembly` + any)
6. convention (`Convention` + any)
7. inference (`Inference` + any)

---

### Supporting: `BrickCombinationKind`

How two role sets interact when both are active on the same element.

```csharp
public enum BrickCombinationKind
{
    Additive,      // both may coexist; valid combination
    Exclusive,     // only one may remain; weaker is suppressed by precedence
    Incompatible   // always invalid; produces a violation post-resolution
}
```

- `Additive`: no resolution effect. Equal-precedence additive assignments
  accumulate.
- `Exclusive`: resolution input. Lower-precedence assignment is suppressed.
  Equal-precedence exclusive conflict surfaces in `BrickResolvedRoles.Conflicts`.
- `Incompatible`: post-resolution violation emitter. Resolution does not
  suppress either role. The combination is flagged after `EffectiveRoles` are
  established.

---

### Supporting: `BrickViolationKind`

Which evaluation stage produced the violation.

```csharp
public enum BrickViolationKind
{
    Dependency,              // a dependency violates a permission rule
    Requirement,             // a required dependency is absent
    RoleCombination,         // an Incompatible role combination is present
    RoleResolution,          // an Exclusive conflict could not be resolved
    ElementConstraint,       // an element violates a single-element constraint
    ElementConstraintConflict // conflicting element constraints, no override
}
```

`ElementConstraint` and `ElementConstraintConflict` are the generic pipeline
slots for Layer 2 constraint types (name constraints, cardinality contracts).
The concrete constraint type is carried in `RuleName` and `Evidence`.

---

### Supporting: `BrickDependencyKind`

The structural nature of a dependency between two elements.

```csharp
public enum BrickDependencyKind
{
    TypeReference,
    MethodCall,
    Inheritance,
    InterfaceImplementation,
    ObjectCreation,
    AttributeUsage,
    GenericConstraint,
    DiRegistration,       // often not compile-time visible; V1 gap
    ReflectionAccess,     // bypasses structural intent; V2
    FriendAssembly        // InternalsVisibleTo opening
}
```

---

### Supporting: `BrickDependencyStrength`

How strongly the dependency is coupled.

```csharp
public enum BrickDependencyStrength
{
    Strong,  // compile-time hard dependency
    Weak,    // runtime or optional dependency
    Inferred // derived from context, not directly declared
}
```

---

### Supporting: Selectors

Selectors are query predicates used to match elements, roles, and dependencies
in rules and aliases. They are the core's mechanism for expressing "which
elements / roles / dependencies does this rule apply to".

```csharp
/// <summary>
/// Matches structural elements by kind, namespace, assembly, or name pattern.
/// </summary>
public sealed class BrickElementSelector
{
    public BrickElementKind? Kind { get; init; }           // null = any kind
    public string? NamespacePattern { get; init; }         // glob, null = any
    public string? AssemblyPattern { get; init; }          // glob, null = any
    public string? NamePattern { get; init; }              // glob, null = any
    public BrickElementOrigin? Origin { get; init; }       // null = any origin
}

/// <summary>
/// Matches roles by name or hierarchical family (e.g. "Business.*").
/// </summary>
public sealed class BrickRoleSelector
{
    public required string Pattern { get; init; }  // exact name or glob
    // Examples: "DomainLayer", "Business.*", "*"
}

/// <summary>
/// Matches dependencies by kind. Null = any kind.
/// </summary>
public sealed class BrickDependencySelector
{
    public BrickDependencyKind? Kind { get; init; }   // null = any kind
    public BrickDependencyStrength? Strength { get; init; }
}

/// <summary>
/// Near-term: matches a concrete type by fully qualified name.
/// Allows rules to reference external types that carry no role.
/// </summary>
public sealed class BrickTypeSelector
{
    public required string FullyQualifiedTypeName { get; init; }
    public string? AssemblyName { get; init; }
    public bool IncludeSubtypes { get; init; } = true;
}
```

`BrickTypeSelector` is a near-term addition. It allows a rule to target a
specific external type (e.g. `System.Net.Http.HttpClient`) without assigning
that type a role. This covers the case where a type is too generic or too
context-dependent for a stable role assignment.

Dependency detection is broader than top-level declarations. Type usage must
be checked anywhere it becomes structurally relevant, including:

- fields and field initializers
- property declarations and property accessors
- method signatures and method bodies
- local functions and nested code blocks
- constructors
- destructors
- operators and conversions
- object creation expressions
- inheritance and interface implementation

If a forbidden type is used inside a member body, that usage still
participates in dependency evaluation.

---

## Target Meta-Model

The following types form the long-term model. All are generic — they carry
no domain assumptions. Domain semantics live in Layer 2.

---

### `BrickElement`

An addressable structural artifact in the .NET system.

```csharp
public sealed class BrickElement
{
    public required BrickElementId Id { get; init; }
    public required BrickElementKind Kind { get; init; }
    public required string DisplayName { get; init; }
    public string? AssemblyName { get; init; }
    public string? NamespaceName { get; init; }
    public string? FullName { get; init; }
    public required BrickElementOrigin Origin { get; init; }
}
```

---

### `BrickRole`

The semantic meaning assigned to one or more elements. Role names are opaque
strings to the core. All built-in role names live in Layer 2.

```csharp
public sealed class BrickRole
{
    public required string Name { get; init; }
    public string? Category { get; init; }
    public string? Description { get; init; }
    public bool IsBuiltin { get; init; }
}
```

---

### `BrickRoleAssignment`

Applies a role to a set of elements through a concrete source.

```csharp
public sealed class BrickRoleAssignment
{
    public required BrickElementSelector Selector { get; init; }
    public required string RoleName { get; init; }
    public required BrickAssignmentMode Mode { get; init; }
    public required BrickAssignmentSource Source { get; init; }
    public required BrickAssignmentPrecedence Precedence { get; init; }
}
```

**Declaration surface — string-based vs. generic attribute (C# 11):**
`RoleName` is always a string at the model level, supporting external config
and dynamic roles. C# 11 generic attributes allow a type-safe declaration
surface for statically known roles. Both compile to the same
`BrickRoleAssignment` record:

```csharp
[Role("MyRole")]           // string-based; works with any role
[Role<MyRoleMarker>]       // C# 11 generic; compile-time checked
```

The attribute form is a declaration surface only. It does not change the
resolution model.

---

### `BrickAlias`

Maps an existing symbol or hierarchy to a canonical role. The primary
mechanism for assigning roles to types that cannot be annotated directly
(external libraries, generated types).

`AppliesTo = null` means the alias applies globally to all elements matching
the alias name or type pattern.

```csharp
public sealed class BrickAlias
{
    public required string AliasName { get; init; }        // or source type name
    public required string CanonicalRoleName { get; init; }
    public BrickElementSelector? AppliesTo { get; init; }  // null = global
    public string? Reason { get; init; }
}
```

**Relationship to `BrickRoleAssignment`:** `BrickAlias` is a declaration of
intent. During resolution, each alias match produces one or more
`BrickRoleAssignment` records with `Mode = AliasMapping` and
`Source = AliasMapping`. The alias is not directly evaluated — it is
materialized into assignments during the candidate collection phase.

**Near-term — type-parameter constraint:** For generic aliases where the
role depends on a type argument (e.g. `IRequestHandler<T>` where T
determines the ring), a `TypeParameterConstraint` property will be added:

```csharp
// Near-term addition to BrickAlias:
public Type? TypeParameterConstraint { get; init; }
public int? TypeParameterIndex { get; init; }
```

---

### `BrickDependency`

A concrete relationship between two elements observed in the analyzed code.

```csharp
public sealed class BrickDependency
{
    public required BrickElement Source { get; init; }
    public required BrickElement Target { get; init; }
    public required BrickDependencyKind Kind { get; init; }
    public required BrickDependencyStrength Strength { get; init; }
    public required BrickDependencyEvidence Evidence { get; init; }
    public BrickSourceLocation? Location { get; init; }
    public string? Detail { get; init; }
}

/// <summary>
/// What the analyzer observed that produced this dependency record.
/// </summary>
public sealed class BrickDependencyEvidence
{
    public required string Description { get; init; }
    public BrickSourceLocation? Location { get; init; }
    public string? RawText { get; init; }
}
```

---

### `BrickRoleConflict`

Records an unresolvable conflict between two assignments in the same exclusive
role family during resolution.

```csharp
public sealed class BrickRoleConflict
{
    public required BrickRoleAssignment FirstAssignment { get; init; }
    public required BrickRoleAssignment SecondAssignment { get; init; }
    public required string Reason { get; init; }
}
```

---

### `BrickRoleCombinationRule`

Declares how two role sets interact when both are active on the same element.

```csharp
public sealed class BrickRoleCombinationRule
{
    public required string Name { get; init; }
    public required BrickRoleSelector LeftRoles { get; init; }
    public required BrickRoleSelector RightRoles { get; init; }
    public required BrickCombinationKind Kind { get; init; }
    public string? Reason { get; init; }
}
```

Selectors may target exact names, hierarchical families (`Business.*`), or
wildcards (`*`). This avoids exploding family-wide rules into manual pairwise
declarations.

---

### `BrickRule`

Evaluates a role relationship or structural requirement.

```csharp
public sealed class BrickRule
{
    public required string Name { get; init; }
    public required BrickRoleSelector SourceRoles { get; init; }
    public required BrickRoleSelector TargetRoles { get; init; }
    public required BrickDependencySelector Dependencies { get; init; }
    public required BrickScope Scope { get; init; }
    public required BrickDecision Decision { get; init; }
    public BrickRuleExceptionSet? Exceptions { get; init; }
    public required BrickSeverity Severity { get; init; }
}

/// <summary>
/// Explicit exceptions to a rule. Matching dependencies or elements
/// are excluded from evaluation for this rule.
/// </summary>
public sealed class BrickRuleExceptionSet
{
    public IReadOnlyList<BrickElementSelector> ExcludedSources { get; init; }
        = [];
    public IReadOnlyList<BrickElementSelector> ExcludedTargets { get; init; }
        = [];
    public IReadOnlyList<BrickTypeSelector> ExcludedTargetTypes { get; init; }
        = [];
    public string? Reason { get; init; }
}
```

`TargetRoles` is evaluated from resolved `EffectiveRoles`, not raw assignments.
For `Require` rules, `TargetRoles` selects the target evaluation units that
must be reachable.

---

### `BrickPolicy`

Groups rules into a coherent enforcement unit.

```csharp
public sealed class BrickPolicy
{
    public required string Name { get; init; }
    public required IReadOnlyList<BrickRule> Rules { get; init; }
    public required IReadOnlyList<BrickRoleCombinationRule> CombinationRules { get; init; }
    public required IReadOnlyList<BrickRoleAssignment> ExternalAssignments { get; init; }
    public required IReadOnlyList<BrickAlias> Aliases { get; init; }
    public required BrickPermissionDefault DefaultDecision { get; init; }
    public required BrickEnforcementMode Enforcement { get; init; }
}
```

`DefaultDecision` applies only to unmatched permission evaluation.
`Require` rules have no default: an unsatisfied `Require` always produces a
violation. `DefaultDecision` is a security-relevant choice and must be
explicit on every policy.

`ExternalAssignments` and `Aliases` are now first-class policy members.
This removes the distinction between "config-file assignments" and
"attribute assignments" at the policy level — both are resolved in the same
candidate collection phase.

---

### `BrickViolation`

The normalized output of any evaluation stage. All stages produce this type.

```csharp
public sealed class BrickViolation
{
    public required BrickViolationKind Kind { get; init; }
    public string? RuleName { get; init; }
    public required BrickElement Source { get; init; }
    public BrickElement? Target { get; init; }
    public BrickDependencyKind? DependencyKind { get; init; }
    public required BrickScope Scope { get; init; }
    public required BrickSeverity Severity { get; init; }
    public required string Message { get; init; }
    public required IReadOnlyList<string> EffectiveSourceRoles { get; init; }
    public required IReadOnlyList<string> EffectiveTargetRoles { get; init; }
    public required IReadOnlyList<string> RelatedRoles { get; init; }
    public BrickSourceLocation? Location { get; init; }
    public string? Evidence { get; init; }
}
```

Field contract by violation kind:

| Kind | Target | DependencyKind | EffectiveSourceRoles | EffectiveTargetRoles | RelatedRoles |
|---|---|---|---|---|---|
| `Dependency` | set | set | set | set | empty |
| `Requirement` | null | set | set | set | empty |
| `RoleCombination` | null | null | empty | empty | set |
| `RoleResolution` | null | null | empty | empty | set |
| `ElementConstraint` | null | null | set | empty | empty |
| `ElementConstraintConflict` | null | null | set | empty | set |

`RelatedRoles` carries the roles directly involved in a combination or
resolution violation, where source/target distinction has no meaning.
`EffectiveTargetRoles` is empty for element-only violations.

---

### `BrickResolvedRoles`

The explicit result of role resolution for one element. Fully inspectable
after evaluation. No intermediate state is discarded.

```csharp
public sealed class BrickResolvedRoles
{
    public required BrickElement Element { get; init; }
    public required IReadOnlyList<BrickRoleAssignment> CandidateAssignments { get; init; }
    public required IReadOnlyList<string> EffectiveRoles { get; init; }
    public required IReadOnlyList<BrickRoleAssignment> AppliedAssignments { get; init; }
    public required IReadOnlyList<BrickRoleAssignment> SuppressedAssignments { get; init; }
    public required IReadOnlyList<BrickRoleConflict> Conflicts { get; init; }
}
```

`CandidateAssignments` — all assignments collected before any precedence
decision. Includes assignments that are later suppressed.

`SuppressedAssignments` — assignments removed by `Exclusive` combination rules.

`Conflicts` — equal-precedence `Exclusive` pairs that could not be resolved.

`EffectiveRoles` — the final resolved role set used in rule evaluation.

Resolution lifecycle:

```text
1. Collect all applicable assignments into CandidateAssignments
2. Apply Exclusive combination rules:
     lower-precedence assignments → SuppressedAssignments
     equal-precedence exclusive pairs → Conflicts
3. Remaining assignments → AppliedAssignments; their roles → EffectiveRoles
4. Post-resolution: check Incompatible pairs against EffectiveRoles
     → emit RoleCombination violations (EffectiveRoles unchanged)
```

---

## Role System Invariants

### Multi-Valued

One element may carry multiple roles when the policy allows it.

Duplicate unparameterized roles do not accumulate. If multiple assignments
resolve to the same role name on the same element, the effective role set
contains that role only once.

Examples:

- direct `Contracts` plus external `Contracts` -> one effective `Contracts`
- namespace `Shared` plus assembly `Shared` -> one effective `Shared`

An exception is allowed for parameterized role instances where the same role
name is intentionally reused with different parameter identity.

Examples:

- `Adapter(Direction=Inbound)`
- `Adapter(Direction=Outbound)`

Conceptually, effective role identity is:

`RoleName + optional parameter identity`

### Hierarchical Naming

Role families use dot-separated names: `Business`, `Business.Sales`,
`Business.Support`. The core treats role names as opaque strings.
`BrickRoleSelector` matches families via glob patterns (`Business.*`).
Hierarchy semantics are a naming convention; the core does not parse the
hierarchy.

### Categorized

Roles may belong to categories. Categories are strings — the core imposes no
predefined category names. They are defined by the policy.

### Combination Rules Are Mandatory for Multi-Role

When an element may carry more than one role, at least one combination rule
must explicitly classify that pairing as `Additive`. The absence of a
combination rule for a pair does not make the pair valid; it means the
behavior is undefined. Policies should include an explicit `Additive` rule
for every expected multi-role combination.

---

## Rule Model

### Rule Families (generic)

Dependency rules can be organized into these generic families. No family
carries a concrete role name — those live in Layer 2:

- **Isolation:** elements of role A must not depend on elements of role B
- **Direction:** dependencies must flow in a defined direction
- **Contract access:** elements may only depend on a specific contract role
- **Ownership:** only specific roles may create or modify elements of another
  role
- **Visibility:** controls which roles may see `internal` members
- **Exception:** explicit, documented deviations from a rule

Layer 2 adds further constraint families (naming, cardinality) as extension
slots in the pipeline, not as extensions of these families.

### Element Constraints

An **element constraint** evaluates a property of a single element rather than
a relation between two elements. The core defines the pipeline slot and the
violation kinds. Concrete constraint types (name constraints, cardinality
contracts) live entirely in Layer 2.

Element constraints are not `BrickRule` instances — they are a parallel
constraint model with their own resolution, conflict detection, and violation
emission.

---

## Evaluation Pipeline

```text
1. Candidate assignment collection
      For each element:
        collect all BrickRoleAssignment records from all sources
        (attributes, aliases, external config, convention, inference)
        → CandidateAssignments

2. Combination-aware role resolution
      For each element:
        apply Exclusive combination rules by precedence
        → SuppressedAssignments, Conflicts, AppliedAssignments, EffectiveRoles

3. Post-resolution violation emission
      For each element:
        evaluate Incompatible combination rules against EffectiveRoles
        emit RoleCombination violations
        emit RoleResolution violations for unresolved Exclusive conflicts
        EffectiveRoles is not modified in this step

4. Element constraint evaluation        ← Layer 2 extension point
      For each element:
        resolve constraint sources (Layer 2 constraint types)
        detect conflicts between constraints
        emit ElementConstraint violations
        emit ElementConstraintConflict violations

5. Permission evaluation
      For each concrete dependency:
        evaluate cross-product of EffectiveRoles (source × target)
        remove matches covered by Exceptions
        if any remaining match → Deny: emit Dependency violation
        else if any match → Allow: dependency permitted
        else: apply DefaultDecision (must be explicit)

6. Requirement evaluation
      For each source evaluation unit matching SourceRoles:
        check whether at least one qualifying outbound dependency exists
        within the Scope and matching TargetRoles
        missing dependency → emit Requirement violation
        DefaultDecision does not apply
```

**Invariant:** Steps 1–4 operate on elements and their properties.
Steps 5–6 operate on dependencies between elements.
Violations from all steps are independent and may coexist in the same report.
No step modifies the output of a preceding step.

---

## Matrix Model

### Primary Matrix

`Role × Role × DependencyKind × Scope`

Answers: does role A permit a dependency of kind K at scope S to role B?

Evaluation always starts from `EffectiveRoles`, never from raw assignments.
One concrete dependency is evaluated by computing the cross-product of the
source element's `EffectiveRoles` and the target element's `EffectiveRoles`.

### Permission Aggregation

For one dependency:

1. Compute cross-product: source effective roles × target effective roles
2. Find all rules matching the cross-product entry, dependency kind, and scope
3. Remove matches covered by `Exceptions`
4. If any remaining match has decision `Deny` → violation
5. If at least one remaining match has decision `Allow` → permitted
6. Otherwise → `DefaultDecision` (must be explicit; no implicit default)

### Requirement Evaluation (separate pass)

For each source evaluation unit:

1. Identify all `Require` rules whose `SourceRoles` match the unit's
   `EffectiveRoles`
2. For each such rule, check whether at least one outbound dependency exists
   that matches `TargetRoles`, `Dependencies`, and `Scope`
3. If no such dependency exists → emit `Requirement` violation

`DefaultDecision` does not participate in requirement evaluation.

### Additional Matrices (planned)

- role-combination matrix (which pairs are Additive/Exclusive/Incompatible)
- visibility matrix (which roles may see internal members)
- exception matrix (which rule/element pairs have active exceptions)
- severity matrix (severity per rule and scope)
- enforcement matrix (enforcement mode per policy and environment)

---

## .NET-Specific Model Requirements

### Assembly Is A Primary Scope

Assemblies are not packaging artifacts. They are structural and visibility
boundaries. `BrickScope.Assembly` is a first-class scope value.
`InternalsVisibleTo` is an explicit structural opening and must be treated
as a `FriendAssembly` dependency kind, not as infrastructure noise.

### Partial Types

Evaluation is symbol-based, not file-based. A partial type spread across
multiple files is one `BrickElement`. No rule may produce different results
depending on how a type's source is split across files.

### Generated Code

Generated code has `BrickElementOrigin.Generated`. Rules may discriminate
by origin. Role assignment must handle generated elements — either via alias,
external config, or namespace-convention — without requiring generated files
to be modified.

### Dependency Injection

DI registration is a real structural relationship between a service type and
its implementation. It corresponds to `BrickDependencyKind.DiRegistration`.
DI registrations are often not visible at compile time. Resolution via Source
Generator or runtime enrichment is required. This is a V1 gap.

### Reflection

Reflection can bypass structural intent. `BrickDependencyKind.ReflectionAccess`
reserves the kind. Practical detection requires heuristics or runtime
instrumentation. This is a V2 concern.

### Source Generators

Generator-emitted assignments must be indistinguishable from attribute-declared
ones at the resolution layer. Both produce `BrickRoleAssignment` records with
`Source = SourceGenerator`, `Mode = Convention` or `ExternalConfiguration`,
and the appropriate `Precedence`. The generator's output is not privileged.

---

## Enforcement Model

Three enforcement surfaces consume `BrickViolation` records:

**Analyzer:** Roslyn diagnostic pipeline. Runs live in IDE and at build time.
Primary surface for developer feedback. Performance-critical — evaluation must
stay within acceptable build-time overhead.

**Testing:** Architecture-test API. Runs outside the compiler. Consumes
`BrickViolation` records and asserts on them. Useful for CI gates that do not
run the full build.

**Reporting:** Export surfaces that consume violation records, resolved role
maps, and dependency graphs. Output formats: JSON, Mermaid, PlantUML, matrix
reports.

All three surfaces consume the same `BrickViolation` type. No surface has
privileged access to evaluation internals.

---

## Packaging Direction

Conceptual target shape:

```
nMolecules.Bricks.Abstractions    ← this layer's types
nMolecules.Bricks.Runtime         ← resolution engine, policy evaluation
nMolecules.Bricks.Analyzers       ← Roslyn diagnostic integration
nMolecules.Bricks.Testing         ← architecture test API
nMolecules.Bricks.Export          ← reporting and export surfaces
nMolecules.Bricks.Roles.*         ← Layer 2 role packs
nMolecules.Bricks.Conventions     ← Layer 2 naming and cardinality
nMolecules.Bricks.Adapters.*      ← Layer 2 library adapter packs
nMolecules.Bricks.Integrations    ← CI, IDE, generator integrations
```

**Fluent API (C# 13 `params`):** When a builder API is introduced,
`params IEnumerable<T>` enables ergonomic multi-value declarations:

```csharp
rule.ForRoles("RoleA", "RoleB")
policy.Deny().From("RoleA").To("RoleB")
```

This is a declaration-surface concern. It does not affect the underlying types.

---

## V1 Boundary

The effective V1 baseline covers:

- role assignment via attribute and alias
- deterministic dependency rule evaluation
- role-based dependency validation
- analyzer diagnostics (`0001`, `0002`)

Near-term expansion:

- external assignment sources (JSON/YAML config as first-class policy input)
- test API
- friend-assembly rules
- richer dependency kinds (DI, reflection)
- reporting and export
- `BrickTypeSelector` in rules
- `BrickAssignmentMode.AttributePresence`
- `BrickRoleAlias` type-parameter constraint

Layer 2 building blocks (member cardinality, naming conventions, role packs)
may ship alongside V1 core but are versioned independently.

---

## Boundary Statement

Bricks is not:

- only a DDD library
- only a namespace-rule checker
- only a bag of marker attributes

Bricks is:

> a semantic role-and-rule model for structural code boundaries in .NET

Everything domain-specific — role names, naming conventions, architecture
style enforcement, library aliases — is defined in Layer 2. The core is the
stable substrate they build on. A codebase using only Layer 1 can define its
own roles, its own rules, and enforce its own structural intent. No Layer 2
package is required for the core to function.

---

## One-Sentence Summary

`NMolecules.Bricks` models .NET artifacts as structural elements, assigns them
semantic roles directly or indirectly, and evaluates their relationships through
declarative rules so architectural boundaries become explicit, analyzable, and
documentable.
