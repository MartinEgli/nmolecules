# UC-L1-20: Forbidden Dependency Except Explicitly Allowed Target Types

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`, `uc-l1-04-forbidden-dependency-between-two-roles.md`

## Goal

- forbid a dependency direction in general
- allow only a small, explicitly defined set of target types as exceptions
- make the allowlist visible in the policy instead of relying on convention

## Scenario

We define:

- `Billing.Domain`
- `Billing.Infrastructure`

Policy:

- `Billing.Domain` must not depend on `Billing.Infrastructure`
- except for one explicitly allowed target type

This models a strict allowlist:
only the defined target types may still be used; all other infrastructure
targets remain forbidden.

## Minimal Example

Conceptual policy:

```csharp
new BrickRule
{
    Name = "BILL-CORE-002",
    SourceRoles = BrickRoleSelector.For("Billing.Domain"),
    TargetRoles = BrickRoleSelector.For("Billing.Infrastructure"),
    Dependencies = new BrickDependencySelector(),
    Scope = BrickScope.Type,
    Decision = BrickDecision.Deny,
    Severity = BrickSeverity.Error,
    Exceptions = new BrickRuleExceptionSet
    {
        ExcludedTargetTypes = new[]
        {
            new BrickTypeSelector
            {
                FullyQualifiedTypeName = "Billing.Infrastructure.Clock"
            }
        },
        Reason = "Clock is an explicitly allowed infrastructure exception"
    }
};
```

Source code:

```csharp
using NMolecules.Bricks;

[Role("Billing.Infrastructure")]
public sealed class Clock
{
}

[Role("Billing.Infrastructure")]
public sealed class SqlInvoiceRepository
{
}

[Role("Billing.Domain")]
public sealed class InvoicePolicy
{
    private readonly Clock _clock;
    private readonly SqlInvoiceRepository _repository;

    public InvoicePolicy(Clock clock, SqlInvoiceRepository repository)
    {
        _clock = clock;
        _repository = repository;
    }
}
```

## Expected Result

The dependency from `InvoicePolicy` to `Clock` is excluded from evaluation for
this rule because `Clock` is explicitly allowlisted.

The dependency from `InvoicePolicy` to `SqlInvoiceRepository` still violates
the rule because that target type is not part of the explicit exception set.

Conceptually this means:

- allowed: `Billing.Domain -> Billing.Infrastructure.Clock`
- forbidden: `Billing.Domain -> Billing.Infrastructure.SqlInvoiceRepository`

## What This Proves

- Bricks can express "forbidden by default, except these exact target types"
- explicit target-type exceptions are more precise than broad role exceptions
- the policy remains deterministic because the allowlist is concrete and inspectable
