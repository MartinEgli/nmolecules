# UC-L1-17: Additive Roles Accumulate On One Element

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- show that multiple roles may coexist on one element
- prove that role resolution is not always replacement
- make additive accumulation explicit and inspectable

## Scenario

We define one type that should carry both:

- `Contracts`
- `Shared`

Policy declares that this pair is `Additive`.

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
      "roleName": "Shared",
      "source": "ExternalConfig",
      "precedence": {
        "specificity": "Element",
        "authority": "External"
      }
    }
  ],
  "combinationRules": [
    {
      "name": "Contracts-plus-Shared",
      "leftRoles": "Contracts",
      "rightRoles": "Shared",
      "kind": "Additive"
    }
  ]
}
```

## Expected Resolution Result

Both roles remain effective:

```csharp
new BrickResolvedRoles
{
    EffectiveRoles = new[] { "Contracts", "Shared" },
    AppliedAssignments = new[]
    {
        new BrickRoleAssignment
        {
            RoleName = "Contracts",
            Source = BrickAssignmentSource.Attribute
        },
        new BrickRoleAssignment
        {
            RoleName = "Shared",
            Source = BrickAssignmentSource.ExternalConfig
        }
    },
    SuppressedAssignments = Array.Empty<BrickRoleAssignment>(),
    Conflicts = Array.Empty<BrickRoleConflict>()
};
```

## What This Proves

- role resolution is multi-valued when policy allows it
- additive combinations are explicit, not accidental merges
- additive multi-role means distinct roles, not duplicated copies of the same role
- later rule evaluation can operate on the full effective role set
