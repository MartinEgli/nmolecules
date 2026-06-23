# UC-L1-15: Inference Derives A Role From Structural Context

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- derive a role without direct declaration
- show the weakest assignment source in the model
- keep the inferred result visible instead of hiding it

## Scenario

We assume the system infers a helper role from structural context.

Example:

- a type is only referenced from test code
- policy classifies such a type as `TestOnly`

No direct, external, or convention-based assignment is present.

## Minimal Example

```csharp
namespace Billing.Tests.Support;

public sealed class InvoiceBuilder
{
}
```

Conceptual inference result:

```csharp
new BrickRoleAssignment
{
    RoleName = "TestOnly",
    Source = BrickAssignmentSource.Inference,
    Precedence = new BrickAssignmentPrecedence(
        BrickAssignmentSpecificity.Inference,
        BrickAssignmentAuthority.Derived)
};
```

## Expected Result

No violation is produced.

The important result is that the role remains inspectable as inferred:

- `InvoiceBuilder` resolves to `TestOnly`
- the assignment is visible as an inference, not mistaken for an explicit declaration

## What This Proves

- inference is a valid but weakest role source
- the model distinguishes guessed structure from explicit policy intent
- inferred roles can participate in later rule evaluation without pretending to be direct declarations
