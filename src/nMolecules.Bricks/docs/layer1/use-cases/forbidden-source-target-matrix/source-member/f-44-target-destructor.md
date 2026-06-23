# F-44: Forbidden Dependency From Member To Destructor

Layer: 1 - Core  
Depends on: ../../../foundational-concept.md, ../../bricks-layer1-core-v2.md, ../../uc-l1-18-forbidden-source-target-matrix.md

## Goal

- define the forbidden source-target combination 'Member -> Destructor'
- make the shape combination explicit instead of leaving it implicit in the overview matrix
- keep the expected forbidden-dependency result clear and repeatable

## Scenario

We define one forbidden dependency whose source shape is 'Member' and whose target shape is 'Destructor'.
The exact normalization of this pair is defined in UC-L1-18. This file is the granular case entry for that pair.

## Policy

### Abstract Concept

```csharp
new BrickPolicy
{
    Name = "Layer1.Forbidden.F-44",
    Rules =
    [
        new BrickRule
        {
            Name = "Forbidden dependency from Member to Destructor",
            SourceRoles = BrickRoleSelector.Exact("Source"),
            TargetRoles = BrickRoleSelector.Exact("Target"),
            Dependencies = new BrickDependencySelector { Kind = null },
            Scope = BrickScope.Member,
            Decision = BrickDecision.Deny,
            Severity = BrickSeverity.Error
        }
    ],
    DefaultDecision = BrickPermissionDefault.Allow
};
```

This abstract form uses `Source` and `Target` as generic role placeholders.
The explicit example below binds them to `Billing.Domain` and `Infrastructure`.

### Explicit Example

```csharp
using NMolecules.Bricks;

[assembly: Rule(
    "F-44",
    "Billing.Domain",
    "Infrastructure",
    RuleMode.ForbidDependency,
    "Rule {rule}: {source} must not depend on {target}")]
```

The exact "Member -> Destructor" shape in this file comes from the
concrete dependency evidence shown below, not from a separate attribute
parameter in the current explicit API surface.

## Minimal Example

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public void Handle()
    {
        var target = new ForbiddenFinalizableTarget();
    }
}

[Role("Infrastructure")]
public sealed class ForbiddenFinalizableTarget
{
    ~ForbiddenFinalizableTarget()
    {
    }
}
```

## Expected Result

A forbidden dependency violation is produced when a source element with the selected source role depends on a target element with the forbidden target role under this source-target shape combination.

The important result is that this exact pair is covered explicitly, not only indirectly through the overview matrix.

## What This Proves

- the combination has its own explicit Layer 1 entry
- the forbidden-dependency engine remains consistent across all source-target shapes
- the matrix can be navigated as granular use-case files instead of one large table



