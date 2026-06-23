# UC-L1-13: Namespace Role Flows Into Contained Types

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- assign a role at namespace scope
- show that contained types inherit the namespace-level semantic assignment
- keep the effective role inspectable without direct type annotations

## Scenario

We define one namespace-wide role:

- `Contracts`

And two contained types without direct role markers.

## Minimal Example

Source code:

```csharp
namespace Billing.Contracts;

public sealed class InvoiceDto
{
}

public sealed class CreditNoteDto
{
}
```

External policy input:

```json
{
  "roleAssignments": [
    {
      "selector": {
        "namespacePattern": "Billing.Contracts"
      },
      "roleName": "Contracts",
      "source": "ExternalConfig",
      "precedence": {
        "specificity": "Namespace",
        "authority": "External"
      }
    }
  ]
}
```

## Expected Result

No violation is produced.

The important result is semantic classification through namespace scope:

- `Billing.Contracts.InvoiceDto` resolves to `Contracts`
- `Billing.Contracts.CreditNoteDto` resolves to `Contracts`
- the role comes from the namespace assignment, not from direct annotations

Conceptually this can be inspected as:

```csharp
new BrickResolvedRoles
{
    Element = new BrickElement
    {
        Kind = BrickElementKind.Type,
        DisplayName = "InvoiceDto",
        NamespaceName = "Billing.Contracts",
        FullName = "Billing.Contracts.InvoiceDto"
    },
    EffectiveRoles = new[] { "Contracts" },
    AppliedAssignments = new[]
    {
        new BrickRoleAssignment
        {
            RoleName = "Contracts",
            Source = BrickAssignmentSource.ExternalConfig
        }
    }
};
```

## What This Proves

- Layer 1 supports semantic assignment at namespace granularity
- contained types can inherit roles from a narrower scope than assembly
- namespace-based assignment is useful for grouping existing code without touching each type
