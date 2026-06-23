# NMolecules.Bricks — Use Cases: Hexagonal Architecture

Layer: 3 — Use Cases  
Depends on: Layer 2 (Building Blocks), Layer 1 (Core)  
Status: March 2026

Authoritative model reference: `../foundational-concept.md`.
This document describes target-model Hexagonal scenarios on top of Layer 1 and
Layer 2. Examples here may include future building blocks that are not yet part
of the shipped baseline.

This document maps Hexagonal Architecture (Ports & Adapters) onto Bricks
building blocks. Use cases UC-H01 through UC-H09 cover the full model: port
and adapter role assignment, the core isolation rule, naming conventions,
Require rules for structural completeness, DDD combination, and a full worked
example.

**Document scope:** Hexagonal Architecture scenarios only.

---

## Hexagonal Architecture Recap

The application **core** (the hexagon) contains all business logic. It
communicates with the outside world only via **ports** — interfaces declared
inside the core. **Adapters** sit outside the hexagon and connect it to
technology.

Two port directions:

- **Driving port (left):** the core exposes this interface. A driving adapter
  calls it. Example: `IPlaceOrderPort` called by an HTTP controller.
- **Driven port (right):** the core depends on this interface. A driven adapter
  implements it. Example: `IOrderRepository` implemented by a database adapter.

The **single rule of Hexagonal Architecture:** the core never references an
adapter. All dependencies from the core point inward — to the core itself or
to other port interfaces also inside the core.

```
HTTP Controller        PlaceOrderPort         Core
[HEX.Adapter.Driving]→[HEX.Port.Driving]→[HEX.Core]
                                                │
                                        IOrderRepository
                                        [HEX.Port.Driven]
                                                │
                                        SqlOrderAdapter
                                        [HEX.Adapter.Driven]
```

---

## Hexagonal Use Case Index

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

---

## Conceptual Map

| Hexagonal Element | Bricks Role | Location | Naming |
|---|---|---|---|
| Application core (domain + use cases) | `HEX.Core` | Core assembly | — |
| Driving port (entry point interface) | `HEX.Core` + `HEX.Port.Driving` | Core assembly | Prefix `I`, Suffix `Port` |
| Driven port (outbound interface) | `HEX.Core` + `HEX.Port.Driven` | Core assembly | Prefix `I`, Suffix `Port` |
| Driving adapter (HTTP, CLI, test) | `HEX.Adapter.Driving` | Adapters assembly | Suffix `Adapter` |
| Driven adapter (DB, email, bus) | `HEX.Adapter.Driven` | Adapters assembly | Suffix `Adapter` |

---

## UC-H01: Port and Adapter Role Assignment

**Problem:** Assign Hexagonal Architecture roles to all types so the analyzer
can enforce the core isolation rule and port completeness rules.

**Building block used:** Hexagonal Pack

### Assembly-Level Assignment (recommended)

```csharp
// MyApp.Core — contains domain model, use cases, and both port kinds
[assembly: Role("HEX.Core")]

// MyApp.Adapters — contains all driving and driven adapters
// Individual types carry their specific adapter direction role
```

### Marker Interfaces

The Hexagonal Pack ships marker interfaces that carry role, naming convention,
and direction in one declaration.

```csharp
// ─── Driving Port (declared in core) ──────────────────────────────────────

[Role("HEX.Core")]
[Role("HEX.Port.Driving")]
[NameConvention("I", NamePosition.Prefix)]
[NameConvention("Port", NamePosition.Suffix)]
public interface IDrivingPort { }

// ─── Driven Port (declared in core) ───────────────────────────────────────

[Role("HEX.Core")]
[Role("HEX.Port.Driven")]
[NameConvention("I", NamePosition.Prefix)]
[NameConvention("Port", NamePosition.Suffix)]
public interface IDrivenPort { }

// ─── Driving Adapter (outside the hexagon) ────────────────────────────────

[Role("HEX.Adapter.Driving")]
[NameConvention("Adapter", NamePosition.Suffix)]
public interface IDrivingAdapter { }

// ─── Driven Adapter (outside the hexagon) ────────────────────────────────

[Role("HEX.Adapter.Driven")]
[NameConvention("Adapter", NamePosition.Suffix)]
public interface IDrivenAdapter { }
```

### Concrete Role Assignment

```csharp
// Core assembly [HEX.Core via assembly attribute]

// Driving port — roles: HEX.Core + HEX.Port.Driving (additive)
public interface IPlaceOrderPort : IDrivingPort  // ✓ prefix I, suffix Port
{
    Task<OrderId> PlaceOrder(PlaceOrderCommand command);
}

// Driven port — roles: HEX.Core + HEX.Port.Driven (additive)
public interface IOrderRepositoryPort : IDrivenPort  // ✓
{
    Task<Order?> FindByIdAsync(OrderId id);
    Task SaveAsync(Order order);
}

// Core use case — role: HEX.Core (via assembly)
public class PlaceOrderUseCase : IPlaceOrderPort  // implements driving port ✓
{
    private readonly IOrderRepositoryPort _orders; // depends on driven port ✓
    // ...
}

// Adapters assembly — each type carries its adapter role directly

// Driving adapter — role: HEX.Adapter.Driving
public class HttpOrderAdapter : IDrivingAdapter  // ✓ suffix Adapter
{
    private readonly IPlaceOrderPort _port;
    // ...
}

// Driven adapter — role: HEX.Adapter.Driven
public class SqlOrderAdapter : IDrivenAdapter, IOrderRepositoryPort  // ✓
{
    private readonly OrderDbContext _db;
    // ...
}
```

---

## UC-H02: Core Isolation Rule

**Problem:** The core must never reference an adapter. All dependencies from
core elements must point only to other core elements (domain types, port
interfaces). Any direct reference to an adapter class is a violation.

**Building block used:** Hexagonal Pack

### Violation: Core references driving adapter

```csharp
// MyApp.Core [HEX.Core]
// XMoleculesBricks0001: 'PlaceOrderUseCase' (HEX.Core) depends on
// 'HttpOrderAdapter' (HEX.Adapter.Driving).
// Denied by rule 'HEX-Core-no-Adapter-Driving'.
public class PlaceOrderUseCase : IPlaceOrderPort
{
    private readonly HttpOrderAdapter _http; // ← adapter in core
}
```

### Violation: Core references driven adapter implementation

```csharp
// MyApp.Core [HEX.Core]
// XMoleculesBricks0001: 'PlaceOrderUseCase' depends on
// 'SqlOrderAdapter' (HEX.Adapter.Driven).
// Denied by rule 'HEX-Core-no-Adapter-Driven'.
public class PlaceOrderUseCase : IPlaceOrderPort
{
    private readonly SqlOrderAdapter _db; // ← concrete adapter in core
}
```

### Compliant: Core depends on driven port interface

```csharp
// ✓ Core depends on the driven port interface — which also lives in core
public class PlaceOrderUseCase : IPlaceOrderPort
{
    private readonly IOrderRepositoryPort _orders; // ✓ HEX.Core + HEX.Port.Driven

    public PlaceOrderUseCase(IOrderRepositoryPort orders) => _orders = orders;

    public async Task<OrderId> PlaceOrder(PlaceOrderCommand command)
    {
        var order = new Order(OrderId.New());
        // ... business logic ...
        await _orders.SaveAsync(order);
        return order.Id;
    }
}
```

### Violation: Port interface references adapter

```csharp
// XMoleculesBricks0001: 'IPlaceOrderPort' (HEX.Port.Driving) depends on
// 'HttpOrderAdapter' (HEX.Adapter.Driving).
// Denied by 'HEX-Port-Driving-no-Adapter'.
public interface IPlaceOrderPort : IDrivingPort
{
    Task<OrderId> PlaceOrder(PlaceOrderCommand command);
    HttpOrderAdapter CreateAdapter(); // ← port must not reference adapter
}
```

---

## UC-H03: Naming Conventions for Ports and Adapters

**Problem:** Ports and adapters must be identifiable by name without inspecting
the type hierarchy or role assignments.

**Building block used:** Hexagonal Pack + Naming Conventions

### Default Naming Rules

All naming conventions are declared on the marker interfaces in UC-H01.

| Marker | Prefix | Suffix | Compliant example |
|---|---|---|---|
| `IDrivingPort` | `I` | `Port` | `IPlaceOrderPort`, `IRegisterCustomerPort` |
| `IDrivenPort` | `I` | `Port` | `IOrderRepositoryPort`, `IEmailNotificationPort` |
| `IDrivingAdapter` | — | `Adapter` | `HttpOrderAdapter`, `CliOrderAdapter` |
| `IDrivenAdapter` | — | `Adapter` | `SqlOrderAdapter`, `SmtpEmailAdapter` |

### Compliant Names

```csharp
public interface IPlaceOrderPort : IDrivingPort { }          // ✓
public interface IOrderRepositoryPort : IDrivenPort { }      // ✓
public class HttpOrderAdapter : IDrivingAdapter { }          // ✓
public class SqlOrderAdapter : IDrivenAdapter { }            // ✓
```

### Violations

```csharp
// XMoleculesBricks0010 — missing suffix
public interface IPlaceOrder : IDrivingPort { }      // must end with 'Port'
public interface IOrderRepository : IDrivenPort { }  // must end with 'Port'
public class HttpOrder : IDrivingAdapter { }         // must end with 'Adapter'
public class SqlOrder : IDrivenAdapter { }           // must end with 'Adapter'
```

### Domain-Aligned Naming Override

Some teams prefer `IOrderRepository` over `IOrderRepositoryPort` for the
driven port. The direction is semantically carried by the role, not the name.
Teams may override the suffix convention:

```json
// bricks-policy.json — local naming override
{
  "namingConventionOverrides": [
    {
      "targetRole": "HEX.Port.Driven",
      "suppressConvention": { "pattern": "Port", "position": "Suffix" },
      "reason": "Driven ports use domain-aligned names (IOrderRepository) rather than the Port suffix"
    }
  ]
}
```

With this override active: `IOrderRepository : IDrivenPort` is compliant.
The `HEX.Port.Driven` role is still enforced; only the naming convention is
relaxed.

---

## UC-H04: Require — Driving Adapter Must Call a Driving Port

**Problem:** A driving adapter that never calls a driving port bypasses the
hexagon. The Require rule catches this structural incompleteness.

**Building block used:** Hexagonal Pack (Require rule `HEX-Adapter-Driving-requires-Port`)

### What the rule checks

For every type with role `HEX.Adapter.Driving`, at least one dependency of
any kind to a type with role `HEX.Port.Driving` must exist within `Type` scope.

### Violation

```csharp
// MyApp.Adapters [HEX.Adapter.Driving via IDrivingAdapter]
// XMoleculesBricks0002 (Requirement):
// 'CliOrderAdapter' (HEX.Adapter.Driving) has no dependency on any type
// with role HEX.Port.Driving. Rule: HEX-Adapter-Driving-requires-Port.
public class CliOrderAdapter : IDrivingAdapter
{
    private readonly PlaceOrderUseCase _uc; // ← depends on concrete use case, not port
    // Bypasses the port — the adapter can see the implementation directly
}
```

### Compliant

```csharp
// ✓ Driving adapter depends on the driving port interface
public class CliOrderAdapter : IDrivingAdapter
{
    private readonly IPlaceOrderPort _port; // ✓ HEX.Port.Driving

    public CliOrderAdapter(IPlaceOrderPort port) => _port = port;

    public async Task RunAsync(string[] args)
    {
        var command = ParseArgs(args);
        var orderId = await _port.PlaceOrder(command); // ✓
        Console.WriteLine($"Order placed: {orderId}");
    }
}
```

---

## UC-H05: Require — Driven Adapter Must Implement a Driven Port

**Problem:** A driven adapter that implements no driven port cannot be
reached from the core and has no purpose in the hexagonal model. The Require
rule catches this.

**Building block used:** Hexagonal Pack (Require rule `HEX-Adapter-Driven-requires-Port`)

### What the rule checks

For every type with role `HEX.Adapter.Driven`, at least one
`InterfaceImplementation` dependency to a type with role `HEX.Port.Driven`
must exist.

### Violation

```csharp
// XMoleculesBricks0002 (Requirement):
// 'InMemoryCache' (HEX.Adapter.Driven) implements no type with role
// HEX.Port.Driven. Rule: HEX-Adapter-Driven-requires-Port.
// The adapter is not reachable from the core.
public class InMemoryCache : IDrivenAdapter
{
    // implements no driven port — cannot be injected into core
    public void Store(string key, object value) { ... }
}
```

### Compliant

```csharp
// Declare the driven port in core:
public interface ICachePort : IDrivenPort
{
    void Store(string key, object value);
    object? Get(string key);
}

// Driven adapter implements the port — now reachable from core ✓
public class InMemoryCacheAdapter : IDrivenAdapter, ICachePort
{
    private readonly Dictionary<string, object> _store = new();
    public void Store(string key, object value) => _store[key] = value;
    public object? Get(string key) => _store.GetValueOrDefault(key);
}
```

---

## UC-H06: Hexagonal + DDD in the Core

**Problem:** The core contains DDD tactical patterns. DDD roles and Hexagonal
roles must coexist without conflicts.

**Building block used:** Hexagonal Pack + DDD Pack

DDD roles are additive with `HEX.Core` per the pack's combination rules.
A type can simultaneously be `HEX.Core` + `AggregateRoot`, or
`HEX.Core` + `DomainEvent`, or `HEX.Core` + `HEX.Port.Driven` + `Repository`.

### Role Coexistence

```csharp
// Core assembly [HEX.Core via assembly]

// AggregateRoot in the core — roles: HEX.Core + AggregateRoot ✓
public class Order : AggregateRoot<OrderId>
{
    public override OrderId Id { get; }
    public void Place() => Raise(new OrderPlacedDomainEvent(Id));
}

// DomainEvent in the core — roles: HEX.Core + DomainEvent ✓
public class OrderPlacedDomainEvent : IDomainEvent { ... }

// Driven port for persistence — roles: HEX.Core + HEX.Port.Driven ✓
// Also carries Repository role from DDD Pack: HEX.Core + HEX.Port.Driven + Repository ✓
public interface IOrderRepositoryPort : IDrivenPort, IRepository<Order, OrderId>
{
    Task<Order?> FindByIdAsync(OrderId id);
    Task SaveAsync(Order order);
}

// Domain service in core — roles: HEX.Core + DomainService ✓
public class PricingDomainService : IDomainService
{
    private readonly IPriceRepositoryPort _prices; // ✓ driven port
}
```

### Active Rules

All DDD rules and all Hexagonal rules are active simultaneously. The intersection:

| Source | Target | Decision | Source |
|---|---|---|---|
| `HEX.Core` | `HEX.Adapter.Driven` | Deny Error | HEX-Core-no-Adapter-Driven |
| `AggregateRoot` | `Repository` (impl) | Deny Error | DDD: AggregateRoot-no-Repository |
| `DomainEvent` | `DomainService` | Deny Error | DDD: DomainEvent-no-services |
| `ValueObject` | `DomainService` | Deny Error | DDD: ValueObject-no-services |
| `HEX.Adapter.Driven` | must implement port | Require Error | HEX-Adapter-Driven-requires-Port |

---

## UC-H07: Multiple Adapters for One Port

**Problem:** A single driven port has multiple adapter implementations — for
example a database adapter in production and an in-memory adapter for tests.
Both must be recognised as valid, and neither may violate the core isolation
rule.

**Building block used:** Hexagonal Pack

### Setup

```csharp
// Core [HEX.Core + HEX.Port.Driven]
public interface IOrderRepositoryPort : IDrivenPort
{
    Task<Order?> FindByIdAsync(OrderId id);
    Task SaveAsync(Order order);
}

// Production adapter [HEX.Adapter.Driven] ✓
public class SqlOrderAdapter : IDrivenAdapter, IOrderRepositoryPort { ... }

// Test adapter [HEX.Adapter.Driven + TestOnly] ✓
// TestOnly is additive with HEX.Adapter.Driven
public class InMemoryOrderAdapter : IDrivenAdapter, IOrderRepositoryPort
{
    private readonly Dictionary<Guid, Order> _store = new();
    public Task<Order?> FindByIdAsync(OrderId id) =>
        Task.FromResult(_store.GetValueOrDefault(id.Value));
    public Task SaveAsync(Order order)
    {
        _store[order.Id.Value] = order;
        return Task.CompletedTask;
    }
}
```

Both adapters implement `IOrderRepositoryPort` → the Require rule is
satisfied for both. The core is injected with whichever implementation the DI
container provides. Bricks does not enforce which adapter is wired — that is
a runtime concern. It only enforces that the structural contracts are met.

---

## UC-H08: Full Example — Order Placement

**Problem:** Build a complete Hexagonal Architecture project for order
placement with HTTP and CLI driving adapters, a database driven adapter, and
a notification driven adapter.

**Building blocks used:** Hexagonal Pack, DDD Pack, Naming Conventions,
Member Cardinality

### Project Structure

```
MyApp.Core                 [HEX.Core]
  ├── Order.cs             [HEX.Core + AggregateRoot]
  ├── OrderId.cs           [HEX.Core + Identity]
  ├── Money.cs             [HEX.Core + ValueObject]
  ├── OrderPlacedDomainEvent.cs [HEX.Core + DomainEvent]
  ├── Ports/
  │   ├── IPlaceOrderPort.cs      [HEX.Core + HEX.Port.Driving]
  │   ├── IOrderRepositoryPort.cs [HEX.Core + HEX.Port.Driven]
  │   └── IEmailNotificationPort.cs [HEX.Core + HEX.Port.Driven]
  └── PlaceOrderUseCase.cs [HEX.Core] — implements IPlaceOrderPort

MyApp.Adapters.Driving     [HEX.Adapter.Driving]
  ├── HttpOrderAdapter.cs  [HEX.Adapter.Driving]
  └── CliOrderAdapter.cs   [HEX.Adapter.Driving]

MyApp.Adapters.Driven      [HEX.Adapter.Driven]
  ├── SqlOrderAdapter.cs   [HEX.Adapter.Driven]
  └── SmtpEmailAdapter.cs  [HEX.Adapter.Driven]

MyApp.Host                 — DI composition root; wires everything
```

### Core Assembly

```csharp
[assembly: Role("HEX.Core")]

// ─── Domain model ──────────────────────────────────────────────────────────

public readonly record struct OrderId(Guid Value) : IIdentity { } // ✓ suffix Id

public sealed class Money : ValueObject  // HEX.Core + ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    public Money(decimal amount, string currency) { Amount = amount; Currency = currency; }
    public static readonly Money Zero = new(0, "CHF");
    protected override IEnumerable<object?> GetEqualityComponents() => [Amount, Currency];
}

public class Order : AggregateRoot<OrderId>  // HEX.Core + AggregateRoot
{
    public override OrderId Id { get; }
    public Money Total { get; private set; } = Money.Zero;

    public Order(OrderId id) => Id = id;

    public void AddLine(int quantity, Money unitPrice)
        => Total = new Money(Total.Amount + unitPrice.Amount * quantity, Total.Currency);

    public void Place() => Raise(new OrderPlacedDomainEvent(Id, Total)); // ✓
}

public class OrderPlacedDomainEvent : IDomainEvent  // HEX.Core + DomainEvent
{
    public OrderId OrderId { get; }
    public Money Amount { get; }
    public OrderPlacedDomainEvent(OrderId id, Money amount) { OrderId = id; Amount = amount; }
}

// ─── Ports ─────────────────────────────────────────────────────────────────

// Driving port — HEX.Core + HEX.Port.Driving ✓ prefix I, suffix Port
public interface IPlaceOrderPort : IDrivingPort
{
    Task<OrderId> PlaceOrder(IReadOnlyList<(int Qty, Money Price)> lines);
}

// Driven ports — HEX.Core + HEX.Port.Driven ✓
public interface IOrderRepositoryPort : IDrivenPort
{
    Task<Order?> FindByIdAsync(OrderId id);
    Task SaveAsync(Order order);
}

public interface IEmailNotificationPort : IDrivenPort
{
    Task SendOrderConfirmationAsync(OrderId orderId, Money total);
}

// ─── Use Case (implements driving port) ────────────────────────────────────

public class PlaceOrderUseCase : IPlaceOrderPort  // ✓ implements driving port
{
    private readonly IOrderRepositoryPort _orders;      // ✓ driven port
    private readonly IEmailNotificationPort _email;     // ✓ driven port

    public PlaceOrderUseCase(
        IOrderRepositoryPort orders,
        IEmailNotificationPort email)
    {
        _orders = orders;
        _email = email;
    }

    public async Task<OrderId> PlaceOrder(IReadOnlyList<(int Qty, Money Price)> lines)
    {
        var order = new Order(OrderId.New());
        foreach (var (qty, price) in lines)
            order.AddLine(qty, price);

        order.Place();
        await _orders.SaveAsync(order);
        await _email.SendOrderConfirmationAsync(order.Id, order.Total);
        return order.Id;
    }
}
```

### Driving Adapters Assembly

```csharp
[assembly: Role("HEX.Adapter.Driving")]

// HTTP driving adapter — calls core via driving port ✓
public class HttpOrderAdapter : IDrivingAdapter  // ✓ suffix Adapter
{
    private readonly IPlaceOrderPort _port; // ✓ HEX.Port.Driving

    public HttpOrderAdapter(IPlaceOrderPort port) => _port = port;

    public async Task<Guid> HandlePostAsync(OrderRequest request)
    {
        var lines = request.Lines
            .Select(l => (l.Quantity, new Money(l.UnitPrice, l.Currency)))
            .ToList();
        var id = await _port.PlaceOrder(lines); // ✓ via port
        return id.Value;
    }
}

// CLI driving adapter
public class CliOrderAdapter : IDrivingAdapter  // ✓
{
    private readonly IPlaceOrderPort _port;

    public CliOrderAdapter(IPlaceOrderPort port) => _port = port;

    public async Task RunAsync(string[] args)
    {
        // parse args into lines ...
        var id = await _port.PlaceOrder(lines);
        Console.WriteLine($"Order {id.Value} placed.");
    }
}
```

### Driven Adapters Assembly

```csharp
[assembly: Role("HEX.Adapter.Driven")]

// Database driven adapter — implements driven port ✓
public class SqlOrderAdapter : IDrivenAdapter, IOrderRepositoryPort  // ✓
{
    private readonly OrderDbContext _db;
    public SqlOrderAdapter(OrderDbContext db) => _db = db;

    public async Task<Order?> FindByIdAsync(OrderId id) =>
        await _db.Orders.FindAsync(id.Value);

    public async Task SaveAsync(Order order)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync();
    }
}

// SMTP notification driven adapter — implements driven port ✓
public class SmtpEmailAdapter : IDrivenAdapter, IEmailNotificationPort  // ✓
{
    private readonly SmtpClient _smtp;
    public SmtpEmailAdapter(SmtpClient smtp) => _smtp = smtp;

    public async Task SendOrderConfirmationAsync(OrderId orderId, Money total)
    {
        await _smtp.SendMailAsync(new MailMessage
        {
            Subject = $"Order confirmed: {orderId.Value}",
            Body = $"Total: {total.Amount} {total.Currency}"
        });
    }
}
```

### Dependency Flow Validation

```
HttpOrderAdapter  →  IPlaceOrderPort  →  PlaceOrderUseCase
                                               │
                                       IOrderRepositoryPort
                                               │
                                       SqlOrderAdapter

HttpOrderAdapter   [HEX.Adapter.Driving]  → depends on IPlaceOrderPort [HEX.Port.Driving] ✓
PlaceOrderUseCase  [HEX.Core]             → depends on IOrderRepositoryPort [HEX.Port.Driven] ✓
SqlOrderAdapter    [HEX.Adapter.Driven]   → implements IOrderRepositoryPort ✓
SmtpEmailAdapter   [HEX.Adapter.Driven]   → implements IEmailNotificationPort ✓
```

### Active Rules Summary

| Rule | Source | Target | Decision |
|---|---|---|---|
| HEX-Core-no-Adapter-Driving | `HEX.Core` | `HEX.Adapter.Driving` | Deny Error |
| HEX-Core-no-Adapter-Driven | `HEX.Core` | `HEX.Adapter.Driven` | Deny Error |
| HEX-Port-Driving-no-Adapter | `HEX.Port.Driving` | `HEX.Adapter.Driving` | Deny Error |
| HEX-Port-Driven-no-Adapter | `HEX.Port.Driven` | `HEX.Adapter.Driven` | Deny Error |
| HEX-Adapter-Driving-requires-Port | `HEX.Adapter.Driving` | `HEX.Port.Driving` | Require Error |
| HEX-Adapter-Driven-requires-Port | `HEX.Adapter.Driven` | `HEX.Port.Driven` | Require Error |
| DDD: AggregateRoot-no-Repository | `AggregateRoot` | `Repository` impl | Deny Error |
| DDD: DomainEvent-pure | `DomainEvent` | any service | Deny Error |

---

## UC-H09: AI-Generated Code Safety Net

**Problem:** AI code generators commonly violate the core isolation rule by
injecting concrete adapter types into the core, or they generate adapters that
bypass the port interface entirely.

**Building blocks used:** Hexagonal Pack (passive detection)

### Common Generator Violations

**Core references concrete adapter:**

```csharp
// Generated in MyApp.Core [HEX.Core]
// XMoleculesBricks0001: 'PlaceOrderUseCase' (HEX.Core) depends on
// 'SqlOrderAdapter' (HEX.Adapter.Driven).
// Denied by HEX-Core-no-Adapter-Driven.
public class PlaceOrderUseCase : IPlaceOrderPort
{
    private readonly SqlOrderAdapter _db; // ← generator pulled concrete adapter
}
```

**Driving adapter bypasses port:**

```csharp
// Generated in MyApp.Adapters.Driving [HEX.Adapter.Driving]
// XMoleculesBricks0002 (Requirement): 'HttpOrderAdapter' has no dependency
// on any HEX.Port.Driving type. Rule: HEX-Adapter-Driving-requires-Port.
public class HttpOrderAdapter : IDrivingAdapter
{
    private readonly PlaceOrderUseCase _uc; // ← concrete use case, not port
}
```

**Driven adapter implements nothing:**

```csharp
// XMoleculesBricks0002 (Requirement): 'RedisCache' (HEX.Adapter.Driven)
// implements no HEX.Port.Driven type. Rule: HEX-Adapter-Driven-requires-Port.
public class RedisCache : IDrivenAdapter
{
    public void Set(string key, string value) { ... }
    // no driven port implemented — not reachable from core
}
```

**Naming violation:**

```csharp
// XMoleculesBricks0010: 'HttpOrder' does not end with 'Adapter'
public class HttpOrder : IDrivingAdapter { ... }

// XMoleculesBricks0010: 'IPlaceOrder' does not end with 'Port'
public interface IPlaceOrder : IDrivingPort { ... }
```

### Phase 2: Policy Export as Generator Context

```json
{
  "architectureStyle": "HexagonalArchitecture",
  "coreRole": "HEX.Core",
  "ports": [
    { "role": "HEX.Port.Driving", "side": "driving", "namingPrefix": "I", "namingSuffix": "Port" },
    { "role": "HEX.Port.Driven",  "side": "driven",  "namingPrefix": "I", "namingSuffix": "Port" }
  ],
  "adapters": [
    { "role": "HEX.Adapter.Driving", "side": "driving", "namingSuffix": "Adapter",
      "constraint": "must depend on at least one HEX.Port.Driving" },
    { "role": "HEX.Adapter.Driven",  "side": "driven",  "namingSuffix": "Adapter",
      "constraint": "must implement at least one HEX.Port.Driven" }
  ],
  "forbiddenFromCore": ["HEX.Adapter.Driving", "HEX.Adapter.Driven"]
}
```

---

## Hexagonal Validation Summary

| UC | Pattern validated | Building blocks |
|---|---|---|
| UC-H01 | Port and adapter role assignment | Hexagonal Pack |
| UC-H02 | Core isolation (no adapter references) | Hexagonal Pack |
| UC-H03 | Naming conventions + domain-aligned override | Naming Conventions |
| UC-H04 | Driving adapter requires driving port | Hexagonal Pack (Require) |
| UC-H05 | Driven adapter requires driven port | Hexagonal Pack (Require) |
| UC-H06 | HEX + DDD coexistence in core | Hexagonal Pack + DDD Pack |
| UC-H07 | Multiple adapters for one port | Hexagonal Pack |
| UC-H08 | Full project composition | All Hexagonal building blocks |
| UC-H09 | AI-generated code safety net | All (passive) |

The key structural insight: Hexagonal Architecture is more explicit about port
direction than Clean Architecture. Bricks models this with two distinct port
roles (`HEX.Port.Driving` vs `HEX.Port.Driven`) and enforces structural
completeness with Require rules — not just Deny rules. A driving adapter that
never touches a driving port, and a driven adapter that implements no driven
port, are both silent structural failures that Bricks surfaces before they
cause runtime confusion.
