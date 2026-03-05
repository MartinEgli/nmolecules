# nMolecules.Bricks

Status: March 5, 2026

`NMolecules.Bricks` is the generic customization layer for domain-specific attribute models.

It enables:

- custom role attributes via role aliases
- generic dependency rules between roles
- custom rule messages
- rule conditions and exclusion filters configured through constructor parameters

## Core Concepts

- `[Role("RoleName")]`
  - assigns a type to a generic role

- `[RoleAlias("RoleName")]`
  - maps a custom attribute type to a generic brick role

- `[Rule(...)]`
  - defines a runtime-evaluated generic rule
  - supports `ForbidDependency` and `RequireDependency`
  - supports custom message template and exclusion/condition filters

## Specialization Guidance

The attributes are designed to stay backward compatible and directly usable (`[Role("...")]`, `[Rule(...)]`).
For clearer domain naming, use specialization:

- preferred for custom role names: define your own marker attribute and map it via `[RoleAlias("...")]`
- optional: derive from `RoleAttribute` / `RuleAttribute` when you want typed wrappers

Properties on brick attributes are virtual and base classes expose protected constructors so derived wrappers can customize behavior without replacing the base model.
For maintainability, keep role names and rule IDs in dedicated constants classes (for example `BillingRoles`, `BillingRules`) and reuse those constants from both attributes and rule declarations.
For analyzer-enforced rules, prefer explicit `[Rule(...)]` declarations (you can still use constants classes) so rule metadata is unambiguous at compile time.


## Rule Parameters

`Rule` constructor parameters:

- `id`
- `sourceRole`
- `targetRole`
- `mode`
- `message`
- `excludedSourceNameContains`
- `excludedTargetNameContains`
- `excludedMemberNameContains`
- `requiredSourceNameContains`
- `requiredTargetNameContains`

Filter parameters use `|` as token separator.

## Analyzer Behavior

The Roslyn analyzer reads `Rule` metadata at compilation time.
Rule changes in code become active immediately on the next analysis pass (no IDE restart required).

## Example

```csharp
using System;
using NMolecules.Bricks;

[assembly: Rule(
    "BILL-ARCH-001",
    "Billing.Domain",
    "Billing.Infrastructure",
    RuleMode.ForbidDependency,
    "Rule {rule}: {source} must not depend on {target} via {member}",
    excludedMemberNameContains: "Allowed")]

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
[RoleAlias("Billing.Domain")]
public sealed class BillingDomainAttribute : RoleAttribute
{
    public BillingDomainAttribute() : base("Billing.Domain") {}
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
[RoleAlias("Billing.Infrastructure")]
public sealed class BillingInfrastructureAttribute : RoleAttribute
{
    public BillingInfrastructureAttribute() : base("Billing.Infrastructure") {}
}
```
