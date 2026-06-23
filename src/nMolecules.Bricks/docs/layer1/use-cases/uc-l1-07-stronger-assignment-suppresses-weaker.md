# UC-L1-07: Stronger Assignment Suppresses Weaker Assignment

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- assign one role directly on a type
- assign a conflicting role from external policy
- let precedence suppress the weaker assignment deterministically

## Scenario

We define two roles in the same exclusive family and give the direct
source-code assignment higher authority than the external one:

- `Business.Sales`
- `Business.Support`

One type receives:

- a direct role assignment in source code
- a conflicting external assignment from policy config

Because both assignments apply to the same element and the combination is
`Exclusive`, the weaker assignment must be suppressed explicitly.

## Minimal Example

Source code:

```csharp
using NMolecules.Bricks;

[Role("Business.Sales")]
public sealed class CustomerCaseHandler
{
}
```

External policy input:

```json
{
  "roleAssignments": [
    {
      "selector": { "fullName": "CustomerCaseHandler" },
      "roleName": "Business.Support",
      "source": "ExternalConfig",
      "precedence": {
        "specificity": "Element",
        "authority": "External"
      }
    }
  ],
  "combinationRules": [
    {
      "name": "Business-areas-are-exclusive",
      "leftRoles": "Business.*",
      "rightRoles": "Business.*",
      "kind": "Exclusive"
    }
  ]
}
```

## Expected Resolution Result

At minimum, resolution must remain inspectable:

```csharp
new BrickResolvedRoles
{
    Element = new BrickElement
    {
        Kind = BrickElementKind.Type,
        DisplayName = "CustomerCaseHandler",
        FullName = "CustomerCaseHandler"
    },
    CandidateAssignments = new[]
    {
        new BrickRoleAssignment
        {
            RoleName = "Business.Sales",
            Source = BrickAssignmentSource.Attribute
        },
        new BrickRoleAssignment
        {
            RoleName = "Business.Support",
            Source = BrickAssignmentSource.ExternalConfig
        }
    },
    EffectiveRoles = new[] { "Business.Sales" },
    AppliedAssignments = new[]
    {
        new BrickRoleAssignment
        {
            RoleName = "Business.Sales",
            Source = BrickAssignmentSource.Attribute
        }
    },
    SuppressedAssignments = new[]
    {
        new BrickRoleAssignment
        {
            RoleName = "Business.Support",
            Source = BrickAssignmentSource.ExternalConfig
        }
    },
    Conflicts = Array.Empty<BrickRoleConflict>()
};
```

## What This Proves

- assignment provenance matters
- precedence must be explainable
- suppression must be visible
- effective roles alone are not enough for debugging
