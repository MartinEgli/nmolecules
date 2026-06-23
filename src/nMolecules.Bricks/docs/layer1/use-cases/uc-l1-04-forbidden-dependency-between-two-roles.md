# UC-L1-04: One Forbidden Dependency Between Two Roles

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- define two semantic roles
- assign them directly
- forbid one dependency direction
- get one deterministic violation when the rule is broken

## Scenario

We define:

- `Billing.Domain`
- `Billing.Infrastructure`

Policy:

- `Billing.Domain` must not depend on `Billing.Infrastructure`

## Minimal Example

```csharp
using NMolecules.Bricks;

[assembly: Rule(
    "BILL-CORE-001",
    "Billing.Domain",
    "Billing.Infrastructure",
    RuleMode.ForbidDependency,
    "Rule {rule}: {source} must not depend on {target}")]

[Role("Billing.Infrastructure")]
public sealed class SqlInvoiceRepository
{
}

[Role("Billing.Domain")]
public sealed class InvoicePolicy
{
    private readonly SqlInvoiceRepository _repository;

    public InvoicePolicy(SqlInvoiceRepository repository)
    {
        _repository = repository;
    }
}
```

## Expected Result

The analyzer reports a dependency violation because a type with role
`Billing.Domain` references a type with role `Billing.Infrastructure`.

Conceptually this produces one normalized violation record:

```csharp
new BrickViolation
{
    Kind = BrickViolationKind.Dependency,
    RuleName = "BILL-CORE-001",
    Scope = BrickScope.Type,
    EffectiveSourceRoles = new[] { "Billing.Domain" },
    EffectiveTargetRoles = new[] { "Billing.Infrastructure" }
};
```

## What This Proves

- direct role assignment works
- role-to-role rule evaluation works
- dependency violations are deterministic
- Bricks already provides value before any Layer 2 packs exist
