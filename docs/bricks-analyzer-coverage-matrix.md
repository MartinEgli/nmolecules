# Bricks analyzer coverage matrix

This document turns the current analyzer coverage into an explicit contract.
It separates shipped and tested behavior from planned coverage and from cases
that belong to runtime evidence instead of the Roslyn analyzer.

## Status legend

| Status | Meaning |
| --- | --- |
| Covered | Implemented and covered by analyzer tests. |
| Partial | Implemented for a useful subset, but the full matrix is not proven yet. |
| Planned | Should become analyzer behavior and needs tests plus samples. |
| Runtime-only | Not a Roslyn analyzer responsibility; belongs to runtime or evidence packages. |
| Unsupported | Deliberately ignored or not meaningful for the current analyzer model. |

## Analyzer scope

The analyzer package covers compile-time C# evidence:

- Bricks metadata declared through attributes.
- Dependency policies, rules and rule filters declared on assemblies, modules,
  or dedicated carrier types.
- Static type dependencies visible in signatures and member bodies.
- Declared dependency facts from `DependencyAttribute`.
- Member contract cardinality on classes and structs.
- Deterministic diagnostics for rule violations and invalid configuration.

The analyzer does not execute code, inspect DI containers at runtime, evaluate
reflection calls semantically, or read external project/folder graphs unless
those concepts are represented as compile-time metadata.

## Current diagnostics

| Diagnostic | Coverage status | Notes |
| --- | --- | --- |
| `XMoleculesBricks0001` | Covered | Denied dependencies, missing required dependencies and same source/target dependencies are tested. |
| `XMoleculesBricks0002` | Covered | Invalid, incomplete and duplicate metadata is tested for roles, namespace roles, policies, rules, dependencies and member contracts. |
| `XMoleculesBricks0003` | Covered | Exactly-one member contract. |
| `XMoleculesBricks0004` | Covered | All-members member contract. |
| `XMoleculesBricks0005` | Covered | Exact member-count contract. |
| `XMoleculesBricks0006` | Covered | Exclusive-choice member contract. |
| `XMoleculesBricks0007` | Covered | Inclusive member-count range contract. |
| `XMoleculesBricks0008` | Covered | Forbidden-member contract. |
| `XMoleculesBricks0009` | Covered | Unique named-member contract. |
| `XMoleculesBricks0010` | Covered | Required named-member contract. |
| `XMoleculesBricks0011` | Covered | Missing XML documentation on public framework API declarations. |

## Role placement coverage

| Role placement | Status | Current behavior | Remaining work |
| --- | --- | --- | --- |
| Direct role attribute on a type | Covered | Used by dependency and member-contract tests. | Keep pass and violation samples aligned. |
| Custom role attribute deriving from `RoleAttribute` | Covered | Tested by alias and custom contract scenarios. | Add one didactic sample that shows the reusable custom attribute pattern. |
| Role alias on target/source type | Covered | Tested by role alias dependency scenarios. | Add explicit pass and violation sample pair. |
| Assembly-level role metadata | Covered | Assembly role metadata flows into dependency evaluation and is tested. | Add didactic sample that explains the broad scope. |
| Module-level role metadata | Covered | Module role metadata flows into dependency evaluation and is tested. | Add didactic sample that explains the broad scope. |
| Namespace role placement | Covered | `NamespaceRoleAttribute` assigns roles to exact namespace names and prefix patterns, and analyzer tests cover both. | Add didactic pass and violation samples. |
| Folder role placement | Runtime-only | Roslyn symbols do not carry folder semantics as architecture metadata. | Implement through project/evidence tooling, not the analyzer alone. |
| Project role placement | Runtime-only | Requires MSBuild/project graph evidence. | Implement through build/evidence layer and feed deterministic rules. |
| Interface role placement | Covered | Implemented interface and inherited interface dependencies are tested. | Add didactic pass and violation samples. |
| Abstract class role placement | Covered | Base class dependencies are tested and abstract classes use the same symbol path. | Add didactic pass and violation samples for abstract bases. |

## Dependency evidence coverage

| Evidence shape | Status | Notes |
| --- | --- | --- |
| Field type | Covered | Included in `DependencyEvidenceCases`. |
| Property type | Covered | Included in `DependencyEvidenceCases`. |
| Method return type | Covered | Included in `DependencyEvidenceCases`. |
| Method parameter type | Covered | Included in `DependencyEvidenceCases`. |
| Local variable declaration | Covered | Included in `DependencyEvidenceCases`. |
| Explicit object creation | Covered | Included in `DependencyEvidenceCases`. |
| Implicit object creation | Covered | Included in `DependencyEvidenceCases`. |
| Generic type argument | Covered | Included in `DependencyEvidenceCases`. |
| Array element type | Covered | Included in `DependencyEvidenceCases`. |
| Nullable value type normalization | Covered | Dedicated coverage test exists. |
| Pointer type expansion | Covered | Dedicated coverage test exists. |
| Declared dependency attribute | Covered | Included in `DependencyEvidenceCases` and self-dependency tests. |
| Attribute argument type usage | Covered | `typeof(...)` arguments inside attributes are tested as compile-time dependency evidence. |
| Generic constraints | Covered | Type-level and method-level generic constraints are tested. |
| Base type and implemented interface declarations | Covered | Base class, implemented interface and inherited interface dependencies are tested. |
| Extension method receiver and invocation target | Covered | Extension method invocations are tested through the containing extension type. |
| Reflection string/type lookup | Runtime-only | Analyzer can catch simple `typeof(T)` patterns, but semantic reflection evidence belongs to runtime/evidence tooling. |
| DI registration | Runtime-only | Requires composition-root or runtime registration evidence. |
| Factory registration | Runtime-only | Requires semantic or runtime evidence beyond static type occurrence. |

## Rule decision matrix

| Decision or policy behavior | Status | Current behavior | Remaining work |
| --- | --- | --- | --- |
| Deny rule violation | Covered | Forbidden dependency scenarios report `XMoleculesBricks0001`. | Keep matrix synced with dependency evidence shapes. |
| Allow rule pass | Covered | `RuleMode.AllowDependency` is tested with an active default-deny policy. | Expand didactic samples. |
| Required dependency pass | Covered | Required dependency evidence cases accept supported evidence shapes. | Keep aligned with role placement expansion. |
| Required dependency violation | Covered | Missing required dependency reports `XMoleculesBricks0001`. | Add namespace/interface/base variants. |
| Default allow | Covered | Explicit default-allow policy test accepts uncovered dependencies. | Expand didactic samples. |
| Default deny | Covered | Active default-deny policy reports uncovered dependencies and accepts matching allow rules. | Expand didactic samples. |
| Same source and target | Covered | Static and declared self-dependencies report `XMoleculesBricks0001`. | Add sample pair showing why self-dependencies are invalid. |
| Rule priority conflicts | Partial | Metadata conflicts are tested; all evaluation priority variants are not proven in analyzer matrix. | Add deny/allow priority tests. |
| Rule filters | Covered | Required/excluded source and target name filters are applied during dependency evaluation and contradictory filters are detected as configuration issues. | Expand didactic samples. |

## Rule placement coverage

| Rule placement | Status | Current behavior | Remaining work |
| --- | --- | --- | --- |
| Assembly-level rule metadata | Covered | Assembly `RuleAttribute` declarations drive forbidden, required and allow-rule dependency evaluation. | Keep examples aligned with the basic Bricks sample. |
| Module-level rule metadata | Covered | Module `RuleAttribute` declarations drive dependency evaluation and are covered by analyzer tests. | Add didactic sample when module-wide rules become part of sample path. |
| Type-level rule metadata | Covered | Rule carrier types can host `RuleAttribute` and matching `RuleFilterAttribute` declarations; both are evaluated by the dependency analyzer. | Add a project-specific rule-catalog sample. |

## Policy placement coverage

| Policy placement | Status | Current behavior | Remaining work |
| --- | --- | --- | --- |
| Assembly-level policy metadata | Covered | Assembly `PolicyAttribute` declarations can activate default-deny dependency evaluation. | Keep default-deny samples aligned. |
| Module-level policy metadata | Covered | Module `PolicyAttribute` declarations can activate default-deny dependency evaluation. | Add didactic sample if module-wide policies are documented. |
| Type-level policy metadata | Covered | Policy carrier types can host `PolicyAttribute` declarations that affect dependency evaluation. | Add a project-specific policy-catalog sample. |

## Member contract coverage

| Contract area | Status | Notes |
| --- | --- | --- |
| Direct contract attributes | Covered | Covered for pass and violation cases. |
| Custom contract attributes | Covered | Covered for pass and violation cases. |
| Classes | Covered | Member contracts run on class carriers. |
| Structs | Covered | Member contracts run on struct carriers. |
| Fields | Covered | Marked member kind test. |
| Properties | Covered | Marked member kind test. |
| Methods | Covered | Marked member kind test. |
| Events | Covered | Marked member kind test. |
| Constructors | Unsupported | Explicitly ignored by current coverage. |
| Records | Covered | Member contracts are registered for records and covered by tests. |
| Interfaces | Covered | Member contracts are registered for interfaces and covered by tests. |
| Abstract classes | Covered | Member contracts on abstract classes are covered by tests. |

## XML documentation coverage

| Public API declaration | Status | Notes |
| --- | --- | --- |
| Public types | Covered | Classes, structs, records, interfaces and enums are analyzed. |
| Constructors and methods | Covered | Public constructors and methods require XML documentation. |
| Properties and indexers | Covered | Public properties and indexers require XML documentation. |
| Fields and events | Covered | Public fields and events require XML documentation. |
| Delegates and enum members | Covered | Public delegates and enum members require XML documentation. |
| Non-public declarations | Covered | Internal/private API and public members of non-public containing types are ignored. |

## Sample coverage work

The analyzer tests are broader than the didactic samples. Samples should be
expanded as a learning path:

| Sample area | Status | Needed examples |
| --- | --- | --- |
| Minimal role and policy pass | Partial | One tiny example that produces no diagnostics. |
| Deny violation | Covered | Keep one simple violation and one multi-role violation. |
| Required dependency pass and violation | Partial | Add side-by-side pass and violation samples. |
| Same source and target violation | Covered | Add a didactic explanation sample if not already linked from docs. |
| Interface and abstract class roles | Planned | Add pass/violation pairs after analyzer matrix tests exist. |
| Namespace/project/folder roles | Planned | Document as planned or runtime-only until representation is implemented. |
| Member contracts | Partial | Add examples from simple exact-one to named-member contracts. |
| IDE setup | Covered | Visual Studio/VS Code docs exist, but should link to pass/violation samples. |

## Sample consistency coverage

| Sample source | Status | Notes |
| --- | --- | --- |
| Markdown analyzer samples | Covered | Markdown code fences marked with `analyzer-pass` or `analyzer-violation <diagnostic-id>` are compiled and checked by `BrickSampleConsistencyAnalyzerTest`. |
| DDD Bricks sample | Covered | `docs/bricks-ddd-sample.md` contains pass and violation samples checked against the Bricks analyzers. |
| External sample projects | Planned | Dedicated sample projects should be added to the same consistency harness once they are part of this repository or referenced deterministically. |

## Next implementation tasks

1. Keep folder and project roles outside the Roslyn analyzer unless they are
   supplied as compile-time metadata by an evidence provider.
2. Add explicit samples for namespace roles, rule filters, default-deny allow rules, attribute type arguments, extension methods and inherited/interface dependencies.
3. Add a small coverage guard that fails when a documented `Planned` row is
   promoted without a matching test or sample reference.
