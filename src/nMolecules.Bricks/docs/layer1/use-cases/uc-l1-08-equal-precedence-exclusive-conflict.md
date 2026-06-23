# UC-L1-08: Equal-Precedence Exclusive Assignments Surface A Conflict

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- apply two conflicting assignments of equal precedence
- keep the outcome deterministic
- surface a conflict instead of inventing a silent winner

## Scenario

We again use the same exclusive role family:

- `Business.Sales`
- `Business.Support`

But this time both assignments come from equally strong external policy input.

## Minimal Example

```json
{
  "roleAssignments": [
    {
      "selector": { "fullName": "CustomerCaseHandler" },
      "roleName": "Business.Sales",
      "source": "ExternalConfig",
      "precedence": {
        "specificity": "Element",
        "authority": "External"
      }
    },
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

The engine must not silently choose one role.

Conceptually the result looks like:

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
            Source = BrickAssignmentSource.ExternalConfig
        },
        new BrickRoleAssignment
        {
            RoleName = "Business.Support",
            Source = BrickAssignmentSource.ExternalConfig
        }
    },
    EffectiveRoles = Array.Empty<string>(),
    AppliedAssignments = Array.Empty<BrickRoleAssignment>(),
    SuppressedAssignments = Array.Empty<BrickRoleAssignment>(),
    Conflicts = new[]
    {
        new BrickRoleConflict()
    }
};
```

## What This Proves

- determinism does not require arbitrary tie-breaking
- conflicts are first-class outputs
- policy authors can see when their assignment model is underspecified
