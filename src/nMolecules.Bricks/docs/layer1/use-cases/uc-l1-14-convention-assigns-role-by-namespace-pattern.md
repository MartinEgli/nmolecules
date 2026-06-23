# UC-L1-14: Convention Assigns A Role By Namespace Pattern

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- assign a role without direct attributes or explicit external type entries
- use a naming or namespace convention as the assignment source
- keep the resulting role inspectable as a convention-based assignment

## Scenario

We assume the codebase follows a stable namespace pattern:

- everything in `Billing.Contracts.*` should be treated as `Contracts`

No direct role attributes are placed on the types.

## Minimal Example

Source code:

```csharp
namespace Billing.Contracts.Messages;

public sealed class InvoiceDto
{
}
```

Convention input:

```json
{
  "roleAssignments": [
    {
      "selector": {
        "namespacePattern": "Billing.Contracts.*"
      },
      "roleName": "Contracts",
      "source": "Convention",
      "precedence": {
        "specificity": "Namespace",
        "authority": "Derived"
      }
    }
  ]
}
```

## Expected Result

No violation is produced.

The important result is semantic classification through a convention:

- `Billing.Contracts.Messages.InvoiceDto` resolves to `Contracts`
- the assignment is inspectable as `Source = Convention`

## What This Proves

- Layer 1 can classify code even when teams rely on stable naming structure
- convention is weaker than direct or explicit external assignment, but still first-class
- legacy migration can start without touching every file immediately
