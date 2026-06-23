# NMolecules.Bricks — Use Cases

Layer: 3 — Use Cases  
Depends on: Layer 2 (Building Blocks), Layer 1 (Core)  
Status: March 2026

Authoritative model reference: `../foundational-concept.md`.
This Layer 3 document shows target-shape usage scenarios on top of Layer 1 and
Layer 2. It is not a statement that every shown attribute, package, or
diagnostic is part of the currently shipped baseline.

Related layer-specific use case documents:

- Layer 1 Core: `../layer1/bricks-layer1-core-use-cases.md`
- Layer 2 Building Blocks: `../layer2/bricks-layer2-building-block-use-cases.md`

This document contains **concrete use case scenarios** for `NMolecules.Bricks`.
Each scenario applies one or more Layer 2 building blocks to a specific
structural problem. No new concepts are introduced here — only applications of
what Layer 1 and Layer 2 define.

**Document scope:** illustrative, validating, and onboarding content. If a
reader asks "how do I use Bricks for X?", this document answers that question.

---

## Use Case Index

| # | Title | Building Blocks Used |
|---|---|---|
| UC-01 | Domain Event naming rule | Naming Conventions |
| UC-02 | Naming rule via alias for external type | Naming Conventions |
| UC-03 | Conflicting naming rules with explicit override | Naming Conventions |
| UC-04 | Override for generated type via assembly alias | Naming Conventions |
| UC-05 | Layered architecture dependency enforcement | Architecture Pack |
| UC-06 | DDD aggregate root cardinality contract | DDD Pack + Member Cardinality |
| UC-07 | Combined: DDD naming + layer rule | DDD Pack + Naming Conventions |
| UC-08 | KI-generated code safety net | All building blocks (passive) |

**DDD Use Cases** are in a separate document: `bricks-layer3-usecases-ddd.md`

**Clean Architecture Use Cases** are in a separate document:
`bricks-layer3-usecases-clean-architecture.md`

**Hexagonal Architecture Use Cases** are in a separate document:
`bricks-layer3-usecases-hexagonal.md`

| # | Title | Building Blocks |
|---|---|---|
| UC-H01 | Port and adapter role assignment | Hexagonal Pack |
| UC-H02 | Core isolation rule | Hexagonal Pack |
| UC-H03 | Naming conventions for ports and adapters | Hexagonal Pack + Naming Conventions |
| UC-H04 | Require: driving adapter must call a driving port | Hexagonal Pack |
| UC-H05 | Require: driven adapter must implement a driven port | Hexagonal Pack |
| UC-H06 | Hexagonal + DDD in the core | Hexagonal Pack + DDD Pack |
| UC-H07 | Multiple adapters for one port | Hexagonal Pack |
| UC-H08 | Full example: Order placement | All Hexagonal building blocks |
| UC-H09 | AI-generated code safety net | All building blocks (passive) |

| # | Title | Building Blocks |
|---|---|---|
| UC-C01 | Ring role assignment | Clean Architecture Pack |
| UC-C02 | Dependency Rule enforcement | Clean Architecture Pack |
| UC-C03 | Naming conventions per ring | Clean Architecture Pack + Naming Conventions |
| UC-C04 | Use case input/output boundary contracts | Clean Architecture Pack + Member Cardinality |
| UC-C05 | Presenter pattern | Clean Architecture Pack |
| UC-C06 | Gateway as boundary to persistence | Clean Architecture Pack + DDD Pack |
| UC-C07 | Clean Architecture + DDD in the Entities ring | Clean Architecture Pack + DDD Pack |
| UC-C08 | Full example: Order placement | All Clean Architecture building blocks |
| UC-C09 | AI-generated code safety net | All building blocks (passive) |

| # | Title | Building Blocks Used |
|---|---|---|
| UC-D01 | DDD role assignment for all tactical building blocks | DDD Pack |
| UC-D02 | Naming conventions for all DDD building blocks | DDD Pack + Naming Conventions |
| UC-D03 | Aggregate root identity contract | DDD Pack + Member Cardinality |
| UC-D04 | Value object immutability contract | DDD Pack + Member Cardinality |
| UC-D05 | Dependency rules within a bounded context | DDD Pack |
| UC-D06 | Repository access rules | DDD Pack |
| UC-D07 | Domain event flow rules | DDD Pack + Events Pack |
| UC-D08 | Bounded context boundary enforcement | DDD Pack + Structural Core Pack |
| UC-D09 | Full bounded context: Order Management | All DDD building blocks |
| UC-D10 | DDD + AI-generated code | All DDD building blocks (passive) |

---

## UC-01: Domain Event Naming Rule

**Problem:** Every class that implements `IDomainEvent` must end its name with
`DomainEvent`. Violations from both developer-written and AI-generated code
must be caught in the IDE and at build time.

**Building block used:** Naming Conventions (`NameConventionAttribute`,
`XMoleculesBricks0010`)

### Setup

```csharp
// Domain layer — convention declared on the interface
[NameConvention("DomainEvent", NamePosition.Suffix,
    Reason = "Domain event implementors must be identifiable by name " +
             "without inspecting the type hierarchy")]
public interface IDomainEvent { }
```

### Compliant Code

```csharp
public class OrderPlacedDomainEvent : IDomainEvent { }       // ✓
public class PaymentReceivedDomainEvent : IDomainEvent { }   // ✓
public class InventoryAdjustedDomainEvent : IDomainEvent { } // ✓
```

### Violations

```csharp
// XMoleculesBricks0010:
// 'OrderPlaced' implements IDomainEvent but does not end with 'DomainEvent'.
public class OrderPlaced : IDomainEvent { }

// XMoleculesBricks0010:
// 'DomainOrderEvent' implements IDomainEvent but does not end with 'DomainEvent'.
// (suffix check is position-sensitive; Contains would allow this)
public class DomainOrderEvent : IDomainEvent { }
```

### Expected Diagnostic Message

```
'OrderPlaced' implements 'IDomainEvent' but its name does not end with
'DomainEvent'.
Convention source: IDomainEvent.
Reason: Domain event implementors must be identifiable by name without
inspecting the type hierarchy.
[XMoleculesBricks0010]
```

---

## UC-02: Naming Rule via Alias for External Type

**Problem:** The project uses MediatR's `INotification` as the base for all
domain events. `INotification` is in an external package and cannot be
annotated. The naming rule should still apply to all notification types that
are also domain events.

**Building block used:** Naming Conventions (`NameConventionAliasAttribute`)

### Setup

```csharp
// IDomainEvent already carries the convention (UC-01)
[NameConvention("DomainEvent", NamePosition.Suffix)]
public interface IDomainEvent { }

// Bridge interface: imports the naming convention from IDomainEvent.
// MediatR.INotification cannot be annotated directly.
[NameConventionAlias(typeof(IDomainEvent),
    Reason = "Notifications that are domain events inherit the DomainEvent " +
             "naming rule via this bridge interface")]
public interface IDomainEventNotification : MediatR.INotification, IDomainEvent { }
```

### Compliant Code

```csharp
// Implements IDomainEventNotification → convention applies transitively
public class OrderShippedDomainEvent : IDomainEventNotification { } // ✓
```

### Invalid Alias Setup (Diagnostic 0012)

```csharp
// IFoo carries no NameConventionAttribute.
// XMoleculesBricks0012:
// 'NameConventionAlias' on 'IBar' references 'IFoo',
// but 'IFoo' carries no NameConventionAttribute. The alias has no effect.
[NameConventionAlias(typeof(IFoo))]
public interface IBar { }
```

---

## UC-03: Conflicting Naming Rules with Explicit Override

**Problem:** A type implements both `IDomainEvent` (suffix "DomainEvent") and
`IIntegrationEvent` (suffix "IntegrationEvent"). These two conventions conflict.
The type is primarily a domain event and the integration event convention should
be suppressed.

**Building block used:** Naming Conventions (`NameConventionOverrideAttribute`,
`XMoleculesBricks0011`)

### Setup

```csharp
[NameConvention("DomainEvent", NamePosition.Suffix)]
public interface IDomainEvent { }

[NameConvention("IntegrationEvent", NamePosition.Suffix)]
public interface IIntegrationEvent { }
```

### Conflict Without Override

```csharp
// XMoleculesBricks0011:
// 'OrderPublished' has conflicting naming conventions:
//   - IDomainEvent requires suffix 'DomainEvent'
//   - IIntegrationEvent requires suffix 'IntegrationEvent'
// These conventions cannot be satisfied simultaneously.
// Add [NameConventionOverride] to resolve this conflict explicitly.
public class OrderPublished : IDomainEvent, IIntegrationEvent { }
```

### Resolved with Override

```csharp
// ✓ — explicit resolution, no violation
[NameConventionOverride(typeof(IIntegrationEvent),
    NameConventionOverrideBehavior.Suppress,
    Reason = "This event crosses the integration boundary via the event bus " +
             "but is semantically a domain event. The integration event naming " +
             "convention is intentionally suppressed here.")]
public class PaymentReceivedDomainEvent : IDomainEvent, IIntegrationEvent { }
```

### Dead Override Detection

```csharp
// PaymentConfirmed does not implement IIntegrationEvent.
// XMoleculesBricks0013:
// 'NameConventionOverride' on 'PaymentConfirmedDomainEvent' suppresses
// 'IIntegrationEvent', but 'PaymentConfirmedDomainEvent' does not implement
// or inherit 'IIntegrationEvent'. The override has no effect.
[NameConventionOverride(typeof(IIntegrationEvent),
    NameConventionOverrideBehavior.Suppress,
    Reason = "...")]
public class PaymentConfirmedDomainEvent : IDomainEvent { }
```

---

## UC-04: Override for Generated Type via Assembly Alias

**Problem:** A code generator emits a type `GeneratedShipmentEvent` that
implements both `IDomainEvent` and `IIntegrationEvent`. The type cannot be
annotated directly (it is generated). The integration event naming convention
should be suppressed via an assembly-level alias.

**Building block used:** Naming Conventions
(`NameConventionOverrideAliasAttribute`)

### Setup

```csharp
// In the policy declaration file (or in AssemblyInfo.cs):
[assembly: NameConventionOverrideAlias(
    targetType: typeof(GeneratedShipmentEvent),
    suppressedSource: typeof(IIntegrationEvent),
    NameConventionOverrideBehavior.Suppress,
    Reason = "Generated type follows DomainEvent naming by architecture " +
             "decision ADR-2026-04. Integration event naming suppressed.")]
```

### Result

`GeneratedShipmentEvent` is evaluated as if it carries the `IDomainEvent`
naming convention only. No violation for the integration event convention.
The `Reason` field references the Architecture Decision Record for traceability.

---

## UC-05: Layered Architecture Dependency Enforcement

**Problem:** Enforce that `DomainLayer` types never depend on
`InfrastructureLayer` types. Violations from any source (developer or
AI-generated code) must surface in the IDE and fail the build.

**Building block used:** Architecture Pack

### Setup

```csharp
// Assign roles to assemblies (or via external config):
[assembly: Role("DomainLayer")]   // in MyApp.Domain
[assembly: Role("ApplicationLayer")]  // in MyApp.Application
[assembly: Role("InfrastructureLayer")] // in MyApp.Infrastructure

// Policy (typically in a central policy config file):
{
  "name": "LayeredArchitecture",
  "defaultDecision": "Allow",
  "rules": [
    {
      "name": "Domain-no-infra",
      "sourceRole": "DomainLayer",
      "targetRole": "InfrastructureLayer",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "Application-no-infra",
      "sourceRole": "ApplicationLayer",
      "targetRole": "InfrastructureLayer",
      "decision": "Deny",
      "severity": "Error"
    }
  ]
}
```

### Violation

```csharp
// In MyApp.Domain (role: DomainLayer)
// XMoleculesBricks0001:
// 'OrderService' (role: DomainLayer) depends on 'SqlOrderRepository'
// (role: InfrastructureLayer). This dependency is denied by rule 'Domain-no-infra'.
public class OrderService
{
    private readonly SqlOrderRepository _repo; // ← violation
}
```

### Compliant Pattern

```csharp
// In MyApp.Domain
public class OrderService
{
    private readonly IOrderRepository _repo; // IOrderRepository is in DomainLayer ✓
}

// In MyApp.Infrastructure (role: InfrastructureLayer)
public class SqlOrderRepository : IOrderRepository { } // ✓ infra implements domain contract
```

---

## UC-06: DDD Aggregate Root Cardinality Contract

**Problem:** Every `AggregateRoot` must declare exactly one identity member
(a property named `Id` of the aggregate's `Identity` type).

**Building blocks used:** DDD Pack (role `AggregateRoot`) + Member Cardinality
Contracts (`RequireExactlyOneMemberAttribute`)

### Setup

```csharp
[Role("AggregateRoot")]
[RequireExactlyOneMember("Id",
    Reason = "Every aggregate root must have a single identity property")]
public abstract class AggregateRoot<TId> where TId : IIdentity
{
    public abstract TId Id { get; }
}
```

### Violation

```csharp
// XMoleculesBricks0003:
// 'Order' is annotated with [RequireExactlyOneMember("Id")] (via AggregateRoot)
// but does not declare 'Id'.
public class Order : AggregateRoot<OrderId>
{
    // no Id property overridden — violation
}
```

### Compliant

```csharp
public class Order : AggregateRoot<OrderId>
{
    public override OrderId Id { get; } // ✓
}
```

---

## UC-07: Combined DDD Naming + Layer Rule

**Problem:** All types with role `DomainEvent` must end their name with
`DomainEvent` (naming convention), and must not depend on anything in
`InfrastructureLayer` (dependency rule).

**Building blocks used:** DDD Pack (Events Pack) + Architecture Pack +
Naming Conventions

### Setup

```csharp
// Events Pack: role DomainEvent
// Architecture Pack: rule Domain-no-infra
// Naming convention on the marker interface:
[NameConvention("DomainEvent", NamePosition.Suffix)]
[Role("DomainEvent")]
public interface IDomainEvent { }
```

### Result

Any class implementing `IDomainEvent` is evaluated against:

1. **Step 4b** (Naming): name must end with `DomainEvent` → `0010` if not
2. **Step 5** (Permission): must not depend on `InfrastructureLayer` → `0001` if it does

Both violations may be present simultaneously on the same type. They are
independent and are reported as separate diagnostics.

### Compliant

```csharp
// ✓ correct name, no infrastructure dependencies
public class OrderPlacedDomainEvent : IDomainEvent
{
    public OrderId OrderId { get; }    // value object from DomainLayer
    public Money Amount { get; }       // value object from DomainLayer
}
```

---

## UC-08: KI-Generated Code Safety Net

**Problem:** The team uses an AI code generator (e.g. GitHub Copilot, Cursor,
or a custom LLM pipeline). Generated code may introduce naming violations,
forbidden layer dependencies, or missing cardinality contracts. The team wants
deterministic detection regardless of whether the code was written by a
developer or generated.

**Building blocks used:** all active building blocks (passive detection, no
generator-specific setup)

### How it works

No generator-specific configuration is needed for the passive safety net
(Layer 2 integration Stufe 1). The Roslyn analyzer runs on all C# code in the
build — generated code included. Violations surface in the IDE as the developer
accepts generated code and in the build pipeline before merge.

### Example: generator produces a naming violation

A code generator emits:

```csharp
// Generated by Copilot suggestion
public class OrderCreated : IDomainEvent
{
    public Guid OrderId { get; init; }
}
```

The analyzer immediately flags `XMoleculesBricks0010`:

```
'OrderCreated' implements 'IDomainEvent' but its name does not end with
'DomainEvent'.
Convention source: IDomainEvent.
[XMoleculesBricks0010]
```

The developer sees this in the IDE before accepting the suggestion and can
either rename the generated class or reject the suggestion.

### Example: generator produces a layer violation

```csharp
// Generated code in MyApp.Domain (role: DomainLayer)
public class PricingService
{
    // Generator pulled in a concrete infrastructure type
    private readonly SqlPricingRepository _repo; // ← XMoleculesBricks0001
}
```

### Progressive integration (Phase 2+)

Phase 2 adds a machine-readable policy export that the generator can consume
as context. With this, the generator knows which naming conventions and layer
rules are active before generating code — reducing violations rather than
detecting them after.

Phase 3 adds a Generate→Analyze→Fix loop where the generator receives
`BrickViolation` records after generation and corrects the output before
presenting it to the developer.

Both phases are additive. The passive safety net (this use case) remains the
foundation and requires no generator-specific configuration.

---

## Validation Summary

| UC | Core concept validated | Building block validated |
|---|---|---|
| UC-01 | Element constraint pipeline slot | `NameConventionAttribute` + `0010` |
| UC-02 | Alias mechanism | `NameConventionAliasAttribute` + `0012` |
| UC-03 | Conflict detection + override | `NameConventionOverrideAttribute` + `0011` + `0013` |
| UC-04 | Assembly-level alias override | `NameConventionOverrideAliasAttribute` |
| UC-05 | Permission evaluation + DefaultDecision | Architecture Pack roles + `0001` |
| UC-06 | Element constraint + cardinality | `RequireExactlyOneMemberAttribute` + `0003` |
| UC-07 | Multi-building-block composition | Events Pack + Architecture Pack + Naming |
| UC-08 | Determinism across code origins | All active building blocks (passive) |

All use cases are expressible with Layer 1 core concepts and Layer 2 building
blocks. No use case requires modifications to the core. New scenarios should
follow this pattern: identify the Layer 2 building block, compose it with
existing roles and rules, document the expected violations.
