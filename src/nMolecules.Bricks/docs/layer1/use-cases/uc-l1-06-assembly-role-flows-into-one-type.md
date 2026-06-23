# UC-L1-06: Assembly Role Flows Into One Type

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- assign a role at assembly scope
- show that a contained type inherits the broader semantic assignment
- make effective roles inspectable before any suppression happens

## Scenario

We define one assembly-wide role:

- `Billing.Domain`

And one contained type without a direct role.

## Minimal Example

```csharp
using NMolecules.Bricks;

[assembly: Role("Billing.Domain")]

namespace Billing;

public sealed class InvoicePolicy
{
}
```

## Expected Resolution Result

No violation is produced.

Conceptually the type still resolves with a role:

```csharp
new BrickResolvedRoles
{
    Element = new BrickElement
    {
        Kind = BrickElementKind.Type,
        DisplayName = "InvoicePolicy",
        FullName = "Billing.InvoicePolicy"
    },
    EffectiveRoles = new[] { "Billing.Domain" }
};
```

## What This Proves

- broader scopes can contribute semantic roles
- rule evaluation works on effective roles, not only on direct annotations
- resolution matters even before conflicts exist
