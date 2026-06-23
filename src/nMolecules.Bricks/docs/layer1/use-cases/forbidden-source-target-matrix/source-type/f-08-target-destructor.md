# F-08: Forbidden Dependency From Type To Destructor

Layer: 1 - Core  
Depends on: ../../../foundational-concept.md, ../../bricks-layer1-core-v2.md, ../../uc-l1-18-forbidden-source-target-matrix.md

## Goal

- define the forbidden source-target combination `Type -> Destructor`
- model the destructor as the real target element
- document the gap between the stronger concept and the current explicit API surface

## Scenario

The source role is assigned to a type and inherited by the member whose code
creates or otherwise reaches a finalizable target.

The forbidden target is the destructor of that finalizable target. In the
concept model this is again a specialized member-target case and is evaluated at
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

var targetDestructorRole = new BrickRoleAssignment
{
    Selector = BrickElementSelector.Member("TargetType.Finalize"),
    RoleName = "Target",
    Source = BrickAssignmentSource.Direct,
    Precedence = new BrickAssignmentPrecedence(
        BrickAssignmentSpecificity.Element,
        BrickAssignmentAuthority.Direct)
};

new BrickPolicy
{
    Name = "Layer1.Forbidden.F-08",
    Rules =
    [
        new BrickRule
        {
            Name = "Forbidden dependency from Type to Destructor",
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
`Target` is assigned to the destructor member.

### Explicit Example

```csharp
using NMolecules.Bricks;

[assembly: Rule(
    "F-08",
    "Billing.Domain",
    "Infrastructure",
    RuleMode.ForbidDependency,
    "Rule {rule}: {source} must not depend on {target}")]
```

The shipped explicit API cannot currently attach a role directly to a
destructor. The explicit example therefore binds `Infrastructure` to the
containing target type and uses finalization evidence to express the destructor
case.

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
    // Abstract target: this destructor carries the forbidden target role.
    ~ForbiddenFinalizableTarget()
    {
    }
}
```

## Expected Result

A forbidden dependency violation is produced when a member inside a source type
with role `Billing.Domain` depends on a target whose resolved destructor member
belongs to the forbidden target role.

The important result is that `F-08` stays a real destructor-target case in the
concept model instead of being collapsed into a generic type target.

## What This Proves

- destructor targets can be modeled explicitly even if they are rare
- member-scope evaluation still works when the source role originates on a type
- the documentation can distinguish clearly between the concept model and the shipped explicit API



