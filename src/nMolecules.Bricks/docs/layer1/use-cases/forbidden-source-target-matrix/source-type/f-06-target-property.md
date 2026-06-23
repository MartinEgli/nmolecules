# F-06: Forbidden Dependency From Type To Property

Layer: 1 - Core  
Depends on: ../../../foundational-concept.md, ../../bricks-layer1-core-v2.md, ../../uc-l1-18-forbidden-source-target-matrix.md

## Goal

- define the forbidden source-target combination `Type -> Property`
- treat the property as the actual target element
- keep the stronger abstract concept separate from the current explicit API approximation

## Scenario

The source role is assigned to a type and inherited by the member that performs
the property access.

The forbidden target is the addressed property itself. In the concept model this
is therefore a specialized member-target case and is evaluated at
`BrickScope.Member`.

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

var targetPropertyRole = new BrickRoleAssignment
{
    Selector = BrickElementSelector.Member("TargetType.Value"),
    RoleName = "Target",
    Source = BrickAssignmentSource.Direct,
    Precedence = new BrickAssignmentPrecedence(
        BrickAssignmentSpecificity.Element,
        BrickAssignmentAuthority.Direct)
};

new BrickPolicy
{
    Name = "Layer1.Forbidden.F-06",
    Rules =
    [
        new BrickRule
        {
            Name = "Forbidden dependency from Type to Property",
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
`Source` is inherited from the containing type into the accessing member.
`Target` is assigned to the addressed property.

### Explicit Example

```csharp
using NMolecules.Bricks;

[assembly: Rule(
    "F-06",
    "Billing.Domain",
    "Infrastructure",
    RuleMode.ForbidDependency,
    "Rule {rule}: {source} must not depend on {target}")]
```

The shipped explicit API still binds `Infrastructure` to the containing target
type. The property access shown below provides the specialized property-target
evidence for this use case.

## Minimal Example

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public string Handle(ForbiddenPropertyTarget target)
    {
        return target.Value;
    }
}

[Role("Infrastructure")]
public sealed class ForbiddenPropertyTarget
{
    // Abstract target: this property carries the forbidden target role.
    public string Value { get; } = "forbidden";
}
```

## Expected Result

A forbidden dependency violation is produced when a member inside a source type
with role `Billing.Domain` reads or writes a property that resolves to the
forbidden target role.

The important result is that `F-06` documents a property target explicitly,
instead of collapsing it back into a generic type target.

## What This Proves

- property targets can be represented as first-class member targets
- type-level roles can flow into the member that performs the property access
- the current explicit API can be described as a deliberate approximation, not as the concept itself



