# nMolecules.Bricks

Status: March 7, 2026

`NMolecules.Bricks` is the generic customization layer for domain-specific attribute models.

This document describes the currently shipped Bricks surface.
For the broader target model and future architecture, use
`src/nMolecules.Bricks/docs/foundational-concept.md` as the authoritative
concept reference.

It enables:

- custom role attributes via role aliases
- generic dependency rules between roles
- custom rule messages
- rule conditions and exclusion filters configured through dedicated filter attributes
- member-cardinality contracts for custom marker ecosystems

## Core Concepts

- `[Role("RoleName")]`
  - assigns a target type directly to a generic role
  - use this when a generic marker is sufficient and you do not need a custom attribute name

- `[RoleAlias("RoleName")]`
  - maps a custom attribute type to a generic brick role
  - use this on a custom marker attribute when you want a clearer domain-specific name such as `[BillingDomainRole]`

- `[Rule(...)]`
  - defines generic rule metadata between two roles
  - supports `ForbidDependency` and `RequireDependency`
  - supports a custom message template directly
  - is combined with dedicated rule-filter attributes when optional filters are needed

- `[RequireExactlyOneMember(typeof(...))]`
  - declares that a type marked with a custom attribute must expose exactly one member with the configured marker attribute

- `[RequireAllMembers(typeof(X), typeof(Y), ...)]`
  - declares that a type marked with a custom attribute must expose all configured member-marker types at least once

- `[RequireMemberCount(typeof(...), n)]`
  - declares that a type marked with a custom attribute must expose exactly `n` members with the configured marker attribute

- `[RequireExclusiveChoice(typeof(A), typeof(B))]`
  - declares that a type marked with a custom attribute must expose exactly one side of a two-marker XOR contract

- `RoleId`
  - typed wrapper for role identifiers in regular code
  - useful for catalogs, comparisons, helper APIs, and reflection results
  - cannot be used directly as an attribute argument because CLR attributes only allow a restricted set of parameter types

- `RuleFilter`
  - abstract base type for specialized optional `Rule` filters
  - `RuleAttribute` can accept them through protected `params RuleFilter[]` constructors
  - useful in runtime code, tests, and custom specialization APIs

- `RuleFilterAttribute`
  - abstract base type for analyzer-visible optional rule filters
  - binds a filter to a concrete `Rule` through the rule id
  - public attribute syntax uses these attributes because CLR attributes cannot store arbitrary custom objects

- specialized `RuleFilter` types
  - `ExcludedSourceNameContainsRuleFilter`
  - `ExcludedTargetNameContainsRuleFilter`
  - `ExcludedMemberNameContainsRuleFilter`
  - `RequiredSourceNameContainsRuleFilter`
  - `RequiredTargetNameContainsRuleFilter`

- `RuleMessage`
  - typed wrapper for the optional `Rule` message template
  - exposes the supported placeholders `{rule}`, `{source}`, `{target}`, `{member}`
  - useful in regular code and derived `RuleAttribute` specializations

- `RuleMessageBuilder`
  - fluent builder for assembling a `RuleMessage`
  - useful when you want to compose a readable template without embedding raw placeholder strings inline

## Direct Roles vs. Alias Markers

These two patterns solve different problems:

- direct role assignment
  - apply `[Role("Billing.Domain")]` directly to the target type
  - simplest option when the generic role name is acceptable in source code

- alias-style custom marker
  - define a custom attribute that derives from `Attribute`
  - annotate that custom attribute with `[RoleAlias("Billing.Domain")]`
  - apply the custom attribute to the target type
  - best option when you want expressive, domain-specific markers in source code

Example:

```csharp
[Role("Billing.Domain")]
public sealed class ContractPolicy
{
}

[RoleAlias("Billing.Domain")]
public sealed class BillingDomainRoleAttribute : Attribute
{
}

[BillingDomainRole]
public sealed class ContractAggregate
{
}
```

Do not mix both styles in the same custom marker unless you intentionally need both metadata forms. In normal usage, a custom marker should usually be either:

- a direct `RoleAttribute` usage on the target type, or
- an alias marker declared with `RoleAliasAttribute`

## Specialization Guidance

The attributes are designed to stay backward compatible and directly usable (`[Role("...")]`, `[Rule(...)]`).
For clearer domain naming, use specialization:

- preferred for custom role names: define your own marker attribute and map it via `[RoleAlias("...")]`
- optional: derive from `RoleAttribute` / `RuleAttribute` when you want typed wrappers

Properties on brick attributes are virtual and base classes expose protected constructors so derived wrappers can customize behavior without replacing the base model.
For maintainability, keep role names and rule IDs in dedicated constants classes (for example `BillingRoles`, `BillingRules`) and reuse those constants from both attributes and rule declarations.
When you want stronger typing in regular code, expose both a `const string` for attribute usage and a `RoleId` property for typed access.
For analyzer-enforced rules, prefer explicit `[Rule(...)]` declarations (you can still use constants classes) so rule metadata is unambiguous at compile time.
When optional filters are needed in attribute syntax, place matching `RuleFilterAttribute` declarations next to the `Rule` and bind them through the same rule id.


## Rule Parameters

Public `Rule` constructor parameters:

- `id`
- `sourceRole`
- `targetRole`
- `mode`
- `message`

Optional rule filters that the analyzer should enforce are declared as separate filter attributes next to the corresponding `Rule` declaration and linked through the same `id`.
For runtime code and helper APIs, those filter attributes map back to specialized `RuleFilter` types.
When you want to handle the optional message template as one concept in regular code, use `RuleMessage` and `RuleMessageBuilder`.
The public attribute constructor no longer accepts the five filter string parameters directly.

## Analyzer Behavior

The Roslyn analyzer reads `Rule` metadata and matching `RuleFilterAttribute` declarations at compilation time.
Rule changes in code become active immediately on the next analysis pass (no IDE restart required).

## Example

```csharp
using System;
using NMolecules.Bricks;

public static class BillingRoles
{
    public const string Domain = "Billing.Domain";
    public const string Infrastructure = "Billing.Infrastructure";

    public static RoleId DomainId => RoleId.From(Domain);
    public static RoleId InfrastructureId => RoleId.From(Infrastructure);
}

[assembly: Rule(
    "BILL-ARCH-001",
    BillingRoles.Domain,
    BillingRoles.Infrastructure,
    RuleMode.ForbidDependency,
    "Rule {rule}: {source} must not depend on {target} via {member}")]

[assembly: Rule(
    "BILL-ARCH-002",
    BillingRoles.Domain,
    BillingRoles.Infrastructure)]

[assembly: ExcludedMemberNameContains("BILL-ARCH-002", "Allowed")]

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
[RoleAlias("Billing.Domain")]
public sealed class BillingDomainAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
[RoleAlias("Billing.Infrastructure")]
public sealed class BillingInfrastructureAttribute : Attribute
{
}
```

Supported filter attributes:

- `ExcludedSourceNameContainsAttribute`
- `ExcludedTargetNameContainsAttribute`
- `ExcludedMemberNameContainsAttribute`
- `RequiredSourceNameContainsAttribute`
- `RequiredTargetNameContainsAttribute`
