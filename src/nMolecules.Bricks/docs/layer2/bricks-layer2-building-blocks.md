# NMolecules.Bricks — Building Blocks

Layer: 2 — Building Blocks  
Depends on: Layer 1 (Core)  
Status: March 2026

Authoritative model reference: `../foundational-concept.md`.
This Layer 2 document describes target building blocks on top of that model.
Unless a block is explicitly listed in the "Current Baseline" section of the
foundational concept, treat it as target-shape design work rather than shipped
API surface.

This document defines the **concrete building blocks** of `NMolecules.Bricks`.
Each building block is a packaged implementation of Layer 1 core concepts for
a specific structural concern. Building blocks add roles, constraint types, rule
templates, and attribute APIs — they do not redefine core machinery.

**Document scope:** everything that is reusable across projects but is not
generic enough to belong to the core. Concrete project-specific scenarios
belong to Layer 3 (Use Cases).

---

## Building Block Catalogue

| Building Block | Package | What it provides |
|---|---|---|
| Role Packs | `nMolecules.Bricks.Roles` | Named role sets for common patterns |
| Member Cardinality Contracts | `nMolecules.Bricks.Conventions` | Rules about member counts per type |
| Naming Conventions | `nMolecules.Bricks.Conventions` | Rules about element names derived from type hierarchy |
| Clean Architecture Pack | `nMolecules.Bricks.Roles.CleanArchitecture` | Roles and dependency rules for Clean Architecture rings |
| Hexagonal Architecture Pack | `nMolecules.Bricks.Roles.Hexagonal` | Roles and rules for Ports & Adapters (Hexagonal Architecture) |

---

## Building Block 1: Role Packs

Role Packs define named `BrickRole` sets for common architectural patterns.
They are the Layer 2 answer to "which roles should I use?" — without them,
each project must define roles from scratch.

Role Pack roles are ordinary `BrickRole` instances with `IsBuiltin = true`.
Projects use them by importing the pack and declaring assignments via
`RoleAttribute`, `RoleAliasAttribute`, or external config.

### Structural Core Pack

Package: `nMolecules.Bricks.Roles.Core`

General-purpose roles applicable to any .NET codebase regardless of
architecture style:

| Role | Meaning |
|---|---|
| `Business.*` | Role family for bounded business areas; subrolled per domain |
| `Contracts` | Public interface surface; may be consumed across boundaries |
| `Shared` | Shared utilities or infrastructure without business semantics |
| `Infrastructure` | Technical plumbing: persistence, messaging, external adapters |
| `Platform` | Cross-cutting concerns managed by a platform team |
| `Legacy` | Code under retirement; receives relaxed dependency rules |
| `Generated` | Machine-generated code; receives explicit origin handling |
| `TestOnly` | Test infrastructure; must not be referenced in production code |

Default combination rules shipped with this pack:

```csharp
// Contracts + Shared is a valid combination
CombinationRule("Contracts+Shared", "Contracts", "Shared", BrickCombinationKind.Additive)

// Generated code must not also be classified as business code
CombinationRule("Generated+Business", "Generated", "Business.*", BrickCombinationKind.Incompatible)

// TestOnly code must not enter production
CombinationRule("TestOnly+Production", "TestOnly", "Business.*", BrickCombinationKind.Incompatible)
CombinationRule("TestOnly+Contracts", "TestOnly", "Contracts", BrickCombinationKind.Incompatible)
```

### DDD Pack

Package: `nMolecules.Bricks.Roles.Ddd`

Roles for Domain-Driven Design building blocks:

| Role | Meaning |
|---|---|
| `Entity` | Has identity; mutable over lifetime |
| `ValueObject` | Defined by attributes; immutable; no identity |
| `AggregateRoot` | Consistency boundary; controls access to its aggregate |
| `Repository` | Persistence abstraction for aggregates |
| `Factory` | Encapsulates complex construction logic |
| `ApplicationService` | Orchestrates use cases; no domain logic |
| `DomainService` | Domain logic that does not fit on an entity or value object |
| `InfrastructureService` | Technical service; implements domain contracts |
| `Identity` | Typed identifier for an aggregate root |
| `BoundedContext` | Explicit semantic boundary |
| `Module` | Logical grouping within a bounded context |

Note: The coarse `Service` role is intentionally absent. DDD contexts
distinguish Application, Domain, and Infrastructure services with different
dependency rules; merging them under one role obscures meaningful violations.

Default dependency rules shipped with this pack:

```csharp
// Domain services must not depend on infrastructure
Rule("DomainService→Infrastructure",
    source: "DomainService", target: "InfrastructureService",
    kind: null, decision: BrickDecision.Deny)

// Repositories may only be accessed via AggregateRoot scope
Rule("Repository-access",
    source: "ApplicationService", target: "Repository",
    kind: null, decision: BrickDecision.Allow)
Rule("Repository-direct-from-domain",
    source: "DomainService", target: "Repository",
    kind: null, decision: BrickDecision.Deny)
```

### Events Pack

Package: `nMolecules.Bricks.Roles.Events`

Roles for event-driven patterns:

| Role | Meaning |
|---|---|
| `DomainEvent` | Signals something that happened within the domain |
| `DomainEventHandler` | Reacts to a domain event |
| `DomainEventPublisher` | Infrastructure surface for publishing domain events |
| `IntegrationEvent` | Cross-boundary event; not a domain concept |

Default combination rule:

```csharp
// DomainEvent and IntegrationEvent on the same type requires explicit resolution
CombinationRule("DomainEvent+IntegrationEvent",
    "DomainEvent", "IntegrationEvent", BrickCombinationKind.Exclusive)
```

### Architecture Pack

Package: `nMolecules.Bricks.Roles.Architecture`

Roles for layered architecture:

| Role | Meaning |
|---|---|
| `DomainLayer` | Core domain model; no infrastructure dependencies |
| `ApplicationLayer` | Use case orchestration; depends on domain only |
| `InfrastructureLayer` | Technical implementations; may depend on all layers |
| `InterfaceLayer` | Entry points: API controllers, UI, CLI |

Default dependency rules shipped with this pack:

```csharp
Rule("Domain-no-infra",
    source: "DomainLayer", target: "InfrastructureLayer",
    decision: BrickDecision.Deny)
Rule("Domain-no-interface",
    source: "DomainLayer", target: "InterfaceLayer",
    decision: BrickDecision.Deny)
Rule("Application-no-infra",
    source: "ApplicationLayer", target: "InfrastructureLayer",
    decision: BrickDecision.Deny)
Rule("Application-no-interface",
    source: "ApplicationLayer", target: "InterfaceLayer",
    decision: BrickDecision.Deny)
```

Future role packs: Onion, Hexagonal, CQRS, Saga, ReadModel, Adapter, Port.

---

## Building Block 2: Member Cardinality Contracts

Package: `nMolecules.Bricks.Conventions`  
Diagnostics: `XMoleculesBricks0003`–`XMoleculesBricks0010`

Member cardinality contracts enforce structural rules about how many members
of a given kind a type may or must declare. This is an element constraint in
the Layer 1 sense: it evaluates a property of a single element.

### Attributes

```csharp
/// <summary>
/// The annotated type must declare exactly one member matching the selector.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface,
    AllowMultiple = true)]
public sealed class RequireExactlyOneMemberAttribute : Attribute
{
    public required string MemberSelector { get; init; }
    public string? Reason { get; init; }
}

/// <summary>
/// The annotated type must declare all members specified.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface,
    AllowMultiple = true)]
public sealed class RequireAllMembersAttribute : Attribute
{
    public required string[] Members { get; init; }
    public string? Reason { get; init; }
}

/// <summary>
/// The annotated type must declare exactly N members matching the selector.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface,
    AllowMultiple = true)]
public sealed class RequireMemberCountAttribute : Attribute
{
    public required string MemberSelector { get; init; }
    public required int Count { get; init; }
    public string? Reason { get; init; }
}

/// <summary>
/// The annotated type must declare a marker count within the configured range.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface,
    AllowMultiple = true)]
public sealed class RequireMemberRangeAttribute : Attribute
{
    public required string MemberSelector { get; init; }
    public required int MinimumCount { get; init; }
    public required int MaximumCount { get; init; }
    public string? Reason { get; init; }
}

/// <summary>
/// The annotated type must not declare members matching the selector.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface,
    AllowMultiple = true)]
public sealed class ForbidMemberAttribute : Attribute
{
    public required string MemberSelector { get; init; }
    public string? Reason { get; init; }
}

/// <summary>
/// The annotated type may declare several named markers, but each name must be unique.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface,
    AllowMultiple = true)]
public sealed class RequireUniqueNamedMemberAttribute : Attribute
{
    public required string MemberSelector { get; init; }
    public string NameArgument { get; init; } = "Name";
    public string? Reason { get; init; }
}

public sealed class RequireNamedMembersAttribute : Attribute
{
    public required string MemberSelector { get; init; }
    public required string[] RequiredNames { get; init; }
    public string NameArgument { get; init; } = "Name";
    public string? Reason { get; init; }
}

/// <summary>
/// The annotated type must declare exactly one of the specified members
/// (exclusive choice).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface,
    AllowMultiple = true)]
public sealed class RequireExclusiveChoiceAttribute : Attribute
{
    public required string[] Members { get; init; }
    public string? Reason { get; init; }
}
```

### Diagnostics

| ID | Trigger |
|---|---|
| `XMoleculesBricks0003` | `RequireExactlyOneMember` — zero or more than one member found |
| `XMoleculesBricks0004` | `RequireAllMembers` — one or more required members missing |
| `XMoleculesBricks0005` | `RequireMemberCount` — member count does not match N |
| `XMoleculesBricks0006` | `RequireExclusiveChoice` — zero or more than one of the exclusive members found |
| `XMoleculesBricks0007` | `RequireMemberRange` — member count is outside the inclusive range |
| `XMoleculesBricks0008` | `ForbidMember` — one or more forbidden member markers found |
| `XMoleculesBricks0009` | `RequireUniqueNamedMember` — more than one member uses the same marker name |
| `XMoleculesBricks0010` | `RequireNamedMembers` — one or more required marker names are missing |

### Pipeline Slot

Member cardinality contracts run in **Pipeline Step 4** (element constraint
evaluation). They evaluate after role resolution and before permission/
requirement evaluation.

---

## Building Block 3: Naming Conventions

Package: `nMolecules.Bricks.Conventions`  
Analyzer diagnostics: `XMoleculesBricks0020`–`XMoleculesBricks0023`

Naming conventions enforce structural rules about how elements must be named
when they implement or inherit a specific type. This is an element constraint:
it evaluates the name of a single element, not a dependency between two elements.

### Core Types

```csharp
/// <summary>
/// Where the pattern must appear in the element's name.
/// </summary>
public enum NamePosition
{
    Any,       // alias import: no position restriction
    Prefix,    // name must start with Pattern
    Suffix,    // name must end with Pattern
    Contains,  // name must contain Pattern anywhere
    Exact      // name must equal Pattern exactly
}
```

### 3.1 `NameConventionAttribute` — Convention definieren

Placed on the interface or base class. Declares that all implementing or
inheriting types must conform to the naming pattern.

```csharp
[AttributeUsage(
    AttributeTargets.Interface | AttributeTargets.Class,
    AllowMultiple = true,
    Inherited = false)]
public sealed class NameConventionAttribute : Attribute
{
    public NameConventionAttribute(string pattern, NamePosition position)
    {
        Pattern = pattern;
        Position = position;
    }

    /// <summary>The required name fragment.</summary>
    public string Pattern { get; }

    /// <summary>Where the pattern must appear in the type name.</summary>
    public NamePosition Position { get; }

    /// <summary>
    /// Required = this convention must match by itself.
    /// Alternative = one active alternative convention must match.
    /// </summary>
    public NameConventionRequirement Requirement { get; set; }

    /// <summary>
    /// When true, convention applies only to direct implementors/derivations.
    /// Default false: applies transitively to all descendants.
    /// </summary>
    public bool DirectOnly { get; set; }

    /// <summary>Surfaced in the diagnostic message.</summary>
    public string Reason { get; set; }
}

public enum NameConventionRequirement
{
    Required,
    Alternative
}
```

### 3.2 `NameConventionAliasAttribute` — Convention auf fremden Typ übertragen

Placed on an own interface or class to import naming conventions from a type
that cannot be annotated directly (external library, third-party base class).

```csharp
[AttributeUsage(
    AttributeTargets.Interface | AttributeTargets.Class,
    AllowMultiple = true,
    Inherited = false)]
public sealed class NameConventionAliasAttribute : Attribute
{
    public NameConventionAliasAttribute(Type sourceType)
    {
        SourceType = sourceType;
    }

    /// <summary>
    /// The type whose NameConventionAttributes are imported.
    /// Must carry at least one NameConventionAttribute.
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    /// Restricts import to a specific position only.
    /// Any = import all conventions from SourceType.
    /// </summary>
    public NamePosition RestrictToPosition { get; set; } = NamePosition.Any;

    public string Reason { get; set; }
}
```

### 3.3 `NameConventionOverrideAttribute` — Kollision auflösen

Placed directly on the conflicting type. Resolves or suppresses one of the
active naming conventions explicitly.

```csharp
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
    AllowMultiple = true,
    Inherited = false)]
public sealed class NameConventionOverrideAttribute : Attribute
{
    public NameConventionOverrideAttribute(
        Type suppressedSource,
        NameConventionOverrideBehavior behavior = NameConventionOverrideBehavior.Suppress)
    {
        SuppressedSource = suppressedSource;
        Behavior = behavior;
    }

    /// <summary>The convention source to suppress or prefer.</summary>
    public Type SuppressedSource { get; }

    public NameConventionOverrideBehavior Behavior { get; }

    /// <summary>
    /// Mandatory justification. Forces the developer to explain the override.
    /// </summary>
    public string Reason { get; set; }
}

public enum NameConventionOverrideBehavior
{
    /// <summary>
    /// The convention from SuppressedSource is intentionally not applied.
    /// No violation is emitted for that convention on this element.
    /// </summary>
    Suppress,

    /// <summary>
    /// The convention from SuppressedSource wins over all conflicting others.
    /// All other conflicting conventions are treated as suppressed.
    /// </summary>
    Prefer
}
```

### 3.4 `NameConventionOverrideAliasAttribute` — Override ohne direkte Annotation

Placed on assembly level or a policy-declaration class. Declares an override
for a type that cannot be annotated directly (generated types, external types).

```csharp
[AttributeUsage(
    AttributeTargets.Assembly | AttributeTargets.Class,
    AllowMultiple = true)]
public sealed class NameConventionOverrideAliasAttribute : Attribute
{
    public NameConventionOverrideAliasAttribute(
        Type targetType,
        Type suppressedSource,
        NameConventionOverrideBehavior behavior)
    {
        TargetType = targetType;
        SuppressedSource = suppressedSource;
        Behavior = behavior;
    }

    public Type TargetType { get; }
    public Type SuppressedSource { get; }
    public NameConventionOverrideBehavior Behavior { get; }
    public string Reason { get; set; }
}
```

### Resolution Model

Name convention resolution mirrors role resolution but operates on name
constraints rather than role assignments:

```csharp
public sealed class BrickResolvedNameConventions
{
    public required BrickElement Element { get; init; }
    public required IReadOnlyList<BrickNameConstraint> CandidateConstraints { get; init; }
    public required IReadOnlyList<BrickNameConstraint> ActiveConstraints { get; init; }
    public required IReadOnlyList<BrickNameConstraint> SuppressedConstraints { get; init; }
    public required IReadOnlyList<BrickNameConstraintConflict> Conflicts { get; init; }
}

public sealed class BrickNameConstraint
{
    public required string Pattern { get; init; }
    public required NamePosition Position { get; init; }
    public NameConventionRequirement Requirement { get; init; }
    public required Type SourceType { get; init; }
    public bool DirectOnly { get; init; }
    public string? Reason { get; init; }
}
```

Resolution lifecycle for name conventions:

1. Collect all `NameConventionAttribute` instances reachable from the element
   via its type hierarchy (transitively unless `DirectOnly = true`)
2. Collect imported conventions via `NameConventionAliasAttribute`
3. Apply `NameConventionOverrideAttribute` and
   `NameConventionOverrideAliasAttribute`: suppressed constraints move to
   `SuppressedConstraints`
4. Detect conflicts: constraints are conflicting when no string exists that
   satisfies both simultaneously (see conflict detection rules below). Only
   `Required` constraints create `XMoleculesBricks0021`; `Alternative`
   constraints form a one-of set instead.
5. Unresolved conflicts surface in `Conflicts`
6. Active required constraints are checked against the element's name
7. Active alternative constraints pass when at least one alternative matches;
   if none match, the analyzer emits one `XMoleculesBricks0020`

### Conflict Detection

Two constraints conflict when they cannot be satisfied simultaneously.
The check is purely structural:

| Constraint A | Constraint B | Conflict? | Reason |
|---|---|---|---|
| Suffix "DomainEvent" | Suffix "IntegrationEvent" | **Yes** | No string ends with two different values |
| Suffix "Event" | Suffix "DomainEvent" | No | "OrderPlacedDomainEvent" satisfies both |
| Alternative suffix "Command" | Alternative suffix "Request" | No | one matching alternative is sufficient |
| Prefix "Order" | Suffix "DomainEvent" | No | "OrderPlacedDomainEvent" satisfies both |
| Exact "OrderEvent" | Suffix "DomainEvent" | **Yes** | "OrderEvent" does not end with "DomainEvent" |
| Contains "Event" | Suffix "DomainEvent" | No | "OrderPlacedDomainEvent" satisfies both |
| Prefix "X" | Exact "Y" | **Yes** unless "Y" starts with "X" |

### Diagnostics

| ID | Kind | Trigger |
|---|---|---|
| `XMoleculesBricks0020` | `ElementConstraint` | Type does not satisfy an active name convention |
| `XMoleculesBricks0021` | `ElementConstraintConflict` | Type has conflicting name conventions with no override |
| `XMoleculesBricks0022` | `ElementConstraint` | `NameConventionAlias` references a type with no `NameConventionAttribute` |
| `XMoleculesBricks0023` | `ElementConstraint` | `NameConventionOverride` references a convention source not active on this type |

**0020 message format:**
```
'[TypeName]' implements/inherits '[SourceType]' but its name does not
[begin with | end with | contain | equal] '[Pattern]'.
Convention source: [SourceType]. [Reason if set]
```

**0021 message format:**
```
'[TypeName]' has conflicting naming conventions:
  - [SourceType1] requires [position] '[Pattern1]'
  - [SourceType2] requires [position] '[Pattern2]'
These conventions cannot be satisfied simultaneously.
Add [NameConventionOverride] to suppress one and document the decision.
```

### Pipeline Integration

Name convention evaluation runs in **Pipeline Step 4** (element constraint
evaluation), alongside member cardinality contracts.

```text
Step 4: Element constraint evaluation
  4a. Member cardinality contracts (BrickMemberCardinalityConstraint)
  4b. Name convention constraints (BrickNameConstraint)
  4c. [future constraint types extend here]
```

Within step 4, the order of 4a and 4b is not significant. Violations from
each are independent.

---

## Building Block 4: Clean Architecture Pack

Package: `nMolecules.Bricks.Roles.CleanArchitecture`

Clean Architecture (Robert C. Martin) organises code in concentric rings. The
central rule — the **Dependency Rule** — states that source code dependencies
must always point inward. An outer ring may depend on an inner ring; an inner
ring must never know about an outer ring.

This building block provides:

- roles for all four rings and their sub-concerns
- a role for the Presenter pattern
- the complete default dependency policy enforcing the Dependency Rule
- naming convention declarations for each ring
- combination rules preventing ring-role ambiguity

### Rings and Roles

```
┌──────────────────────────────────────────────┐
│  Frameworks & Drivers        [CA.Frameworks] │
│  ┌────────────────────────────────────────┐  │
│  │  Interface Adapters      [CA.Adapters] │  │
│  │  ┌──────────────────────────────────┐  │  │
│  │  │  Application / Use Cases         │  │  │
│  │  │                  [CA.UseCases]   │  │  │
│  │  │  ┌────────────────────────────┐  │  │  │
│  │  │  │  Entities        [CA.Core] │  │  │  │
│  │  │  └────────────────────────────┘  │  │  │
│  │  └──────────────────────────────────┘  │  │
│  └────────────────────────────────────────┘  │
└──────────────────────────────────────────────┘
```

| Role | Ring | Meaning |
|---|---|---|
| `CA.Core` | Entities | Enterprise-wide business rules; no framework knowledge |
| `CA.UseCases` | Use Cases | Application-specific business rules; orchestrates entities |
| `CA.Adapters` | Interface Adapters | Converts data between use cases and external formats |
| `CA.Frameworks` | Frameworks & Drivers | Web frameworks, databases, UI, external tools |
| `CA.Presenter` | Interface Adapters | Formats output from use cases for delivery mechanisms |
| `CA.Gateway` | Interface Adapters | Interface to persistence or external systems (in adapters) |
| `CA.Controller` | Interface Adapters | Receives input and delegates to use cases |
| `CA.ViewModel` | Interface Adapters | Data structure for presentation; no business logic |

Sub-roles (`CA.Presenter`, `CA.Gateway`, `CA.Controller`, `CA.ViewModel`)
are additive on top of `CA.Adapters`. A controller is both `CA.Controller`
and `CA.Adapters`.

### Combination Rules

```csharp
// Ring roles are exclusive — a type belongs to exactly one ring
CombinationRule("CA-ring-exclusive-Core-UseCases",
    "CA.Core", "CA.UseCases", BrickCombinationKind.Exclusive)
CombinationRule("CA-ring-exclusive-Core-Adapters",
    "CA.Core", "CA.Adapters", BrickCombinationKind.Exclusive)
CombinationRule("CA-ring-exclusive-Core-Frameworks",
    "CA.Core", "CA.Frameworks", BrickCombinationKind.Exclusive)
CombinationRule("CA-ring-exclusive-UseCases-Adapters",
    "CA.UseCases", "CA.Adapters", BrickCombinationKind.Exclusive)
CombinationRule("CA-ring-exclusive-UseCases-Frameworks",
    "CA.UseCases", "CA.Frameworks", BrickCombinationKind.Exclusive)
CombinationRule("CA-ring-exclusive-Adapters-Frameworks",
    "CA.Adapters", "CA.Frameworks", BrickCombinationKind.Exclusive)

// Sub-roles are additive within their ring
CombinationRule("CA-Controller-Adapters",
    "CA.Controller", "CA.Adapters", BrickCombinationKind.Additive)
CombinationRule("CA-Presenter-Adapters",
    "CA.Presenter", "CA.Adapters", BrickCombinationKind.Additive)
CombinationRule("CA-Gateway-Adapters",
    "CA.Gateway", "CA.Adapters", BrickCombinationKind.Additive)
CombinationRule("CA-ViewModel-Adapters",
    "CA.ViewModel", "CA.Adapters", BrickCombinationKind.Additive)
```

### Interoperability with DDD Pack

Clean Architecture's Entities ring maps naturally to DDD tactical patterns.
The DDD Pack roles (`AggregateRoot`, `Entity`, `ValueObject`, `DomainEvent`,
etc.) are additive with `CA.Core`:

```csharp
CombinationRule("CA-Core-AggregateRoot",
    "CA.Core", "AggregateRoot", BrickCombinationKind.Additive)
CombinationRule("CA-Core-Entity",
    "CA.Core", "Entity", BrickCombinationKind.Additive)
CombinationRule("CA-Core-ValueObject",
    "CA.Core", "ValueObject", BrickCombinationKind.Additive)
CombinationRule("CA-Core-DomainEvent",
    "CA.Core", "DomainEvent", BrickCombinationKind.Additive)
CombinationRule("CA-UseCases-ApplicationService",
    "CA.UseCases", "ApplicationService", BrickCombinationKind.Additive)
CombinationRule("CA-Adapters-Repository-impl",
    "CA.Adapters", "InfrastructureService", BrickCombinationKind.Additive)
```

### Naming Conventions

```csharp
[Role("CA.UseCases")]
[NameConvention("UseCase", NamePosition.Suffix,
    Reason = "Use case classes must be identifiable by name")]
public interface IUseCase<TInput, TOutput> { }

[Role("CA.Presenter")]
[NameConvention("Presenter", NamePosition.Suffix,
    Reason = "Presenters must be identifiable by name")]
public interface IPresenter<TOutput> { }

[Role("CA.Gateway")]
[NameConvention("Gateway", NamePosition.Suffix,
    Reason = "Gateways must be identifiable by name")]
public interface IGateway { }

[Role("CA.Controller")]
[NameConvention("Controller", NamePosition.Suffix,
    Reason = "Controllers must be identifiable by name")]
public interface IController { }

[Role("CA.ViewModel")]
[NameConvention("ViewModel", NamePosition.Suffix,
    Reason = "ViewModels must be identifiable by name")]
public interface IViewModel { }
```

Entities (`CA.Core`) and the outer framework ring (`CA.Frameworks`) carry no
enforced naming suffix by default. Teams may add their own.

### Default Dependency Policy

The Dependency Rule expressed as Bricks rules:

```json
{
  "name": "CleanArchitecture",
  "defaultDecision": "Allow",
  "rules": [
    {
      "name": "CA-Core-no-UseCases",
      "description": "Entities must not know about use cases",
      "sourceRole": "CA.Core",
      "targetRole": "CA.UseCases",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "CA-Core-no-Adapters",
      "description": "Entities must not know about adapters",
      "sourceRole": "CA.Core",
      "targetRole": "CA.Adapters",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "CA-Core-no-Frameworks",
      "description": "Entities must not know about frameworks",
      "sourceRole": "CA.Core",
      "targetRole": "CA.Frameworks",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "CA-UseCases-no-Adapters",
      "description": "Use cases must not know about adapters",
      "sourceRole": "CA.UseCases",
      "targetRole": "CA.Adapters",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "CA-UseCases-no-Frameworks",
      "description": "Use cases must not know about frameworks",
      "sourceRole": "CA.UseCases",
      "targetRole": "CA.Frameworks",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "CA-Adapters-no-Frameworks",
      "description": "Adapters must not depend on framework internals directly",
      "sourceRole": "CA.Adapters",
      "targetRole": "CA.Frameworks",
      "decision": "Deny",
      "severity": "Warning"
    }
  ]
}
```

Note: the Adapters → Frameworks rule is `Warning` by default, not `Error`,
because some adapter implementations legitimately reference framework types
(e.g. an EF Core repository adapter references `DbContext`). Teams may
tighten this to `Error` when stricter isolation is desired.

---

## Building Block Interoperability

Building blocks are independent packages. A project may use:

- Core only (no building blocks)
- Core + Structural Core Pack only
- Core + DDD Pack + Events Pack
- Core + Naming Conventions only
- Any combination

Building blocks must not introduce circular dependencies on each other at the
package level. Role Packs, Member Cardinality, and Naming Conventions are
parallel building blocks on the same Layer 2 shelf — none depends on another.

---

## Adding a New Building Block

A new building block is valid if:

1. It is expressed entirely in terms of Layer 1 core types
2. It introduces no new core concepts — only new instances of existing concepts
   (roles, constraint types, rule templates, attributes)
3. It ships as a separate package that does not pull in other Layer 2 packages
4. It registers its constraint types with the Layer 1 `ElementConstraint`
   pipeline slot via a known extension point

Future building block candidates: interface contract rules, CQRS boundary
constraints, saga step ordering.

---

## Library Adapter Packs

Library Adapter Packs are a special category of Layer 2 building block. They
provide pre-defined `RoleAlias` declarations for well-known external .NET
libraries, making external types available to the Bricks role and rule system
without modifying those libraries.

Each pack is opinionated: it makes a concrete architectural decision about
what role a library type should carry. Projects that disagree define their
own aliases.

### Available Packs

| Package | Library | Provides |
|---|---|---|
| `nMolecules.Bricks.Adapters.MediatR` | MediatR | Aliases for `IRequest<T>`, `INotification`, `IRequestHandler<,>`, `IPipelineBehavior<,>` |
| `nMolecules.Bricks.Adapters.EfCore` | Entity Framework Core | Aliases for `DbContext`, `IEntityTypeConfiguration<T>`, `Migration` |
| `nMolecules.Bricks.Adapters.AspNetCore` | ASP.NET Core | Aliases for `ControllerBase`, `IMiddleware`, `BackgroundService`, `IHostedService` |
| `nMolecules.Bricks.Adapters.FluentValidation` | FluentValidation | Aliases for `AbstractValidator<T>`, `IValidator<T>` |
| `nMolecules.Bricks.Adapters.MassTransit` | MassTransit | Aliases for `IConsumer<T>`, `IPublishEndpoint` |
| `nMolecules.Bricks.Adapters.Rebus` | Rebus | Aliases for `IHandleMessages<T>`, `IBus` |

### Design Rules for Library Adapter Packs

A Library Adapter Pack is valid if:

1. It references only stable, public API surfaces of the target library
2. Every alias declaration includes a `Reason` that documents the architectural
   decision
3. It ships default aliases only — no mandatory policy rules; projects opt in
   to rules explicitly
4. It is versioned independently and aligned with the target library's major
   versions

### Example: MediatR Adapter Pack

```csharp
// nMolecules.Bricks.Adapters.MediatR

// IRequest<T> → Use case entry point
[RoleAlias(typeof(MediatR.IRequest<>), "CA.UseCases",
    Reason = "MediatR requests are use case entry points; " +
             "they belong in the application / use cases layer")]
// IRequestHandler<,> → Use case implementation
[RoleAlias(typeof(MediatR.IRequestHandler<,>), "CA.UseCases",
    Reason = "MediatR request handlers implement use cases")]
// INotification → Domain event (opinionated default; override if project uses
// INotification for integration events too)
[RoleAlias(typeof(MediatR.INotification), "DomainEvent",
    Reason = "MediatR notifications are treated as domain events by default")]
// INotificationHandler<T> → Domain event handler
[RoleAlias(typeof(MediatR.INotificationHandler<>), "DomainEventHandler",
    Reason = "MediatR notification handlers are domain event handlers")]
// IPipelineBehavior<,> → Cross-cutting platform concern
[RoleAlias(typeof(MediatR.IPipelineBehavior<,>), "Platform",
    Reason = "MediatR pipeline behaviors are cross-cutting platform concerns")]
[assembly: BrickLibraryAdapterPack("MediatR", version: "12.x")]
public static class MediatRBrickAliases { }
```

### Limitations of Library Adapter Packs

Three cases cannot be addressed by a Library Adapter Pack alone and require
either a custom bridge interface or a future framework extension:

**Case 1 — Type-parameter-dependent role:** `IRequestHandler<TRequest,
TResponse>` should be `CA.UseCases` when `TRequest` is an application command,
but `HEX.Core` when `TRequest` is a domain query. A global alias cannot
distinguish these. Solution: custom bridge interfaces per context.

**Case 2 — Attribute-presence assignment:** `[ApiController]` should assign
`CA.Adapters`, but the current alias mechanism works on base types, not
applied attributes. Requires `BrickAssignmentMode.AttributePresence`
(near-term expansion).

**Case 3 — DI registration as structural dependency:** The registration
`services.AddScoped<IOrderRepository, SqlOrderRepository>()` is a structural
statement that Bricks cannot see at compile time. Requires a Source Generator
to emit `BrickDependency` annotations (Phase 2).

Package: `nMolecules.Bricks.Roles.Hexagonal`

Hexagonal Architecture (Alistair Cockburn, also known as Ports & Adapters)
places the **application core** at the centre. The core communicates with the
outside world exclusively through **ports** — interfaces that belong to the
core. **Adapters** implement or call those ports. The core never knows about
adapters.

```
     ┌──────────────────────────────────────────────────────┐
     │                    Outside World                      │
     │                                                       │
     │  ┌─────────────────┐         ┌──────────────────────┐│
     │  │  Driving Adapter│         │  Driven Adapter      ││
     │  │ [HEX.Adapter    │         │  [HEX.Adapter.Driven]││
     │  │   .Driving]     │         │                      ││
     │  └────────┬────────┘         └──────────▲───────────┘│
     │           │ calls                        │ implements  │
     │           ▼                              │             │
     │  ┌────────────────────────────────────────────────┐   │
     │  │              Application Core  [HEX.Core]      │   │
     │  │                                                │   │
     │  │  ┌──────────────────┐  ┌────────────────────┐  │   │
     │  │  │  Driving Port    │  │  Driven Port        │  │   │
     │  │  │ [HEX.Port.Driving│  │ [HEX.Port.Driven]  │  │   │
     │  │  │  + HEX.Core]    │  │  + HEX.Core]        │  │   │
     │  │  └──────────────────┘  └────────────────────┘  │   │
     │  └────────────────────────────────────────────────┘   │
     └──────────────────────────────────────────────────────┘

Driving side (left):  external actor → Driving Adapter → Driving Port → Core
Driven side (right):  Core → Driven Port → Driven Adapter → external system
```

**Driving side (left):** external actors (HTTP, CLI, tests) call the
application via a **driving port** — an interface declared in the core and
implemented by the core. A **driving adapter** translates the external call
into a driving port invocation.

**Driven side (right):** the application calls **driven ports** — interfaces
declared in the core that the application depends on. A **driven adapter**
implements a driven port using external technology (database, message bus,
email service).

The core declares both port kinds. It never references an adapter.

### Roles

| Role | Side | Meaning |
|---|---|---|
| `HEX.Core` | Centre | Application core: domain model, use case logic, port declarations |
| `HEX.Port.Driving` | Left | Interface declared in core; entry point for driving adapters |
| `HEX.Port.Driven` | Right | Interface declared in core; implemented by driven adapters |
| `HEX.Adapter.Driving` | Left | Translates external input into a driving port call |
| `HEX.Adapter.Driven` | Right | Implements a driven port using external technology |

Port roles are additive with `HEX.Core` — a port interface lives inside the
core and carries both `HEX.Core` and its port direction role:

```csharp
CombinationRule("HEX-Core-Port-Driving",
    "HEX.Core", "HEX.Port.Driving", BrickCombinationKind.Additive)
CombinationRule("HEX-Core-Port-Driven",
    "HEX.Core", "HEX.Port.Driven", BrickCombinationKind.Additive)
```

Adapter roles are incompatible with core — adapters must never live in the
core:

```csharp
CombinationRule("HEX-Core-Adapter-Driving",
    "HEX.Core", "HEX.Adapter.Driving", BrickCombinationKind.Incompatible)
CombinationRule("HEX-Core-Adapter-Driven",
    "HEX.Core", "HEX.Adapter.Driven", BrickCombinationKind.Incompatible)
```

Driving and driven adapter roles are exclusive — an adapter has one direction:

```csharp
CombinationRule("HEX-Adapter-sides",
    "HEX.Adapter.Driving", "HEX.Adapter.Driven", BrickCombinationKind.Exclusive)
```

### Interoperability with DDD Pack

DDD tactical patterns are additive with `HEX.Core` — the core contains the
domain model:

```csharp
CombinationRule("HEX-Core-AggregateRoot",
    "HEX.Core", "AggregateRoot", BrickCombinationKind.Additive)
CombinationRule("HEX-Core-Entity",
    "HEX.Core", "Entity", BrickCombinationKind.Additive)
CombinationRule("HEX-Core-ValueObject",
    "HEX.Core", "ValueObject", BrickCombinationKind.Additive)
CombinationRule("HEX-Core-DomainEvent",
    "HEX.Core", "DomainEvent", BrickCombinationKind.Additive)
CombinationRule("HEX-Core-DomainService",
    "HEX.Core", "DomainService", BrickCombinationKind.Additive)
```

### Interoperability with Clean Architecture Pack

Hexagonal and Clean Architecture are compatible. `HEX.Core` maps to
`CA.Core` + `CA.UseCases`; adapters map to `CA.Adapters`:

```csharp
CombinationRule("HEX-CA-Core",
    "HEX.Core", "CA.Core", BrickCombinationKind.Additive)
CombinationRule("HEX-CA-UseCases",
    "HEX.Core", "CA.UseCases", BrickCombinationKind.Additive)
CombinationRule("HEX-Driving-CA-Adapters",
    "HEX.Adapter.Driving", "CA.Adapters", BrickCombinationKind.Additive)
CombinationRule("HEX-Driven-CA-Adapters",
    "HEX.Adapter.Driven", "CA.Adapters", BrickCombinationKind.Additive)
```

### Naming Conventions

```csharp
[Role("HEX.Port.Driving")]
[NameConvention("I", NamePosition.Prefix)]
[NameConvention("Port", NamePosition.Suffix,
    Reason = "Driving ports must be identifiable by name")]
public interface IDrivingPort { }

[Role("HEX.Port.Driven")]
[NameConvention("I", NamePosition.Prefix)]
[NameConvention("Port", NamePosition.Suffix,
    Reason = "Driven ports must be identifiable by name")]
public interface IDrivenPort { }

[Role("HEX.Adapter.Driving")]
[NameConvention("Adapter", NamePosition.Suffix,
    Reason = "Driving adapters must be identifiable by name")]
public interface IDrivingAdapter { }

[Role("HEX.Adapter.Driven")]
[NameConvention("Adapter", NamePosition.Suffix,
    Reason = "Driven adapters must be identifiable by name")]
public interface IDrivenAdapter { }
```

The `Port` suffix is the strict default. Teams that prefer domain-aligned
names (`IOrderService` for a driving port, `IOrderRepository` for a driven
port) may override the suffix convention in their policy config. The role
and direction remain enforced regardless of naming convention override.

### Default Dependency Policy

```json
{
  "name": "HexagonalArchitecture",
  "defaultDecision": "Allow",
  "rules": [
    {
      "name": "HEX-Core-no-Adapter-Driving",
      "description": "Core must not know about driving adapters",
      "sourceRole": "HEX.Core",
      "targetRole": "HEX.Adapter.Driving",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "HEX-Core-no-Adapter-Driven",
      "description": "Core must not depend on driven adapter implementations",
      "sourceRole": "HEX.Core",
      "targetRole": "HEX.Adapter.Driven",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "HEX-Port-Driving-no-Adapter",
      "description": "Driving port interfaces must not reference adapters",
      "sourceRole": "HEX.Port.Driving",
      "targetRole": "HEX.Adapter.Driving",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "HEX-Port-Driven-no-Adapter",
      "description": "Driven port interfaces must not reference adapters",
      "sourceRole": "HEX.Port.Driven",
      "targetRole": "HEX.Adapter.Driven",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "HEX-Adapter-Driving-requires-Port",
      "description": "Each driving adapter must call at least one driving port",
      "sourceRole": "HEX.Adapter.Driving",
      "targetRole": "HEX.Port.Driving",
      "decision": "Require",
      "scope": "Type",
      "severity": "Error"
    },
    {
      "name": "HEX-Adapter-Driven-requires-Port",
      "description": "Each driven adapter must implement at least one driven port",
      "sourceRole": "HEX.Adapter.Driven",
      "targetRole": "HEX.Port.Driven",
      "dependencyKind": "InterfaceImplementation",
      "decision": "Require",
      "scope": "Type",
      "severity": "Error"
    },
    {
      "name": "HEX-Adapter-Driven-no-Driving-Port",
      "description": "Driven adapters must not initiate calls via driving ports",
      "sourceRole": "HEX.Adapter.Driven",
      "targetRole": "HEX.Port.Driving",
      "decision": "Deny",
      "severity": "Warning"
    }
  ]
}
```

The two `Require` rules enforce structural completeness: a driving adapter
that never calls a driving port bypasses the hexagon entirely; a driven
adapter that implements no driven port serves no purpose and cannot be reached
from the core.
