# UC-L1-03: Alias Marker Adapts An Existing Class To A Canonical Role

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`

## Goal

- classify an existing class without placing `Role` directly on it
- show that alias mapping is part of the current core role-assignment model
- keep the effective role inspectable

## Scenario

We assume a class already exists and should be treated semantically as a
contract, but we do not want to annotate it directly with the generic
`[Role("Contracts")]` marker.

We define:

- one domain-specific marker attribute
- one canonical role that this marker resolves to
- one existing class that receives the marker attribute

## Minimal Example

```csharp
using NMolecules.Bricks;

[RoleAlias("Contracts")]
public sealed class ContractDtoAttribute : Attribute
{
}

[ContractDto]
public sealed class InvoiceDto
{
}
```

## Expected Result

No violation is produced.

The important result is semantic classification through the alias marker:

- `InvoiceDto` resolves to `Contracts`
- the effective role is indirect, but still deterministic

Conceptually this can be inspected as:

```csharp
new BrickResolvedRoles
{
    Element = new BrickElement
    {
        Kind = BrickElementKind.Type,
        DisplayName = "InvoiceDto",
        FullName = "InvoiceDto"
    },
    EffectiveRoles = new[] { "Contracts" },
    AppliedAssignments = new[]
    {
        new BrickRoleAssignment
        {
            RoleName = "Contracts",
            Source = BrickAssignmentSource.AliasMapping
        }
    }
};
```

## What This Proves

- Layer 1 is broader than direct `Role` usage alone
- existing classes can be adapted with domain-specific markers
- alias-based role assignment feeds the same rule engine as direct roles

## Boundary

This use case still assumes that you can place a custom marker attribute on the
target class.

If you cannot touch the type at all, for example because it comes from legacy
code, generated code, or an external assembly, use external role assignment
instead. See `UC-L1-09`.
