# UC-L1-12: Inherited-Interface Alias Reaches Concrete Implementations

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- assign a canonical role through a parent interface
- let that semantic meaning flow through an inherited interface chain
- classify a concrete implementation at the end of the chain

## Scenario

We assume the semantic anchor is not implemented directly by the class.
Instead:

- interface `IContractMessage` defines the semantic anchor
- interface `IInvoiceContract` inherits from `IContractMessage`
- class `InvoiceDto` implements `IInvoiceContract`

## Minimal Example

Existing interfaces and type:

```csharp
namespace Billing.Transport;

public interface IContractMessage
{
}

public interface IInvoiceContract : IContractMessage
{
}

public sealed class InvoiceDto : IInvoiceContract
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

The important result is semantic adaptation through the inherited-interface
chain:

- `InvoiceDto` resolves to `Contracts`
- the role originates at `IContractMessage`
- `IInvoiceContract` does not need its own duplicate mapping

## What This Proves

- interface inheritance is part of the semantic lookup path
- one alias declaration can classify a broader family of implementations
- Bricks can adapt existing type systems without flattening their abstraction hierarchy
