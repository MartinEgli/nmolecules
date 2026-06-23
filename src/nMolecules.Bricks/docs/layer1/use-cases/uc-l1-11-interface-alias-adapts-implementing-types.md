# UC-L1-11: Interface Alias Adapts Implementing Types

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- assign a canonical role through an implemented interface
- classify concrete types without direct role annotations
- show that interface-based semantics are part of the core model

## Scenario

We assume a type implements a known interface whose semantic meaning is
already understood by policy.

We define:

- one interface
- one canonical role mapped to that interface
- one implementing type

## Minimal Example

Existing types:

```csharp
namespace Billing.Transport;

public interface IContractMessage
{
}

public sealed class InvoiceDto : IContractMessage
{
}
```

Alias declaration:

```csharp
new BrickAlias
{
    AliasName = "Billing.Transport.IContractMessage",
    CanonicalRoleName = "Contracts"
};
```

## Expected Result

No violation is produced.

The important result is semantic adaptation via interface implementation:

- `InvoiceDto` resolves to `Contracts`
- the role comes from the matched interface

## What This Proves

- interfaces can carry semantic meaning for concrete implementations
- Bricks can classify types by contract shape, not only by class inheritance
- interface aliasing is useful for existing abstractions you cannot rewrite
