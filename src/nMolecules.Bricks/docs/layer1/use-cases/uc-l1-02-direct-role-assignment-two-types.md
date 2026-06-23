# UC-L1-02: Direct Role Assignment On Two Types

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- classify two elements independently
- show that Layer 1 does not require predefined role packs

## Scenario

We define two types and annotate both directly:

- one domain type
- one infrastructure type

## Minimal Example

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class InvoicePolicy
{
}

[Role("Billing.Infrastructure")]
public sealed class SqlInvoiceRepository
{
}
```

## Expected Result

No violation is produced.

The important result is semantic classification:

- `InvoicePolicy` resolves to `Billing.Domain`
- `SqlInvoiceRepository` resolves to `Billing.Infrastructure`

## What This Proves

- multiple elements can be classified without any rule set
- role names are generic semantic identifiers, not framework enums
- later rules can build on this classification state
