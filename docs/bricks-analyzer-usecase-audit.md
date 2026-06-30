# Bricks analyzer use case audit

This audit maps the documented Bricks use cases to the current Roslyn analyzer
coverage. It distinguishes compile-time analyzer coverage from runtime/model
coverage so that new analyzer work does not accidentally claim behavior that
requires role-resolution, project graph, folder or runtime evidence.

Date: 2026-06-30

## Summary

| Area | Status | Result |
| --- | --- | --- |
| Requested analyzer set | Covered | All 10 requested analyzer entry points exist and are covered by tests. |
| Compile-time metadata validity | Covered | Roles, policies, rules, dependencies, rule filters, namespace roles and member contracts are validated. |
| Static dependency evidence | Covered | Type/member/signature/body evidence is broadly covered by analyzer tests. |
| Default policy and rule filters | Covered | Default-deny behavior, conflicting defaults and invalid filter references are tested. |
| Namespace role evidence | Covered | Exact and prefix namespace role patterns are evaluated; unmatched patterns are reported. |
| Folder and project evidence | Partial | Analyzer reports invalid folder-style namespace patterns and unresolved declared dependency endpoints. Full folder/project roles require external evidence. |
| Runtime evidence | Partial | Runtime dependency declarations are checked for runtime evidence level. Live runtime discovery remains outside Roslyn. |
| Source-target shape matrix | Partial | Static type/member/body shapes are covered. Namespace, constructor and destructor as independent architectural elements are not first-class Roslyn role targets yet. |
| Role precedence and external role assignment | Runtime/model-only | Core role resolver behavior exists outside the analyzer. Roslyn currently accumulates roles and reports metadata conflicts, but does not model full precedence semantics. |

## Requested analyzers

| Analyzer | Current behavior | Coverage |
| --- | --- | --- |
| `BrickInheritanceDependencyAnalyzer` | Reports roled inheritance edges that are not covered by an explicit rule. Dependency violations themselves are still produced by `BrickDependencyRuleAnalyzer`. | `BrickRequestedAnalyzerSetTest` |
| `BrickDefaultPolicyAnalyzer` | Reports conflicting default decisions for the same policy id. | `BrickRequestedAnalyzerSetTest` |
| `BrickRuleFilterAnalyzer` | Reports rule filters with no tokens or references to unknown rules. Filter application and contradictory filters are covered elsewhere. | `BrickRequestedAnalyzerSetTest`, `BrickAnalyzerCoverageTest`, `BrickAnalyzerTest` |
| `BrickNamespaceRoleAnalyzer` | Reports namespace role patterns that do not match any namespace in the compilation. | `BrickRequestedAnalyzerSetTest` |
| `BrickProjectEvidenceAnalyzer` | Reports declared dependency endpoints that cannot be resolved to declared project types. | `BrickRequestedAnalyzerSetTest` |
| `BrickXmlDocumentationAnalyzer` | Reports public API declarations without XML documentation. | `BrickAnalyzerCoverageTest` |
| `BrickSampleConsistencyAnalyzer` | Validates analyzer sample markers in Markdown additional files. Repository Markdown samples are also compiled by tests. | `BrickRequestedAnalyzerSetTest`, `BrickSampleConsistencyAnalyzerTest` |
| `BrickPackageBoundaryAnalyzer` | Reports analyzer/runtime package reference boundaries that can be inferred from compilation references. Repository package boundaries are also tested structurally. | `BrickRequestedAnalyzerSetTest`, `BrickPackageBoundaryAnalyzerTest` |
| `BrickFolderEvidenceAnalyzer` | Reports namespace role patterns that use folder path separators. | `BrickRequestedAnalyzerSetTest` |
| `BrickRuntimeEvidenceAnalyzer` | Reports runtime dependency declarations whose evidence level is weaker than runtime-inferred evidence. | `BrickRequestedAnalyzerSetTest` |

## Layer1 core use cases

| Use case | Analyzer status | Notes |
| --- | --- | --- |
| UC-L1-01 Direct role assignment on one type | Covered | Direct `RoleAttribute` metadata is used by dependency tests. |
| UC-L1-02 Direct role assignment on two types | Covered | Source/target role combinations are tested through dependency rules. |
| UC-L1-03 Alias marker adapts an existing class | Covered | Role aliases are tested for dependency evaluation. |
| UC-L1-04 Forbidden dependency between two roles | Covered | Reports `XMoleculesBricks0001`. |
| UC-L1-05 Required dependency per type | Covered | Required dependency pass and violation cases are tested. |
| UC-L1-06 Assembly role flows into one type | Covered | Assembly and module role metadata flow into dependency analysis. |
| UC-L1-07 Stronger assignment suppresses weaker assignment | Runtime/model-only | Requires precedence-aware role resolution. The analyzer currently accumulates compile-time roles. |
| UC-L1-08 Equal-precedence exclusive assignments surface a conflict | Partial | Role combination metadata conflicts are reported; full precedence conflict resolution belongs to role resolver/model coverage. |
| UC-L1-09 External policy assigns a role to an untouchable existing type | Partial | Declared project evidence is checked, but external policy role assignment needs configuration/project evidence not present in normal C# symbols. |
| UC-L1-10 Base-type alias adapts derived types | Planned | The dependency analyzer observes base type dependencies, but role inheritance from base aliases to derived types is not implemented as analyzer role assignment. |
| UC-L1-11 Interface alias adapts implementing types | Planned | Interface dependency evidence is covered; alias-to-implementation role propagation is not yet analyzer behavior. |
| UC-L1-12 Inherited-interface alias reaches concrete implementations | Planned | Inherited interface dependencies are covered; inherited alias role propagation is not yet analyzer behavior. |
| UC-L1-13 Namespace role flows into contained types | Covered | Namespace role resolution is consumed by dependency analysis. |
| UC-L1-14 Convention assigns a role by namespace pattern | Covered | Exact and prefix namespace role patterns are covered. |
| UC-L1-15 Inference derives a role from structural context | Planned | Structural role inference is not implemented in the Roslyn analyzer. |
| UC-L1-16 Direct element role overrides namespace role | Runtime/model-only | Override semantics require precedence-aware role resolution. Analyzer role map currently accumulates roles. |
| UC-L1-17 Additive roles accumulate on one element | Partial | Analyzer accumulates global, namespace and direct roles; full assignment-source semantics are model-level. |
| UC-L1-18 Forbidden source-target matrix | Partial | Static dependency evidence is broadly covered; namespace/constructor/destructor as independent source or target elements are not fully represented. |
| UC-L1-19 Duplicate roles collapse unless parameterized | Partial | Duplicate direct role declarations are reported; parameterized role collapse is model-level. |
| UC-L1-20 Forbidden dependency except explicitly allowed target types | Partial | Name-based rule filters are covered; richer target-type exception semantics are not implemented. |
| UC-L1-21 Type usage checked across member bodies and operations | Covered | Fields, properties, methods, locals, object creation, generic arguments, attributes, extension methods and constraints are tested. |

## Forbidden source-target matrix

The matrix contains 90 concrete source-target shape files plus overview
documents. Current analyzer coverage is strongest for shapes that compile to
type-symbol, member-symbol or syntax type evidence:

| Matrix slice | Analyzer status | Notes |
| --- | --- | --- |
| Source type, derived type, interface, inherited interface | Covered for static dependencies | Base types, implemented interfaces and inherited interfaces are tested. |
| Source member/property/constructor body | Covered for type usage | Member body syntax and member signatures are analyzed as dependencies from the containing type. |
| Source destructor body | Partial | Destructor body type usage is syntax-visible, but destructor is not modeled as an independent Bricks source element. |
| Target type, derived type, interface, inherited interface | Covered for static dependencies | Type symbol expansion covers these shapes. |
| Target member/property/constructor/destructor | Partial | Type usage in member signatures/bodies is covered; member-level target identity is not modeled as a distinct Bricks element by the analyzer. |
| Source or target namespace | Partial | Namespace roles are supported, but namespace as the dependency endpoint itself is not a first-class Roslyn dependency element. |

## Gaps to close next

1. Add executable analyzer tests for UC-L1-10 to UC-L1-12 if base/interface alias propagation should become analyzer behavior.
2. Decide whether role precedence and override semantics belong in Roslyn or remain in the runtime/model role resolver.
3. Add generated or table-driven tests for the 90 source-target matrix files, at least for the shapes that are intended to be Roslyn-supported.
4. Extend samples so every `Covered` analyzer behavior has one pass and one violation snippet.
5. Keep folder/project/runtime cases represented as explicit evidence inputs; Roslyn cannot infer them reliably from C# syntax alone.
