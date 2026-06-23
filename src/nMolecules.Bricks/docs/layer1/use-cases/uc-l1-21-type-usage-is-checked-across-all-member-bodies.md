# UC-L1-21: Forbidden Dependency Coverage Across All Member Bodies And Operations

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`, `uc-l1-04-forbidden-dependency-between-two-roles.md`

## Goal

- make explicit that forbidden type usage is checked everywhere in code
- show that dependency evaluation is not limited to class-level declarations
- cover methods, functions, procedures, members, fields, properties, constructors, destructors, and nested operations

## Scenario

We define:

- `Billing.Domain`
- `Billing.Infrastructure`

Policy:

- `Billing.Domain` must not depend on `Billing.Infrastructure`

The same forbidden target type is then used in multiple places inside code:

- field declaration
- field initializer
- property type
- property accessor
- method body
- local function
- constructor body
- destructor body

## Minimal Example

```csharp
using NMolecules.Bricks;

[assembly: Rule(
    "BILL-CORE-003",
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
    private readonly SqlInvoiceRepository _field;
    private readonly SqlInvoiceRepository _initialized = new SqlInvoiceRepository();

    public SqlInvoiceRepository Repository => _field;

    public InvoicePolicy(SqlInvoiceRepository repository)
    {
        _field = repository;

        SqlInvoiceRepository CreateLocal()
        {
            return new SqlInvoiceRepository();
        }

        _ = CreateLocal();
    }

    public SqlInvoiceRepository GetRepository()
    {
        return _field;
    }

    ~InvoicePolicy()
    {
        _ = _field;
    }
}
```

## Expected Result

All of these usages participate in dependency evaluation.

The reporting strategy may collapse them into one violation or surface multiple
pieces of evidence, but the semantic rule is stable:

- forbidden field usage counts
- forbidden property usage counts
- forbidden method or function usage counts
- forbidden constructor and destructor usage counts

## What This Proves

- Bricks checks type usage everywhere it becomes structurally relevant
- member-level evidence is not restricted to one syntax form
- methods, functions, procedures, members, fields, properties, constructors, destructors, and nested operations are all part of dependency analysis
