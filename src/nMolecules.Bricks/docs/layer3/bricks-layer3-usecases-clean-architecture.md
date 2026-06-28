# NMolecules.Bricks — Use Cases: Clean Architecture

Layer: 3 — Use Cases  
Depends on: Layer 2 (Building Blocks), Layer 1 (Core)  
Status: March 2026

Authoritative model reference: `../foundational-concept.md`.
This document describes target-model Clean Architecture scenarios on top of
Layer 1 and Layer 2. Examples here may include future building blocks that are
not yet part of the shipped baseline.

This document maps Clean Architecture onto Bricks building blocks. It is a
self-contained extension of the main use case catalogue. Use cases UC-C01
through UC-C09 cover the full Clean Architecture model: ring roles, naming
conventions, the Dependency Rule, boundary contracts, the Presenter pattern,
and combination with DDD tactical patterns.

**Document scope:** Clean Architecture scenarios only.

---

## Clean Architecture Recap

Clean Architecture (Robert C. Martin) organises code in four concentric rings.
The central constraint — the **Dependency Rule** — states that source code
dependencies must always point inward: outer rings may depend on inner rings,
inner rings must never know about outer rings.

```
┌──────────────────────────────────────────────────────┐
│  Frameworks & Drivers                  [CA.Frameworks]│
│  ┌────────────────────────────────────────────────┐  │
│  │  Interface Adapters                [CA.Adapters]│  │
│  │  ┌──────────────────────────────────────────┐  │  │
│  │  │  Application / Use Cases       [CA.UseCases]│ │  │
│  │  │  ┌────────────────────────────────────┐  │  │  │
│  │  │  │  Entities / Core Business  [CA.Core]│  │  │  │
│  │  │  └────────────────────────────────────┘  │  │  │
│  │  └──────────────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────┘

Dependency Rule: arrows point inward only.
CA.Frameworks → CA.Adapters → CA.UseCases → CA.Core
```

---

## Clean Architecture Use Case Index

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

---

## Conceptual Map: Clean Architecture to Bricks Roles

| Ring | Bricks Role | Sub-roles (additive) | Key Naming Convention |
|---|---|---|---|
| Entities / Core Business | `CA.Core` | — (combine with DDD Pack) | Team decides |
| Application / Use Cases | `CA.UseCases` | — | Suffix `UseCase` |
| Interface Adapters | `CA.Adapters` | `CA.Controller`, `CA.Presenter`, `CA.Gateway`, `CA.ViewModel` | Suffix per sub-role |
| Frameworks & Drivers | `CA.Frameworks` | — | Team decides |

---

## UC-C01: Ring Role Assignment

**Problem:** Assign Clean Architecture ring roles to all assemblies and types
in a project so that the Dependency Rule can be enforced by the analyzer.

**Building block used:** Clean Architecture Pack

### Option A — Assembly-Level Assignment (recommended for most projects)

Each assembly represents one ring. Role assignment goes on the assembly.

```csharp
// MyApp.Core (Entities ring)
[assembly: Role("CA.Core")]

// MyApp.UseCases (Use Cases ring)
[assembly: Role("CA.UseCases")]

// MyApp.Adapters (Interface Adapters ring)
[assembly: Role("CA.Adapters")]

// MyApp.Infrastructure (Frameworks & Drivers ring)
[assembly: Role("CA.Frameworks")]
```

### Option B — Type-Level Assignment (for single-assembly projects)

When all rings live in one assembly, roles go on types or namespaces
via external config:

```json
// bricks-policy.json
{
  "roleAssignments": [
    { "namespace": "MyApp.Core.*",           "role": "CA.Core" },
    { "namespace": "MyApp.UseCases.*",       "role": "CA.UseCases" },
    { "namespace": "MyApp.Adapters.*",       "role": "CA.Adapters" },
    { "namespace": "MyApp.Infrastructure.*", "role": "CA.Frameworks" }
  ]
}
```

### Option C — Marker Interfaces (for explicit, IDE-navigable assignment)

```csharp
[Role("CA.Core")]
public interface ICoreEntity { }

[Role("CA.UseCases")]
public interface IUseCase<TInput, TOutput> { }

[Role("CA.Adapters")]
public interface IAdapter { }
```

---

## UC-C02: Dependency Rule Enforcement

**Problem:** The Dependency Rule must be enforced for all code — including
AI-generated scaffolding that commonly violates it by pulling framework types
into the core ring.

**Building block used:** Clean Architecture Pack

The default policy from the Clean Architecture Pack (see Layer 2) enforces all
ring crossings. This use case documents the violation patterns and compliant
alternatives.

### Violation: Core knows about Use Cases

```csharp
// MyApp.Core [CA.Core]
// XMoleculesBricks0001:
// 'Customer' (CA.Core) depends on 'RegisterCustomerUseCase' (CA.UseCases).
// Dependency denied by rule 'CA-Core-no-UseCases'.
public class Customer
{
    private readonly RegisterCustomerUseCase _uc; // ← outer ring in inner ring
}
```

### Violation: Use Cases know about Adapters

```csharp
// MyApp.UseCases [CA.UseCases]
// XMoleculesBricks0001: 'PlaceOrderUseCase' (CA.UseCases) depends on
// 'OrderController' (CA.Adapters). Dependency denied by 'CA-UseCases-no-Adapters'.
public class PlaceOrderUseCase
{
    private readonly OrderController _ctrl; // ← adapter in use case
}
```

### Violation: Use Cases know about Frameworks

```csharp
// MyApp.UseCases [CA.UseCases]
// XMoleculesBricks0001: 'InvoiceUseCase' depends on 'HttpClient' (CA.Frameworks).
public class InvoiceUseCase
{
    private readonly HttpClient _http; // ← framework in use case
}
```

### Compliant Pattern: Dependency Inversion via Interface

The Dependency Rule is enforced with Dependency Inversion. An inner ring
declares an interface; an outer ring implements it.

```
CA.Core declares:        ICustomerRepository (interface, role CA.Core)
CA.Adapters implements:  SqlCustomerRepository (role CA.Adapters + CA.Gateway)
CA.UseCases depends on:  ICustomerRepository — pointing inward ✓
```

```csharp
// MyApp.Core [CA.Core] — interface stays in the inner ring
public interface ICustomerRepository
{
    Task<Customer?> FindByIdAsync(CustomerId id);
}

// MyApp.UseCases [CA.UseCases] — depends on interface, pointing inward ✓
public class RegisterCustomerUseCase : IUseCase<RegisterCustomerInput, CustomerId>
{
    private readonly ICustomerRepository _customers; // ✓ CA.Core interface

    public RegisterCustomerUseCase(ICustomerRepository customers)
        => _customers = customers;
}

// MyApp.Adapters [CA.Adapters + CA.Gateway]
// Implements the interface declared in CA.Core (pointing inward ✓)
public class SqlCustomerGateway : ICustomerRepository
{
    private readonly DbContext _db; // framework dependency stays in adapters ✓
}
```

---

## UC-C03: Naming Conventions Per Ring

**Problem:** Each type in the Use Case and Adapter rings must be identifiable
by its name. The analyzer enforces naming conventions declared on the marker
interfaces.

**Building block used:** Clean Architecture Pack + Naming Conventions

### Naming Rules Summary

| Marker | Pattern | Position | Compliant example |
|---|---|---|---|
| `IUseCase<,>` | `UseCase` | Suffix | `PlaceOrderUseCase`, `RegisterCustomerUseCase` |
| `IPresenter<>` | `Presenter` | Suffix | `OrderConfirmationPresenter` |
| `IGateway` | `Gateway` | Suffix | `SqlCustomerGateway`, `EmailNotificationGateway` |
| `IController` | `Controller` | Suffix | `OrderController`, `CustomerController` |
| `IViewModel` | `ViewModel` | Suffix | `OrderConfirmationViewModel`, `CustomerListViewModel` |

`CA.Core` and `CA.Frameworks` carry no enforced suffix by default.

### Compliant Names

```csharp
public class PlaceOrderUseCase
    : IUseCase<PlaceOrderInput, OrderId> { }            // ✓

public class OrderConfirmationPresenter
    : IPresenter<OrderConfirmationViewModel> { }         // ✓

public class SqlOrderGateway
    : IGateway, IOrderRepository { }                    // ✓

public class OrderController : IController { }          // ✓

public class OrderSummaryViewModel : IViewModel { }     // ✓
```

### Violations

```csharp
// XMoleculesBricks0020
public class PlaceOrder
    : IUseCase<PlaceOrderInput, OrderId> { }            // must end with 'UseCase'

public class OrderConfirmation
    : IPresenter<OrderConfirmationViewModel> { }         // must end with 'Presenter'

public class SqlOrder : IGateway { }                    // must end with 'Gateway'
```

---

## UC-C04: Use Case Input/Output Boundary Contracts

**Problem:** Every use case must declare exactly one `Execute` method (or
equivalent). Missing or multiple entry points are a structural violation.

**Building block used:** Clean Architecture Pack + Member Cardinality

```csharp
[Role("CA.UseCases")]
[NameConvention("UseCase", NamePosition.Suffix)]
[RequireExactlyOneMember("Execute",
    Reason = "Each use case must have a single Execute entry point")]
public interface IUseCase<TInput, TOutput>
{
    Task<TOutput> Execute(TInput input);
}
```

### Violation

```csharp
// XMoleculesBricks0003:
// 'PlaceOrderUseCase' implements IUseCase which requires exactly one 'Execute'
// member, but 'PlaceOrderUseCase' does not declare 'Execute'.
public class PlaceOrderUseCase : IUseCase<PlaceOrderInput, OrderId>
{
    public Task<OrderId> Run(PlaceOrderInput input) => ...; // wrong method name
}
```

### Compliant

```csharp
public class PlaceOrderUseCase : IUseCase<PlaceOrderInput, OrderId>
{
    public async Task<OrderId> Execute(PlaceOrderInput input) // ✓
    {
        // orchestration logic
    }
}
```

---

## UC-C05: Presenter Pattern

**Problem:** Output from use cases must be formatted by a Presenter before
reaching the delivery mechanism. Use cases must not depend on ViewModels
directly — they depend on a Presenter interface declared in their ring.

**Building block used:** Clean Architecture Pack

### Structure

```
CA.UseCases declares:   IOrderPresenter (output boundary interface)
CA.Adapters implements: OrderConfirmationPresenter
CA.Frameworks hosts:    the HTTP response assembly
```

```csharp
// MyApp.UseCases [CA.UseCases] — output boundary interface
// Points inward: declared inside the use case ring ✓
public interface IOrderPresenter
{
    void PresentSuccess(OrderId orderId, Money total);
    void PresentValidationError(string message);
}

// MyApp.UseCases [CA.UseCases] — use case depends on its own interface
public class PlaceOrderUseCase : IUseCase<PlaceOrderInput, Unit>
{
    private readonly ICustomerRepository _customers; // ✓ CA.Core interface
    private readonly IOrderPresenter _presenter;     // ✓ own ring interface

    public async Task<Unit> Execute(PlaceOrderInput input)
    {
        // ... business logic ...
        _presenter.PresentSuccess(order.Id, order.Total);
        return Unit.Value;
    }
}

// MyApp.Adapters [CA.Adapters + CA.Presenter]
// Implements the use case output boundary — pointing inward ✓
public class OrderConfirmationPresenter : IOrderPresenter, IPresenter<OrderConfirmationViewModel>
{
    public OrderConfirmationViewModel? Result { get; private set; }

    public void PresentSuccess(OrderId orderId, Money total)
    {
        Result = new OrderConfirmationViewModel
        {
            OrderId = orderId.Value,
            TotalFormatted = $"{total.Amount:N2} {total.Currency}"
        };
    }

    public void PresentValidationError(string message) =>
        Result = new OrderConfirmationViewModel { Error = message };
}

// MyApp.Adapters [CA.Adapters + CA.ViewModel]
public class OrderConfirmationViewModel : IViewModel
{
    public Guid OrderId { get; init; }
    public string? TotalFormatted { get; init; }
    public string? Error { get; init; }
}
```

### Violation: Use Case depends on ViewModel directly

```csharp
// XMoleculesBricks0001: 'PlaceOrderUseCase' (CA.UseCases) depends on
// 'OrderConfirmationViewModel' (CA.Adapters).
// Dependency denied by rule 'CA-UseCases-no-Adapters'.
public class PlaceOrderUseCase : IUseCase<PlaceOrderInput, OrderConfirmationViewModel>
{
    // returning ViewModel directly couples use case to adapter ring
}
```

---

## UC-C06: Gateway as Persistence Boundary

**Problem:** Persistence logic must not leak into the use case ring. The
gateway pattern keeps persistence in the adapter ring and exposes a data-access
interface to the use case ring.

**Building block used:** Clean Architecture Pack + DDD Pack (optional)

### Structure

```csharp
// MyApp.Core [CA.Core] — data access interface lives in inner ring
public interface IOrderRepository
{
    Task<Order?> FindByIdAsync(OrderId id);
    Task SaveAsync(Order order);
}

// MyApp.Adapters [CA.Adapters + CA.Gateway]
// SqlOrderGateway implements the CA.Core interface
public class SqlOrderGateway : IGateway, IOrderRepository  // ✓ suffix Gateway
{
    private readonly OrderDbContext _db;  // framework dependency stays here ✓

    public async Task<Order?> FindByIdAsync(OrderId id) =>
        await _db.Orders.FindAsync(id.Value);

    public async Task SaveAsync(Order order)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync();
    }
}

// MyApp.UseCases [CA.UseCases] — depends on interface, not implementation
public class CancelOrderUseCase : IUseCase<CancelOrderInput, Unit>
{
    private readonly IOrderRepository _orders; // ✓ CA.Core interface

    public async Task<Unit> Execute(CancelOrderInput input)
    {
        var order = await _orders.FindByIdAsync(input.OrderId);
        order?.Cancel();
        await _orders.SaveAsync(order!);
        return Unit.Value;
    }
}
```

---

## UC-C07: Clean Architecture + DDD in the Entities Ring

**Problem:** The project uses DDD tactical patterns in the Entities ring.
DDD roles and Clean Architecture roles must coexist without violations.

**Building block used:** Clean Architecture Pack + DDD Pack

The Clean Architecture Pack ships combination rules that allow DDD roles to
coexist with `CA.Core` (additive). This use case shows what that looks like
in practice.

### Role Assignment

```csharp
// Types in MyApp.Core carry both CA.Core and their DDD role.
// No conflict — combination is Additive per the pack's combination rules.

[Role("CA.Core")]   // from assembly-level attribute
// [Role("AggregateRoot")] via base class AggregateRoot<OrderId>

public class Order : AggregateRoot<OrderId>  // roles: CA.Core + AggregateRoot
{
    public override OrderId Id { get; }

    public void Place() => Raise(new OrderPlacedDomainEvent(Id));
}

// roles: CA.Core + DomainEvent  (both valid, additive)
public class OrderPlacedDomainEvent : IDomainEvent { }

// roles: CA.Core + ValueObject
public sealed class Money : ValueObject { ... }
```

### Dependency Rules Active Simultaneously

Both the Clean Architecture pack rules and the DDD pack rules are active.
The more restrictive set applies:

| Source | Target | Decision | Source Rule |
|---|---|---|---|
| `CA.Core` | `CA.UseCases` | Deny | CA-Core-no-UseCases |
| `AggregateRoot` | `Repository` | Deny | DDD: AggregateRoot-no-Repository |
| `ValueObject` | `DomainService` | Deny | DDD: ValueObject-no-services |
| `DomainEvent` | `CA.Adapters` | Deny | CA-Core-no-Adapters |

### Use Case Ring with Application Services

```csharp
// roles: CA.UseCases + ApplicationService (additive)
public class PlaceOrderApplicationService
    : IApplicationService, IUseCase<PlaceOrderInput, OrderId>
{
    private readonly IOrderRepository _orders; // ✓ CA.Core interface
    private readonly OrderFactory _factory;    // ✓ CA.Core + Factory
    private readonly IOrderPresenter _presenter; // ✓ own ring interface

    public async Task<OrderId> Execute(PlaceOrderInput input)
    {
        var order = _factory.Create(OrderId.New());
        // ... add lines ...
        order.Place();
        await _orders.SaveAsync(order);
        _presenter.PresentSuccess(order.Id, order.Total);
        return order.Id;
    }
}
```

---

## UC-C08: Full Example — Order Placement

**Problem:** Set up a complete, self-consistent Clean Architecture project
for an order placement use case, combining all Clean Architecture building
blocks.

**Building blocks used:** Clean Architecture Pack, DDD Pack, Naming Conventions,
Member Cardinality

### Project Structure

```
MyApp.Core              [CA.Core]
  ├── Order.cs          [CA.Core + AggregateRoot]
  ├── OrderLine.cs      [CA.Core + Entity]
  ├── Money.cs          [CA.Core + ValueObject]
  ├── OrderId.cs        [CA.Core + Identity]
  ├── IOrderRepository.cs [CA.Core + Repository]
  └── Events/
      └── OrderPlacedDomainEvent.cs [CA.Core + DomainEvent]

MyApp.UseCases          [CA.UseCases]
  ├── PlaceOrderUseCase.cs     [CA.UseCases + ApplicationService] ✓ suffix UseCase
  ├── IOrderPresenter.cs       [CA.UseCases] — output boundary interface
  └── PlaceOrderInput.cs       [CA.UseCases] — input data structure

MyApp.Adapters          [CA.Adapters]
  ├── OrderController.cs       [CA.Adapters + CA.Controller] ✓ suffix Controller
  ├── OrderConfirmationPresenter.cs [CA.Adapters + CA.Presenter] ✓ suffix Presenter
  ├── OrderConfirmationViewModel.cs [CA.Adapters + CA.ViewModel] ✓ suffix ViewModel
  └── SqlOrderGateway.cs       [CA.Adapters + CA.Gateway] ✓ suffix Gateway

MyApp.Infrastructure    [CA.Frameworks]
  ├── Program.cs        — DI composition root
  ├── OrderDbContext.cs — EF Core context
  └── appsettings.json
```

### Dependency Flow

```
OrderController (CA.Adapters)
  → calls PlaceOrderUseCase.Execute() (CA.UseCases) ✓ inward
    → uses IOrderRepository (CA.Core) ✓ inward
    → calls IOrderPresenter.PresentSuccess() (CA.UseCases own interface) ✓ same ring
      ← implemented by OrderConfirmationPresenter (CA.Adapters) ✓ via DI

SqlOrderGateway (CA.Adapters)
  → implements IOrderRepository (CA.Core) ✓ inward
  → depends on OrderDbContext (CA.Frameworks) ← only allowed at adapter/framework level

Program.cs (CA.Frameworks)
  → wires everything via DI ✓ outermost ring knows everything
```

### Core Assembly

```csharp
[assembly: Role("CA.Core")]

// Identity
public readonly record struct OrderId(Guid Value) : IIdentity { } // ✓ suffix Id

// Value Object
public sealed class Money : ValueObject
{
    public decimal Amount { get; }  // ✓ immutable
    public string Currency { get; }
    public Money(decimal amount, string currency) { Amount = amount; Currency = currency; }
    public static readonly Money Zero = new(0, "CHF");
    protected override IEnumerable<object?> GetEqualityComponents() => [Amount, Currency];
}

// Aggregate Root
public class Order : AggregateRoot<OrderId>  // roles: CA.Core + AggregateRoot
{
    public override OrderId Id { get; }
    public Money Total { get; private set; } = Money.Zero;

    public Order(OrderId id) => Id = id;

    public void AddLine(int quantity, Money unitPrice)
        => Total = new Money(Total.Amount + unitPrice.Amount * quantity, Total.Currency);

    public void Place() => Raise(new OrderPlacedDomainEvent(Id, Total)); // ✓
}

// Domain Event
public class OrderPlacedDomainEvent : IDomainEvent // roles: CA.Core + DomainEvent ✓
{
    public OrderId OrderId { get; }
    public Money Amount { get; }
    public OrderPlacedDomainEvent(OrderId orderId, Money amount)
        { OrderId = orderId; Amount = amount; }
}

// Repository interface (in inner ring)
public interface IOrderRepository
{
    Task<Order?> FindByIdAsync(OrderId id);
    Task SaveAsync(Order order);
}
```

### Use Cases Assembly

```csharp
[assembly: Role("CA.UseCases")]

// Output boundary — declared in use case ring so use case can depend on it ✓
public interface IOrderPresenter
{
    void PresentSuccess(OrderId orderId, Money total);
    void PresentFailure(string reason);
}

// Input data structure
public record PlaceOrderInput(
    IReadOnlyList<(int Quantity, Money UnitPrice)> Lines);

// Use case — roles: CA.UseCases + ApplicationService
public class PlaceOrderUseCase   // ✓ suffix UseCase
    : IUseCase<PlaceOrderInput, Unit>
{
    private readonly IOrderRepository _orders; // ✓ CA.Core interface (inward)
    private readonly IOrderPresenter _presenter; // ✓ own ring interface

    public PlaceOrderUseCase(
        IOrderRepository orders, IOrderPresenter presenter)
    {
        _orders = orders;
        _presenter = presenter;
    }

    public async Task<Unit> Execute(PlaceOrderInput input)
    {
        var order = new Order(OrderId.New());
        foreach (var (qty, price) in input.Lines)
            order.AddLine(qty, price);

        order.Place();
        await _orders.SaveAsync(order);
        _presenter.PresentSuccess(order.Id, order.Total);
        return Unit.Value;
    }
}
```

### Adapters Assembly

```csharp
[assembly: Role("CA.Adapters")]

// Controller — roles: CA.Adapters + CA.Controller
public class OrderController : IController  // ✓ suffix Controller
{
    private readonly PlaceOrderUseCase _useCase;
    private readonly OrderConfirmationPresenter _presenter;

    public OrderController(PlaceOrderUseCase useCase,
                           OrderConfirmationPresenter presenter)
    {
        _useCase = useCase;
        _presenter = presenter;
    }

    public async Task<OrderConfirmationViewModel> PlaceOrderAsync(
        PlaceOrderInput input)
    {
        await _useCase.Execute(input);
        return _presenter.Result!;
    }
}

// Presenter — roles: CA.Adapters + CA.Presenter
public class OrderConfirmationPresenter  // ✓ suffix Presenter
    : IPresenter<OrderConfirmationViewModel>, IOrderPresenter
{
    public OrderConfirmationViewModel? Result { get; private set; }

    public void PresentSuccess(OrderId orderId, Money total)
        => Result = new OrderConfirmationViewModel
        {
            OrderId = orderId.Value,
            TotalFormatted = $"{total.Amount:N2} {total.Currency}"
        };

    public void PresentFailure(string reason)
        => Result = new OrderConfirmationViewModel { Error = reason };
}

// ViewModel — roles: CA.Adapters + CA.ViewModel
public class OrderConfirmationViewModel : IViewModel  // ✓ suffix ViewModel
{
    public Guid OrderId { get; init; }
    public string? TotalFormatted { get; init; }
    public string? Error { get; init; }
}

// Gateway — roles: CA.Adapters + CA.Gateway
public class SqlOrderGateway : IGateway, IOrderRepository  // ✓ suffix Gateway
{
    private readonly OrderDbContext _db;  // framework dep stays in adapter ✓
    public SqlOrderGateway(OrderDbContext db) => _db = db;

    public async Task<Order?> FindByIdAsync(OrderId id) =>
        await _db.Orders.FindAsync(id.Value);

    public async Task SaveAsync(Order order)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync();
    }
}
```

### Active Rules Summary

| Rule | Source | Target | Decision |
|---|---|---|---|
| CA-Core-no-UseCases | `CA.Core` | `CA.UseCases` | Error |
| CA-Core-no-Adapters | `CA.Core` | `CA.Adapters` | Error |
| CA-Core-no-Frameworks | `CA.Core` | `CA.Frameworks` | Error |
| CA-UseCases-no-Adapters | `CA.UseCases` | `CA.Adapters` | Error |
| CA-UseCases-no-Frameworks | `CA.UseCases` | `CA.Frameworks` | Error |
| CA-Adapters-no-Frameworks | `CA.Adapters` | `CA.Frameworks` | Warning |
| DDD: AggregateRoot-no-Repository | `AggregateRoot` | `Repository` | Error |
| DDD: ValueObject-no-services | `ValueObject` | `DomainService` | Error |
| DDD: DomainEvent-pure | `DomainEvent` | `DomainService` | Error |

---

## UC-C09: AI-Generated Code Safety Net

**Problem:** The team uses an AI code generator to scaffold Clean Architecture
components. Generators commonly violate the Dependency Rule by pulling in
framework types or adapter types into inner rings.

**Building blocks used:** Clean Architecture Pack (passive detection)

### Common Generator Violations

**Ring violation — framework in use case:**

```csharp
// Generated in MyApp.UseCases [CA.UseCases]
// XMoleculesBricks0001: depends on HttpClient (CA.Frameworks)
// Denied by CA-UseCases-no-Frameworks
public class FetchOrderUseCase : IUseCase<Guid, OrderDto>
{
    private readonly HttpClient _http; // ← generator pulled in framework type
}
```

**Ring violation — ViewModel in use case:**

```csharp
// Generated in MyApp.UseCases [CA.UseCases]
// XMoleculesBricks0001: depends on OrderConfirmationViewModel (CA.Adapters)
// Denied by CA-UseCases-no-Adapters
public class PlaceOrderUseCase : IUseCase<PlaceOrderInput, OrderConfirmationViewModel>
{
    // returning ViewModel couples use case to adapter ring
}
```

**Naming violation:**

```csharp
// XMoleculesBricks0020: 'PlaceOrder' does not end with 'UseCase'
public class PlaceOrder : IUseCase<PlaceOrderInput, Unit> { }

// XMoleculesBricks0020: 'OrderPresentation' does not end with 'Presenter'
public class OrderPresentation : IPresenter<OrderConfirmationViewModel> { }
```

**Missing Execute (cardinality):**

```csharp
// XMoleculesBricks0003: 'FetchOrderUseCase' does not declare 'Execute'
public class FetchOrderUseCase : IUseCase<Guid, OrderDto>
{
    public Task<OrderDto> Get(Guid id) => ...; // wrong method name
}
```

### IDE Feedback Loop

All violations appear immediately in the IDE as the developer accepts
generated code. The developer can correct names, remove forbidden dependencies,
and rename entry point methods before committing — without a separate
architecture review step.

### Phase 2: Policy Export as Generator Context

With Phase 2 active, the generator receives the Clean Architecture policy:

```json
{
  "architectureStyle": "CleanArchitecture",
  "rings": [
    { "role": "CA.Core",       "ring": 1, "inwardOnly": true },
    { "role": "CA.UseCases",   "ring": 2, "inwardOnly": true },
    { "role": "CA.Adapters",   "ring": 3, "inwardOnly": true },
    { "role": "CA.Frameworks", "ring": 4 }
  ],
  "namingConventions": [
    { "marker": "IUseCase<,>",    "pattern": "UseCase",    "position": "Suffix" },
    { "marker": "IPresenter<>",   "pattern": "Presenter",  "position": "Suffix" },
    { "marker": "IGateway",       "pattern": "Gateway",    "position": "Suffix" },
    { "marker": "IController",    "pattern": "Controller", "position": "Suffix" },
    { "marker": "IViewModel",     "pattern": "ViewModel",  "position": "Suffix" }
  ]
}
```

The generator uses this to produce conformant ring assignments and naming
from the first attempt.

---

## Clean Architecture Validation Summary

| UC | Pattern validated | Building blocks |
|---|---|---|
| UC-C01 | Ring role assignment (assembly, type, marker) | Clean Architecture Pack |
| UC-C02 | Dependency Rule all four ring crossings | Clean Architecture Pack (policy) |
| UC-C03 | Naming conventions per ring | Naming Conventions |
| UC-C04 | Use case Execute contract | Member Cardinality |
| UC-C05 | Presenter output boundary | Clean Architecture Pack |
| UC-C06 | Gateway persistence boundary | Clean Architecture Pack + DDD Pack |
| UC-C07 | CA + DDD coexistence (additive roles) | Clean Architecture Pack + DDD Pack |
| UC-C08 | Full project composition | All building blocks |
| UC-C09 | AI-generated code safety net | All (passive) |

All Clean Architecture use cases are expressible with Layer 1 and Layer 2
building blocks. The Dependency Rule maps directly to Bricks Deny rules. The
Dependency Inversion principle is the structural mechanism that makes inner
rings compilable without outer ring knowledge — Bricks enforces it
automatically once the roles are assigned.
