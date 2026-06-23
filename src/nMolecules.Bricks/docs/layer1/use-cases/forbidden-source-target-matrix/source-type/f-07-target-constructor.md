# F-07: Forbidden Dependency From Type To Constructor

Layer: 1 - Core  
Depends on: ../../../foundational-concept.md, ../../bricks-layer1-core-v2.md, ../../uc-l1-18-forbidden-source-target-matrix.md

## Goal

- define the forbidden source-target combination `Type -> Constructor`
- model object creation as a dependency to a concrete constructor target
- keep the current explicit API example visible as an approximation of that target

## Scenario

The source role is assigned to the containing type and inherited by the member
that performs object creation.

The forbidden target is the addressed constructor. In the concept model this is
a specialized member target, so the rule is evaluated at `BrickScope.Member`
even though the source role originates on a type.

## Policy

### Abstract Concept

```csharp
var sourceTypeRole = new BrickRoleAssignment
{
    Selector = BrickElementSelector.Type("SourceType"),
    RoleName = "Source",
    Source = BrickAssignmentSource.Direct,
    Precedence = new BrickAssignmentPrecedence(
        BrickAssignmentSpecificity.Element,
        BrickAssignmentAuthority.Direct)
};

var targetConstructorRole = new BrickRoleAssignment
{
    Selector = BrickElementSelector.Member("TargetType..ctor"),
    RoleName = "Target",
    Source = BrickAssignmentSource.Direct,
    Precedence = new BrickAssignmentPrecedence(
        BrickAssignmentSpecificity.Element,
        BrickAssignmentAuthority.Direct)
};

new BrickPolicy
{
    Name = "Layer1.Forbidden.F-07",
    Rules =
    [
        new BrickRule
        {
            Name = "Forbidden dependency from Type to Constructor",
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
`Source` is inherited from the containing type into the source member.
`Target` is assigned to the concrete constructor.

### Explicit Example

```csharp
using NMolecules.Bricks;

[assembly: Rule(
    "F-07",
    "Billing.Domain",
    "Infrastructure",
    RuleMode.ForbidDependency,
    "Rule {rule}: {source} must not depend on {target}")]
```

The shipped explicit API binds `Infrastructure` to the containing target type.
The constructor call below is the evidence that narrows this use case to the
constructor target.

## Minimal Example

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public void Handle()
    {
        var target = new ForbiddenConstructorTarget();
    }
}

[Role("Infrastructure")]
public sealed class ForbiddenConstructorTarget
{
    // Abstract target: this constructor carries the forbidden target role.
    public ForbiddenConstructorTarget()
    {
    }
}
```

## Expected Result

A forbidden dependency violation is produced when a member inside a source type
with role `Billing.Domain` invokes a constructor that resolves to the forbidden
target role.

The important result is that `F-07` documents constructor calls as dependencies
to constructor targets, not only to the constructed type.

## What This Proves

- constructor targets can be described as specialized member targets
- type-level roles can still drive member-scope evaluation
- object creation can be documented precisely without pretending the current explicit API already exposes constructor roles



