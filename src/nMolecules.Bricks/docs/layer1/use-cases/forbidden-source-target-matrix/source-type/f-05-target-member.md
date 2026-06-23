# F-05: Forbidden Dependency From Type To Member

Layer: 1 - Core  
Depends on: ../../../foundational-concept.md, ../../bricks-layer1-core-v2.md, ../../uc-l1-18-forbidden-source-target-matrix.md

## Goal

- define the forbidden source-target combination `Type -> Member`
- model the target as a real member-level target, not only as a containing type
- keep the current explicit API example visible next to the stronger abstract concept

## Scenario

The source role originates on a type, but the concrete forbidden dependency is
emitted from a member inside that type.

The target is an addressable member with its own resolved role. In the abstract
concept this is therefore a member-target case evaluated at `BrickScope.Member`.
The source side still carries the label `Type` because the source role is
assigned to the containing type and flows into the source member.

## Policy

### Status Note

Conceptually, this policy is correct: `F-05` is a real `Type -> Member` case.

In the current shipped API surface, however, the target member is not yet
directly role-addressable through `RoleAttribute`, because `RoleAttribute`
currently applies to classes, interfaces, and structs, not to members.

That means this file intentionally shows two layers:

- the abstract concept, where the target member is explicitly role-assigned
- the explicit current example, where the containing target type carries the
  role and the member access acts as the concrete evidence

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

var targetMemberRole = new BrickRoleAssignment
{
    Selector = BrickElementSelector.Member("TargetType.TargetMember"),
    RoleName = "Target",
    Source = BrickAssignmentSource.Direct,
    Precedence = new BrickAssignmentPrecedence(
        BrickAssignmentSpecificity.Element,
        BrickAssignmentAuthority.Direct)
};

new BrickPolicy
{
    Name = "Layer1.Forbidden.F-05",
    Rules =
    [
        new BrickRule
        {
            Name = "Forbidden dependency from Type to Member",
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
`Source` is assigned to the containing type and inherited by the source member.
`Target` is assigned to the addressed target member itself.

### Explicit Example

```csharp
using NMolecules.Bricks;

[assembly: Rule(
    "F-05",
    "Billing.Domain",
    "Infrastructure",
    RuleMode.ForbidDependency,
    "Rule {rule}: {source} must not depend on {target}")]
```

The shipped explicit API does not currently assign roles directly to members.
This concrete example therefore binds `Infrastructure` to the containing target
type, while the member access shown below provides the member-target evidence.

## Minimal Example

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public void Handle(ForbiddenMemberTarget target)
    {
        target.ForbiddenOperation();
    }
}

[Role("Infrastructure")]
public sealed class ForbiddenMemberTarget
{
    // Abstract target: this member carries the forbidden target role.
    public void ForbiddenOperation()
    {
    }
}
```

## Combined Contrast Example

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public void Handle(MixedMemberTarget target)
    {
        target.AllowedOperation();
        target.ForbiddenOperation();
    }
}

public sealed class MixedMemberTarget
{
    // Abstract concept: this member resolves to an allowed role such as `Shared`.
    public void AllowedOperation()
    {
    }

    // Abstract concept: this member resolves to the forbidden target role.
    public void ForbiddenOperation()
    {
    }
}
```

In the stronger concept model, both members may live on the same class while
resolving to different target roles. That is the exact contrast this example is
meant to show.

The current explicit API cannot yet attach different roles to individual members
of the same type. In today's shipped surface this combined contrast therefore
remains a conceptual example, while the concrete explicit example above still
approximates the forbidden side through the containing target type.

## Expected Result

A forbidden dependency violation is produced when a member inside a source type
with role `Billing.Domain` depends on a target member that resolves to the
forbidden target role.

The important result is that `F-05` is treated as a true member-target case in
the concept model, even though today's explicit example approximates that target
through the containing type.

Within the combined contrast example, `AllowedOperation()` would be allowed,
while `ForbiddenOperation()` would produce the violation. The difference comes
from the resolved target member role, not from the containing class alone.

## What This Proves

- the source role can originate on a type and still be evaluated at member scope
- the target of the violation can be modeled as a concrete member
- the same source member can distinguish between forbidden and allowed target members on one target class
- the current explicit API can be documented honestly as an approximation of the stronger concept



