# UC-L1-09: External Policy Assigns A Role To An Untouchable Existing Type

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- attach a role to an existing type that cannot be modified directly
- avoid any source-code attribute on the target type
- keep the resulting role assignment deterministic and inspectable

## Scenario

We assume a type already exists, but we cannot place either `Role` or a custom
alias marker on it.

Typical reasons:

- the type is in a legacy project we do not want to edit yet
- the type is generated
- the type comes from another assembly or package

We still want Bricks to treat that type semantically as `Contracts`.

## Minimal Example

Existing type:

```csharp
namespace Billing.Transport;

public sealed class InvoiceDto
{
}
```

External policy input:

```json
{
  "roleAssignments": [
    {
      "selector": {
        "fullName": "Billing.Transport.InvoiceDto"
      },
      "roleName": "Contracts",
      "source": "ExternalConfig",
      "precedence": {
        "specificity": "Element",
        "authority": "External"
      }
    }
  ]
}
```

## Expected Result

No violation is produced.

The important result is semantic classification without touching the source
type:

- `Billing.Transport.InvoiceDto` resolves to `Contracts`
- the role is attached externally, but the effective role is still explicit

Conceptually this can be inspected as:

```csharp
new BrickResolvedRoles
{
    Element = new BrickElement
    {
        Kind = BrickElementKind.Type,
        DisplayName = "InvoiceDto",
        FullName = "Billing.Transport.InvoiceDto"
    },
    CandidateAssignments = new[]
    {
        new BrickRoleAssignment
        {
            RoleName = "Contracts",
            Source = BrickAssignmentSource.ExternalConfig
        }
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

- Layer 1 must support role assignment beyond source attributes
- existing or external types can still participate in the semantic model
- external assignment is the right mechanism when the target type is untouchable
