# NMolecules.Bricks — Use Cases: Domain-Driven Design

Layer: 3 — Use Cases  
Depends on: Layer 2 (Building Blocks), Layer 1 (Core)  
Status: June 27, 2026

Authoritative model reference: `../foundational-concept.md`.
This document describes target-model DDD scenarios on top of Layer 1 and Layer
2. Examples here may include future building blocks that are not yet part of
the shipped baseline.

This document maps the full DDD tactical pattern set onto Bricks building
blocks. It is a self-contained extension of the main use case catalogue.
Use cases UC-D01 through UC-D10 cover the complete DDD model: building block
roles, naming conventions, dependency rules, cardinality contracts, bounded
context boundaries, and event flows.

Shipped baseline note: strict DDD naming conventions in this document are
target-model examples. The current analyzer-backed DDD sample uses specialized
role attributes with `RoleAliasAttribute` and member-cardinality contracts. The
current naming-oriented shipped sample surface is the rule-filter family under
`nmolecules.brick-examples/samples/bricks/implementation-samples/rule-filters`.

**Document scope:** DDD-specific scenarios only. Cross-cutting concerns
(AI safety net, layer architecture) are in the main use case document.

---

## DDD Use Case Index

| # | Title | Building Blocks |
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

## Conceptual Map: DDD Building Blocks to Bricks Roles

Before the individual use cases, this table shows how DDD tactical patterns
map to Bricks DDD Pack roles and which constraints apply to each.

| DDD Pattern | Bricks Role | Naming Convention | Cardinality Contract | Key Dependency Rules |
|---|---|---|---|---|
| Aggregate Root | `AggregateRoot` | Suffix optional (team decides) | Exactly one explicit identity marker | May not depend on `Repository` directly |
| Entity | `Entity` | Suffix optional | Exactly one explicit identity marker | May not depend on `Repository` |
| Value Object | `ValueObject` | Suffix optional | No settable properties; no `Id` | No dependencies on `Entity` or `AggregateRoot` |
| Repository (interface) | `Repository` | Prefix `I`, Suffix `Repository` | — | Lives in domain; implemented in infrastructure |
| Repository (impl) | `Repository` + `InfrastructureService` | Suffix `Repository` | — | Implements a domain `Repository` interface |
| Domain Service | `DomainService` | Suffix `DomainService` | — | No dependency on `Repository` implementations, no `ApplicationService` |
| Application Service | `ApplicationService` | Suffix `ApplicationService` or `Service` | — | May use `Repository`, `DomainService`, `DomainEventPublisher` |
| Factory | `Factory` | Suffix `Factory` | — | May depend on `Entity`, `ValueObject`, `AggregateRoot` |
| Domain Event | `DomainEvent` | Suffix `DomainEvent` | No setters (events are immutable) | No dependency on services or repositories |
| Domain Event Handler | `DomainEventHandler` | Suffix `DomainEventHandler` | — | May use `Repository`, `ApplicationService` |
| Domain Event Publisher | `DomainEventPublisher` | — | — | Interface in domain; impl in infrastructure |
| Identity | `Identity` | Suffix `Id` | Single value property | No dependencies on other building blocks |
| Bounded Context | `BoundedContext` | — | — | No direct type-level dependency across contexts |
| Module | `Module` | — | — | Grouping only; no structural rule |

---

## UC-D01: DDD Role Assignment for All Tactical Building Blocks

**Problem:** Assign Bricks roles to all DDD tactical types in a project so
that dependency rules and naming conventions can be enforced consistently.

**Building block used:** DDD Pack

### Marker Interfaces (domain assembly)

The cleanest approach is role assignment via marker interfaces. Each DDD
pattern gets a typed marker interface that carries both the Bricks role and
any applicable naming convention or cardinality contract.

```csharp
// ─── Aggregate Root ────────────────────────────────────────────────────────

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class IdentityMemberAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class)]
[RoleAlias("AggregateRoot")]
[RequireExactlyOneMember(typeof(IdentityMemberAttribute))]
public sealed class AggregateRootAttribute : RoleAttribute
{
    public AggregateRootAttribute() : base("AggregateRoot")
    {
    }
}

// ─── Entity ────────────────────────────────────────────────────────────────

[AttributeUsage(AttributeTargets.Class)]
[RoleAlias("Entity")]
[RequireExactlyOneMember(typeof(IdentityMemberAttribute))]
public sealed class EntityAttribute : RoleAttribute
{
    public EntityAttribute() : base("Entity")
    {
    }
}

// ─── Value Object ──────────────────────────────────────────────────────────

[Role("ValueObject")]
public abstract class ValueObject
{
    // No Id. Equality is structural.
    protected abstract IEnumerable<object?> GetEqualityComponents();
}

// ─── Identity ──────────────────────────────────────────────────────────────

[Role("Identity")]
[NameConvention("Id", NamePosition.Suffix,
    Reason = "Identity types must be identifiable by name")]
public interface IIdentity { }

// ─── Repository Interface ──────────────────────────────────────────────────

[Role("Repository")]
[NameConvention("Repository", NamePosition.Suffix,
    Reason = "Repository interfaces must be identifiable by name")]
[NameConvention("I", NamePosition.Prefix,
    Reason = "Repository interfaces follow the I-prefix convention")]
public interface IRepository<TAggregateRoot, TId>
    where TAggregateRoot : AggregateRoot<TId>
    where TId : IIdentity { }

// ─── Domain Service ───────────────────────────────────────────────────────

[Role("DomainService")]
[NameConvention("DomainService", NamePosition.Suffix,
    Reason = "Domain services must be identifiable by name")]
public interface IDomainService { }

// ─── Application Service ──────────────────────────────────────────────────

[Role("ApplicationService")]
[NameConvention("ApplicationService", NamePosition.Suffix,
    Reason = "Application services must be identifiable by name")]
public interface IApplicationService { }

// ─── Factory ──────────────────────────────────────────────────────────────

[Role("Factory")]
[NameConvention("Factory", NamePosition.Suffix,
    Reason = "Factories must be identifiable by name")]
public interface IFactory<T> { }

// ─── Domain Event ─────────────────────────────────────────────────────────

[Role("DomainEvent")]
[NameConvention("DomainEvent", NamePosition.Suffix,
    Reason = "Domain events must be identifiable by name")]
public interface IDomainEvent { }

// ─── Domain Event Handler ─────────────────────────────────────────────────

[Role("DomainEventHandler")]
[NameConvention("DomainEventHandler", NamePosition.Suffix,
    Reason = "Domain event handlers must be identifiable by name")]
public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent { }

// ─── Domain Event Publisher ────────────────────────────────────────────────

[Role("DomainEventPublisher")]
public interface IDomainEventPublisher { }
```

### Role Assignment via Alias (for external base classes)

If the project uses an external base class that cannot be annotated:

```csharp
[RoleAlias(typeof(ExternalValueObjectBase), "ValueObject",
    Reason = "ExternalValueObjectBase is the shared library's value object base")]
public static class ExternalLibraryAliases { }
```

---

## UC-D02: Naming Conventions for All DDD Building Blocks

**Problem:** Enforce that each DDD type is identifiable by its name without
inspecting the type hierarchy.

**Building block used:** DDD Pack + Naming Conventions

The naming conventions are declared on the marker interfaces in UC-D01. This
use case documents what the analyzer enforces and which names are compliant.

### Naming Rules Summary

| Marker | Pattern | Position | Example compliant name |
|---|---|---|---|
| `IIdentity` | `Id` | Suffix | `OrderId`, `CustomerId`, `ProductId` |
| `IRepository<,>` | `I` | Prefix | `IOrderRepository` |
| `IRepository<,>` | `Repository` | Suffix | `IOrderRepository` |
| `IDomainService` | `DomainService` | Suffix | `PricingDomainService` |
| `IApplicationService` | `ApplicationService` | Suffix | `PlaceOrderApplicationService` |
| `IFactory<>` | `Factory` | Suffix | `OrderFactory`, `ProductFactory` |
| `IDomainEvent` | `DomainEvent` | Suffix | `OrderPlacedDomainEvent` |
| `IDomainEventHandler<>` | `DomainEventHandler` | Suffix | `OrderPlacedDomainEventHandler` |

`AggregateRoot`, `Entity`, and `ValueObject` carry no enforced naming suffix
by default — teams may add their own conventions on top. The base classes carry
no `NameConventionAttribute` in the default DDD Pack.

### Compliant Names

```csharp
public class OrderId : IIdentity { }                           // ✓ suffix Id
public interface IOrderRepository
    : IRepository<Order, OrderId> { }                          // ✓ prefix I, suffix Repository
public class PricingDomainService : IDomainService { }         // ✓ suffix DomainService
public class PlaceOrderApplicationService
    : IApplicationService { }                                  // ✓ suffix ApplicationService
public class OrderFactory : IFactory<Order> { }               // ✓ suffix Factory
public class OrderPlacedDomainEvent : IDomainEvent { }        // ✓ suffix DomainEvent
public class OrderPlacedDomainEventHandler
    : IDomainEventHandler<OrderPlacedDomainEvent> { }         // ✓ suffix DomainEventHandler
```

### Violations

```csharp
// XMoleculesBricks0020 — missing suffix
public class OrderIdentifier : IIdentity { }      // must end with 'Id'
public class OrderRepo
    : IRepository<Order, OrderId> { }             // must end with 'Repository'
public class PricingService : IDomainService { }  // must end with 'DomainService'
public class PlaceOrder
    : IApplicationService { }                     // must end with 'ApplicationService'
public class OrderCreator : IFactory<Order> { }   // must end with 'Factory'
public class OrderCreated : IDomainEvent { }      // must end with 'DomainEvent'
```

---

## UC-D03: Aggregate Root Identity Contract

**Problem:** Every concrete aggregate root must declare exactly one explicit
identity member. Missing or duplicated identity markers must be caught at design
time.

**Building block used:** DDD Pack + Member Cardinality (`RequireExactlyOneMember`)

The shipped Bricks pattern is marker-based. A specialized role attribute carries
`RequireExactlyOneMember(typeof(IdentityMemberAttribute))`; the concrete type
marks the one identity member with `[IdentityMember]`.

### Violation

```csharp
// XMoleculesBricks0003:
// 'Order' is marked as an AggregateRoot but no member is marked
// with [IdentityMember].
[AggregateRoot]
public class Order
{
    public string Description { get; set; } = "";
}
```

### Compliant

```csharp
[AggregateRoot]
public class Order
{
    [IdentityMember]
    public OrderId OrderNumber { get; }  // ✓ name is domain-specific

    public Order(OrderId orderNumber) => OrderNumber = orderNumber;
}
```

---

## UC-D04: Value Object Immutability Contract

**Problem:** Value objects must be immutable. No public setters allowed.
The analyzer must catch value objects with mutable state.

**Building block used:** DDD Pack + Member Cardinality

Add a cardinality contract to the `ValueObject` base class that enforces
zero public setter members:

```csharp
[Role("ValueObject")]
[RequireMemberCount("public setter", 0,
    Reason = "Value objects must be immutable — no public setters allowed")]
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();
}
```

### Violation

```csharp
// XMoleculesBricks0005:
// 'Money' inherits ValueObject which requires 0 public setter members,
// but 'Money' declares 1 (Amount).
public class Money : ValueObject
{
    public decimal Amount { get; set; }  // ← mutable setter, violation
    public string Currency { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
        => [Amount, Currency];
}
```

### Compliant

```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }    // ✓ init-only or readonly
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
        => [Amount, Currency];
}
```

---

## UC-D05: Dependency Rules Within a Bounded Context

**Problem:** Enforce the correct dependency direction between DDD building
blocks within a bounded context. A domain service must not depend on
infrastructure; entities must not depend on repositories.

**Building block used:** DDD Pack

### Policy

```json
{
  "name": "DddDependencyRules",
  "defaultDecision": "Allow",
  "rules": [
    {
      "name": "DomainService-no-Repository-impl",
      "description": "Domain services depend on repository interfaces, not implementations",
      "sourceRole": "DomainService",
      "targetRole": "InfrastructureService",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "Entity-no-Repository",
      "description": "Entities must not depend on repositories",
      "sourceRole": "Entity",
      "targetRole": "Repository",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "AggregateRoot-no-Repository",
      "description": "Aggregate roots must not depend on repositories",
      "sourceRole": "AggregateRoot",
      "targetRole": "Repository",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "ValueObject-no-services",
      "description": "Value objects are pure data; no service dependencies",
      "sourceRole": "ValueObject",
      "targetRole": "DomainService",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "ValueObject-no-ApplicationService",
      "sourceRole": "ValueObject",
      "targetRole": "ApplicationService",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "DomainEvent-no-services",
      "description": "Domain events are pure data; no service or repository dependencies",
      "sourceRole": "DomainEvent",
      "targetRole": "DomainService",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "DomainEvent-no-Repository",
      "sourceRole": "DomainEvent",
      "targetRole": "Repository",
      "decision": "Deny",
      "severity": "Error"
    }
  ]
}
```

### Violations

```csharp
// XMoleculesBricks0001 — DomainService depends on infrastructure impl
public class PricingDomainService : IDomainService
{
    // ← depends on infrastructure impl, not interface
    private readonly SqlPriceRepository _prices;
}

// XMoleculesBricks0001 — Entity depends on repository
public class Order : AggregateRoot<OrderId>
{
    public override OrderId Id { get; }
    private readonly IOrderRepository _repo; // ← aggregate must not hold repository
}

// XMoleculesBricks0001 — ValueObject depends on service
public class DiscountedPrice : ValueObject
{
    private readonly PricingDomainService _svc; // ← value objects are pure
    ...
}
```

### Compliant

```csharp
// DomainService depends on repository interface (in domain)
public class PricingDomainService : IDomainService
{
    private readonly IPriceRepository _prices; // ✓ interface, role Repository
    public PricingDomainService(IPriceRepository prices) => _prices = prices;
}

// Aggregate root holds only value objects and raises events
public class Order : AggregateRoot<OrderId>
{
    public override OrderId Id { get; }
    public Money TotalAmount { get; private set; } = Money.Zero; // ✓ ValueObject

    public void Place()
    {
        Raise(new OrderPlacedDomainEvent(Id, TotalAmount)); // ✓ raises event
    }
}
```

---

## UC-D06: Repository Access Rules

**Problem:** Only application services may resolve repositories. Domain
services must use repository interfaces, not call repositories directly with
complex queries. The repository implementation lives in infrastructure.

**Building block used:** DDD Pack

### Additional Policy Rules

```json
{
  "rules": [
    {
      "name": "Repository-impl-in-infrastructure",
      "description": "Repository implementations must live in infrastructure",
      "sourceRole": "Repository",
      "targetRole": "DomainLayer",
      "decision": "Allow"
    },
    {
      "name": "Only-AppService-resolves-Repository",
      "description": "Only application services may depend on repository interfaces",
      "sourceRole": "DomainService",
      "targetRole": "Repository",
      "decision": "Allow",
      "note": "Domain services may use repository interfaces — but not impls"
    }
  ]
}
```

### Correct Structure

```
MyApp.Domain (DomainLayer)
  ├── Order.cs                [AggregateRoot]
  ├── IOrderRepository.cs     [Repository] ← interface lives in domain
  └── PricingDomainService.cs [DomainService] ← uses IOrderRepository

MyApp.Application (ApplicationLayer)
  └── PlaceOrderApplicationService.cs [ApplicationService]
      ← depends on IOrderRepository, PricingDomainService

MyApp.Infrastructure (InfrastructureLayer)
  └── SqlOrderRepository.cs   [Repository + InfrastructureService]
      ← implements IOrderRepository from domain
```

---

## UC-D07: Domain Event Flow Rules

**Problem:** Domain events must flow in one direction: raised by aggregate
roots, handled by domain event handlers, published by the infrastructure
publisher. Events must not create circular dependencies.

**Building block used:** DDD Pack + Events Pack

### Policy

```json
{
  "rules": [
    {
      "name": "AggregateRoot-raises-DomainEvent",
      "sourceRole": "AggregateRoot",
      "targetRole": "DomainEvent",
      "decision": "Allow"
    },
    {
      "name": "DomainEventHandler-depends-AppService",
      "description": "Handlers may orchestrate via application services",
      "sourceRole": "DomainEventHandler",
      "targetRole": "ApplicationService",
      "decision": "Allow"
    },
    {
      "name": "DomainEventHandler-depends-Repository",
      "sourceRole": "DomainEventHandler",
      "targetRole": "Repository",
      "decision": "Allow"
    },
    {
      "name": "DomainEvent-no-handler-dependency",
      "description": "Events must not depend on handlers — no circular flow",
      "sourceRole": "DomainEvent",
      "targetRole": "DomainEventHandler",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "DomainEvent-no-publisher-dependency",
      "sourceRole": "DomainEvent",
      "targetRole": "DomainEventPublisher",
      "decision": "Deny",
      "severity": "Error"
    }
  ]
}
```

### Compliant Event Flow

```csharp
// 1. Aggregate raises the event (AggregateRoot → DomainEvent: Allow)
public class Order : AggregateRoot<OrderId>
{
    public override OrderId Id { get; }

    public void Place()
    {
        // business logic ...
        Raise(new OrderPlacedDomainEvent(Id));  // ✓
    }
}

// 2. Event is pure data (no service dependencies)
public class OrderPlacedDomainEvent : IDomainEvent
{
    public OrderId OrderId { get; }
    public OrderPlacedDomainEvent(OrderId orderId) => OrderId = orderId;
}

// 3. Handler reacts (DomainEventHandler → ApplicationService: Allow)
public class OrderPlacedDomainEventHandler
    : IDomainEventHandler<OrderPlacedDomainEvent>
{
    private readonly IOrderRepository _orders;       // ✓ Repository
    private readonly NotifyCustomerApplicationService _notify; // ✓ ApplicationService

    public async Task Handle(OrderPlacedDomainEvent ev)
    {
        var order = await _orders.GetByIdAsync(ev.OrderId);
        await _notify.SendConfirmationAsync(order);
    }
}
```

### Require Rule: Every Aggregate Root Must Raise At Least One Domain Event

```json
{
  "name": "AggregateRoot-must-raise-events",
  "sourceRole": "AggregateRoot",
  "targetRole": "DomainEvent",
  "decision": "Require",
  "scope": "Type",
  "severity": "Warning"
}
```

This surfaces a warning for every aggregate root that never raises any domain
event — useful as an early signal that the event model is incomplete.

---

## UC-D08: Bounded Context Boundary Enforcement

**Problem:** Two bounded contexts (Order Management and Inventory) must not
have direct type-level dependencies. Cross-context communication must go
through integration events or an anti-corruption layer.

**Building block used:** DDD Pack + Structural Core Pack (role `Contracts`)

### Role Assignment

```csharp
// Order Management bounded context
[assembly: Role("BoundedContext.Orders")]    // in MyApp.Orders.*

// Inventory bounded context
[assembly: Role("BoundedContext.Inventory")] // in MyApp.Inventory.*

// Shared contracts (integration events, anti-corruption layer interfaces)
[assembly: Role("Contracts")]                // in MyApp.Shared.Contracts
```

### Policy

```json
{
  "name": "BoundedContextIsolation",
  "defaultDecision": "Allow",
  "combinationRules": [
    {
      "name": "BoundedContexts-are-exclusive",
      "leftRoles": "BoundedContext.*",
      "rightRoles": "BoundedContext.*",
      "kind": "Exclusive"
    }
  ],
  "rules": [
    {
      "name": "Orders-no-direct-Inventory",
      "sourceRole": "BoundedContext.Orders",
      "targetRole": "BoundedContext.Inventory",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "Inventory-no-direct-Orders",
      "sourceRole": "BoundedContext.Inventory",
      "targetRole": "BoundedContext.Orders",
      "decision": "Deny",
      "severity": "Error"
    },
    {
      "name": "BoundedContext-may-use-Contracts",
      "sourceRole": "BoundedContext.*",
      "targetRole": "Contracts",
      "decision": "Allow"
    }
  ]
}
```

### Violation

```csharp
// In MyApp.Orders (role: BoundedContext.Orders)
// XMoleculesBricks0001: 'OrderFulfillmentService' depends on 'InventoryItem'
// (role: BoundedContext.Inventory). Cross-context direct dependency denied.
public class OrderFulfillmentService : IApplicationService
{
    private readonly InventoryItem _item; // ← direct dependency on Inventory type
}
```

### Compliant Pattern

```csharp
// In MyApp.Shared.Contracts (role: Contracts)
public interface IInventoryAvailabilityChecker
{
    Task<bool> IsAvailableAsync(ProductId productId, int quantity);
}

// In MyApp.Orders (role: BoundedContext.Orders) — depends on Contracts, not Inventory
public class OrderFulfillmentService : IApplicationService
{
    private readonly IInventoryAvailabilityChecker _checker; // ✓ via Contracts
}

// In MyApp.Inventory (role: BoundedContext.Inventory)
// implements the Contracts interface — dependency direction is correct
public class InventoryAvailabilityChecker : IInventoryAvailabilityChecker { }
```

---

## UC-D09: Full Bounded Context — Order Management

**Problem:** Set up a complete, self-consistent DDD configuration for the
Order Management bounded context using all applicable Bricks building blocks.

**Building blocks used:** DDD Pack (all packs), Naming Conventions,
Member Cardinality, Structural Core Pack

### Project Structure

```
MyApp.Orders.Domain          [BoundedContext.Orders + DomainLayer]
  ├── Order.cs               [AggregateRoot]
  ├── OrderLine.cs           [Entity]
  ├── Money.cs               [ValueObject]
  ├── OrderId.cs             [Identity]
  ├── OrderLineId.cs         [Identity]
  ├── IOrderRepository.cs    [Repository]
  ├── PricingDomainService.cs [DomainService]
  ├── OrderFactory.cs        [Factory]
  └── Events/
      └── OrderPlacedDomainEvent.cs [DomainEvent]

MyApp.Orders.Application     [BoundedContext.Orders + ApplicationLayer]
  ├── PlaceOrderApplicationService.cs [ApplicationService]
  └── Handlers/
      └── OrderPlacedDomainEventHandler.cs [DomainEventHandler]

MyApp.Orders.Infrastructure  [BoundedContext.Orders + InfrastructureLayer]
  └── SqlOrderRepository.cs  [Repository + InfrastructureService]
```

### Complete Domain Assembly Setup

```csharp
// AssemblyInfo.cs in MyApp.Orders.Domain
[assembly: Role("BoundedContext.Orders")]
[assembly: Role("DomainLayer")]

// ─── Identities ────────────────────────────────────────────────────────────

public readonly record struct OrderId(Guid Value) : IIdentity { }   // ✓ suffix Id
public readonly record struct OrderLineId(Guid Value) : IIdentity { } // ✓ suffix Id

// ─── Value Objects ─────────────────────────────────────────────────────────

public sealed class Money : ValueObject  // role: ValueObject (via base class)
{
    public decimal Amount { get; }       // ✓ no setter
    public string Currency { get; }      // ✓ no setter

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static readonly Money Zero = new(0, "CHF");

    protected override IEnumerable<object?> GetEqualityComponents()
        => [Amount, Currency];
}

// ─── Entities ──────────────────────────────────────────────────────────────

public class OrderLine : Entity<OrderLineId>  // role: Entity
{
    public override OrderLineId Id { get; }   // ✓ required Id
    public int Quantity { get; private set; }
    public Money UnitPrice { get; }           // ✓ ValueObject, no service dep

    public OrderLine(OrderLineId id, int quantity, Money unitPrice)
    {
        Id = id;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}

// ─── Aggregate Root ────────────────────────────────────────────────────────

public class Order : AggregateRoot<OrderId>  // role: AggregateRoot
{
    public override OrderId Id { get; }       // ✓ required Id

    private readonly List<OrderLine> _lines = new();
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public Money Total { get; private set; } = Money.Zero;

    public Order(OrderId id) => Id = id;

    public void AddLine(OrderLine line)
    {
        _lines.Add(line);
        Total = new Money(Total.Amount + line.UnitPrice.Amount * line.Quantity,
                          Total.Currency);
    }

    public void Place()
    {
        if (!_lines.Any())
            throw new InvalidOperationException("Cannot place empty order");

        Raise(new OrderPlacedDomainEvent(Id, Total)); // ✓ raises DomainEvent
    }
}

// ─── Domain Event ──────────────────────────────────────────────────────────

public class OrderPlacedDomainEvent : IDomainEvent  // role: DomainEvent
{
    public OrderId OrderId { get; }   // ✓ no service dependencies
    public Money Amount { get; }      // ✓ ValueObject only

    public OrderPlacedDomainEvent(OrderId orderId, Money amount)
    {
        OrderId = orderId;
        Amount = amount;
    }
}

// ─── Repository Interface (in domain) ─────────────────────────────────────

public interface IOrderRepository  // role: Repository, ✓ prefix I, suffix Repository
    : IRepository<Order, OrderId>
{
    Task<Order?> GetByIdAsync(OrderId id);
    Task SaveAsync(Order order);
}

// ─── Domain Service ────────────────────────────────────────────────────────

public class PricingDomainService : IDomainService  // role: DomainService
{
    private readonly IPriceRepository _prices; // ✓ repository interface
    public PricingDomainService(IPriceRepository prices) => _prices = prices;

    public async Task<Money> CalculatePriceAsync(OrderLine line)
    {
        var basePrice = await _prices.GetPriceAsync(line.Id);
        return new Money(basePrice.Amount * line.Quantity, basePrice.Currency);
    }
}

// ─── Factory ───────────────────────────────────────────────────────────────

public class OrderFactory : IFactory<Order>  // role: Factory, ✓ suffix Factory
{
    public Order Create(OrderId id) => new(id);
}
```

### Application Assembly

```csharp
// AssemblyInfo.cs in MyApp.Orders.Application
[assembly: Role("BoundedContext.Orders")]
[assembly: Role("ApplicationLayer")]

public class PlaceOrderApplicationService : IApplicationService  // ✓ suffix
{
    private readonly IOrderRepository _orders;          // ✓ Repository interface
    private readonly OrderFactory _factory;             // ✓ Factory
    private readonly PricingDomainService _pricing;     // ✓ DomainService
    private readonly IDomainEventPublisher _publisher;  // ✓ DomainEventPublisher

    public async Task<OrderId> PlaceOrderAsync(PlaceOrderCommand command)
    {
        var order = _factory.Create(OrderId.New());
        foreach (var item in command.Items)
        {
            var price = await _pricing.CalculatePriceAsync(...);
            order.AddLine(new OrderLine(OrderLineId.New(), item.Quantity, price));
        }
        order.Place();
        await _orders.SaveAsync(order);
        await _publisher.PublishAsync(order.DomainEvents);
        return order.Id;
    }
}

public class OrderPlacedDomainEventHandler          // ✓ suffix DomainEventHandler
    : IDomainEventHandler<OrderPlacedDomainEvent>
{
    private readonly IOrderRepository _orders;      // ✓ Repository
    public async Task Handle(OrderPlacedDomainEvent ev) { ... }
}
```

### Infrastructure Assembly

```csharp
// AssemblyInfo.cs in MyApp.Orders.Infrastructure
[assembly: Role("BoundedContext.Orders")]
[assembly: Role("InfrastructureLayer")]

// Role alias: SqlOrderRepository is both Repository and InfrastructureService
[RoleAlias(typeof(IOrderRepository), "Repository")]
public class SqlOrderRepository : IOrderRepository  // ✓ suffix Repository
{
    private readonly DbContext _db;

    public async Task<Order?> GetByIdAsync(OrderId id) =>
        await _db.Orders.FindAsync(id.Value);

    public async Task SaveAsync(Order order) =>
        await _db.SaveChangesAsync();
}
```

### Active Rules Summary for This Context

| Rule | Source Role | Target Role | Decision |
|---|---|---|---|
| Domain-no-infra | `DomainLayer` | `InfrastructureLayer` | Deny |
| App-no-infra | `ApplicationLayer` | `InfrastructureLayer` | Deny |
| Entity-no-Repository | `Entity` | `Repository` | Deny |
| AggregateRoot-no-Repository | `AggregateRoot` | `Repository` | Deny |
| ValueObject-no-services | `ValueObject` | `DomainService` | Deny |
| DomainEvent-no-services | `DomainEvent` | `DomainService` | Deny |
| Orders-no-Inventory | `BoundedContext.Orders` | `BoundedContext.Inventory` | Deny |
| AggregateRoot-must-raise | `AggregateRoot` → `DomainEvent` | — | Require |

---

## UC-D10: DDD + AI-Generated Code

**Problem:** The team uses an AI code generator to scaffold DDD boilerplate
(aggregates, events, handlers). Generated code must conform to all DDD
naming conventions, cardinality contracts, and dependency rules automatically.

**Building blocks used:** all DDD building blocks (passive detection)

### How passive detection works for DDD

The Roslyn analyzer evaluates generated code identically to hand-written code.
All DDD rules are active. Common generator patterns that produce violations:

**Naming violations (XMoleculesBricks0020):**

```csharp
// Generator emits a short name without the required suffix
public class OrderCreated : IDomainEvent { }      // missing suffix DomainEvent
public class OrderRepo : IOrderRepository { }      // missing suffix Repository
public class Pricing : IDomainService { }          // missing suffix DomainService
```

**Dependency violations (XMoleculesBricks0001):**

```csharp
// Generator pulls in a concrete infrastructure type in domain context
public class OrderAggregate : AggregateRoot<OrderId>
{
    public override OrderId Id { get; }
    private readonly SqlOrderRepository _repo; // ← infrastructure in aggregate
}
```

**Cardinality violations (XMoleculesBricks0003):**

```csharp
// Generator forgets to override Id
public class Customer : AggregateRoot<CustomerId>
{
    public string Name { get; set; } = "";
    // Id not overridden — XMoleculesBricks0003
}
```

**Immutability violations (XMoleculesBricks0005):**

```csharp
// Generator emits mutable value object
public class Address : ValueObject
{
    public string Street { get; set; } = ""; // ← setter — XMoleculesBricks0005
    ...
}
```

### IDE feedback loop

All violations appear inline in the IDE as the developer accepts generator
suggestions. The developer can:

1. Accept the suggestion and immediately see the violation
2. Fix the name/structure before committing
3. Use the IDE quick-fix (if a code fix is provided) to rename automatically

### Phase 2: Policy Export as Generator Context

With Phase 2 active, the generator receives the DDD policy as context:

```json
{
  "namingConventions": [
    { "marker": "IDomainEvent", "pattern": "DomainEvent", "position": "Suffix" },
    { "marker": "IRepository<,>", "pattern": "Repository", "position": "Suffix" },
    { "marker": "IDomainService", "pattern": "DomainService", "position": "Suffix" }
  ],
  "dependencyRules": [
    { "description": "Domain layer must not depend on infrastructure" },
    { "description": "Entities and aggregates must not hold repository references" }
  ]
}
```

The generator uses this context to produce conformant code from the first
attempt — reducing violations before they are detected.

---

## DDD Validation Summary

| UC | Pattern validated | Building blocks |
|---|---|---|
| UC-D01 | Role assignment for all DDD patterns | DDD Pack |
| UC-D02 | Naming conventions for all DDD patterns | Naming Conventions |
| UC-D03 | Aggregate root identity contract | Member Cardinality |
| UC-D04 | Value object immutability | Member Cardinality |
| UC-D05 | Dependency direction within bounded context | DDD Pack (policy) |
| UC-D06 | Repository access rules | DDD Pack (policy) |
| UC-D07 | Domain event flow direction | Events Pack (policy + Require) |
| UC-D08 | Bounded context isolation | Structural Core Pack (Contracts) |
| UC-D09 | Full bounded context composition | All DDD building blocks |
| UC-D10 | AI-generated DDD code safety net | All (passive) |

All DDD use cases are expressible with Layer 1 and Layer 2 building blocks.
No DDD-specific concept requires a change to the core. The marker interface
pattern (UC-D01) is the recommended entry point: it concentrates role, naming
convention, and cardinality declarations in one place and makes the DDD
contract self-documenting at the type level.
