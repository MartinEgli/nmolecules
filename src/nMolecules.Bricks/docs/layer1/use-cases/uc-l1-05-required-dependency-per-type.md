# UC-L1-05: One Required Dependency Per Type

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- require every type with role `Application.UseCase`
- to depend on at least one type with role `Application.Port`
- at `Type` scope

## Scenario

We define:

- `Application.UseCase`
- `Application.Port`

Policy:

- every use case type must depend on at least one port type
- evaluation happens at `Type` scope

This is a generic structural completeness rule:
a type may be isolated in the dependency graph, but if it claims the role
`Application.UseCase`, it must collaborate with at least one boundary type.

## Minimal Example

```csharp
using NMolecules.Bricks;

[assembly: Rule(
    "APP-CORE-001",
    "Application.UseCase",
    "Application.Port",
    RuleMode.RequireDependency,
    "Rule {rule}: {source} must depend on at least one {target}")]

[Role("Application.Port")]
public interface IInvoicePort
{
    void Save();
}

[Role("Application.UseCase")]
public sealed class GoodCreateInvoiceUseCase
{
    private readonly IInvoicePort _port;

    public GoodCreateInvoiceUseCase(IInvoicePort port)
    {
        _port = port;
    }
}

[Role("Application.UseCase")]
public sealed class BadCreateInvoiceUseCase
{
    public void Execute()
    {
    }
}
```

## Expected Result

`GoodCreateInvoiceUseCase` satisfies the rule because it depends on
`IInvoicePort`.

`BadCreateInvoiceUseCase` violates the rule because no dependency to a type
with role `Application.Port` exists.

Conceptually this produces a requirement violation like:

```csharp
new BrickViolation
{
    Kind = BrickViolationKind.Requirement,
    RuleName = "APP-CORE-001",
    Source = new BrickElement
    {
        Kind = BrickElementKind.Type,
        DisplayName = "BadCreateInvoiceUseCase",
        FullName = "BadCreateInvoiceUseCase"
    },
    Target = null,
    DependencyKind = null,
    Scope = BrickScope.Type,
    EffectiveSourceRoles = new[] { "Application.UseCase" },
    EffectiveTargetRoles = new[] { "Application.Port" },
    Message = "Rule APP-CORE-001: BadCreateInvoiceUseCase must depend on at least one Application.Port"
};
```

## What This Proves

- `Require` is not the same as `Allow` or `Deny`
- the rule is evaluated per source evaluation unit
- the absence of a dependency can be modeled and reported explicitly
- Layer 1 already supports structural completeness rules, not only forbidden dependencies
