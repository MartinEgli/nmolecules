# UC-L1-16: Direct Element Role Overrides Namespace Role

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- show precedence across scopes
- prove that element-level assignment wins over namespace-level assignment
- keep the weaker namespace assignment visible as suppressed

## Scenario

We define:

- a namespace-wide role `Shared`
- one type inside that namespace with an explicit direct role `Contracts`
- an exclusive combination between `Shared` and `Contracts`

## Minimal Example

Source code:

```csharp
using NMolecules.Bricks;

namespace Billing.Contracts;

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
        "namespacePattern": "Billing.Contracts"
      },
      "roleName": "Shared",
      "source": "ExternalConfig",
      "precedence": {
        "specificity": "Namespace",
        "authority": "External"
      }
    }
  ],
  "combinationRules": [
    {
      "name": "Contracts-vs-Shared",
      "leftRoles": "Contracts",
      "rightRoles": "Shared",
      "kind": "Exclusive"
    }
  ]
}
```

## Expected Resolution Result

The direct role wins because `Element` specificity is stronger than `Namespace`.

Conceptually:

```csharp
new BrickResolvedRoles
{
    EffectiveRoles = new[] { "Contracts" },
    SuppressedAssignments = new[]
    {
        new BrickRoleAssignment
        {
            RoleName = "Shared",
            Source = BrickAssignmentSource.ExternalConfig
        }
    }
};
```

## What This Proves

- scope matters in precedence
- direct element intent can narrow a broader namespace classification
- suppressed broader assignments remain inspectable
