# UC-L1-10: Base-Type Alias Adapts Derived Types

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- assign a canonical role through a known base type
- classify derived types without touching each derived class
- make hierarchy-based adaptation explicit

## Scenario

We assume several existing types inherit from a common base class.

We define:

- one known base type
- one canonical role mapped to that base type
- one derived type that should inherit the semantic meaning

## Minimal Example

Existing type hierarchy:

```csharp
namespace Billing.Legacy;

public abstract class ContractMessageBase
{
}

public sealed class InvoiceDto : ContractMessageBase
{
}
```

Alias declaration:

```csharp
new BrickAlias
{
    AliasName = "Billing.Legacy.ContractMessageBase",
    CanonicalRoleName = "Contracts"
};
```

## Expected Result

No violation is produced.

The important result is semantic adaptation via inheritance:

- `InvoiceDto` resolves to `Contracts`
- the role comes from the matched base type, not from a direct annotation

Conceptually this can be inspected as:

```csharp
new BrickResolvedRoles
{
    Element = new BrickElement
    {
        Kind = BrickElementKind.Type,
        DisplayName = "InvoiceDto",
        FullName = "Billing.Legacy.InvoiceDto"
    },
    EffectiveRoles = new[] { "Contracts" },
    AppliedAssignments = new[]
    {
        new BrickRoleAssignment
        {
            RoleName = "Contracts",
            Source = BrickAssignmentSource.AliasMapping
        }
    }
};
```

## What This Proves

- role adaptation can follow inheritance hierarchies
- existing derived types do not need individual annotations
- base-type aliasing is a practical bridge for legacy class families
