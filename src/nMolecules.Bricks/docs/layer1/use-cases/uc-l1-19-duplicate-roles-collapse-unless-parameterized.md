# UC-L1-19: Duplicate Roles Collapse Unless Parameterized

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- show that one element may carry multiple roles
- prevent the same unparameterized role from appearing twice
- allow distinct parameterized role instances to coexist

## Scenario

We define one type that receives the same role through more than one
assignment path:

- direct `Contracts`
- external `Contracts`

We also define a conceptual parameterized role family:

- `Adapter(Direction=Inbound)`
- `Adapter(Direction=Outbound)`

## Minimal Example

Source code:

```csharp
using NMolecules.Bricks;

[Role("Contracts")]
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
        "fullName": "InvoiceDto"
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

## Expected Resolution Result

The effective role set contains `Contracts` only once:

```csharp
new BrickResolvedRoles
{
    EffectiveRoles = new[] { "Contracts" },
    AppliedAssignments = new[]
    {
        new BrickRoleAssignment
        {
            RoleName = "Contracts",
            Source = BrickAssignmentSource.Attribute
        },
        new BrickRoleAssignment
        {
            RoleName = "Contracts",
            Source = BrickAssignmentSource.ExternalConfig
        }
    }
};
```

This is not a conflict and not an additive multi-role combination. It is one
effective role supported by multiple assignment sources.

## Parameterized Exception

When the role model distinguishes parameterized role instances, the same role
name may appear more than once if the parameter identity is different.

Conceptually:

```csharp
new[]
{
    "Adapter(Direction=Inbound)",
    "Adapter(Direction=Outbound)"
}
```

These do not collapse because they are not the same effective role instance.

## What This Proves

- multi-role does not mean duplicate role names
- duplicate elimination happens before rule evaluation
- one effective role may still retain multiple assignment sources
- parameterized role instances are the valid exception to duplicate collapse
