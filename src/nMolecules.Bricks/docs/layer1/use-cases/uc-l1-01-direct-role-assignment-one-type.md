# UC-L1-01: Direct Role Assignment On One Type

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- assign one semantic role directly
- make the semantic model visible without any rules yet

## Scenario

We define one type and annotate it directly.

No rule evaluation happens yet.

## Minimal Example

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class InvoicePolicy
{
}
```

## Expected Result

No violation is produced.

The important result is semantic classification:

- `InvoicePolicy` resolves to `Billing.Domain`

## What This Proves

- Bricks starts with semantic labeling, not with rules
- direct assignment is the most basic path into the model
- Layer 1 is already useful as a semantic classification mechanism
