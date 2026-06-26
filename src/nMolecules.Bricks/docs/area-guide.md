# nMolecules Bricks Developer Area Guide

`NMolecules.Bricks` intentionally exposes one public namespace. The folders below are internal source areas inside that namespace, not separate public namespaces. This keeps consumers on one import while still giving maintainers clear ownership boundaries.

Use this guide with `api-catalog.md`: the catalog lists every public type; this guide explains how the areas fit together.

## Namespace

`NMolecules.Bricks` contains the complete Bricks contract surface: typed identifiers, elements, dependencies, roles, policies, rules, reports, adoption state, governance, conformance, benchmarking, runtime evidence, visibility evidence, and AI-assistance DTOs.

Keep new public types in this namespace unless there is a package-level reason to split the API. Prefer folder structure for maintainability and stable namespace for consumer ergonomics.

## Adoption

Owns baseline and suppression exchange documents.

Use this area when violations need lifecycle state after deterministic evaluation: active, suppressed, baselined, expired suppression, or expired baseline. `BrickAdoptionDocument`, `BrickSuppression`, `BrickBaselineEntry`, and `BrickViolationStateProjector` belong here.

## Ai

Owns AI-readable comments and AI-proposed rule data.

Use this area only for advisory output that explains deterministic Bricks results. AI types must not become enforcement authority. `BrickAiViolationComment`, `BrickRemediationOption`, and `BrickRuleProposal` help tools generate review packets while `BrickRuleEvaluator` remains the source of truth.

## Attributes

Owns attribute-only configuration entry points.

Use this area when a team wants policy, role, rule, dependency, filter, or member-contract metadata declared directly in code with attributes. These types bridge developer-facing annotations into the Bricks model.

## Benchmarking

Owns performance budgets, benchmark cases, benchmark reports, and comparison reports.

Use this area when measuring or guarding hot Bricks operations such as role resolution, policy composition, rule evaluation, violation projection, runtime dependency evaluation, and report serialization.

## Configuration

Owns resolved configuration precedence.

Use this area when the same setting can come from several sources such as source annotations, policy files, analyzer config, MSBuild, packages, or generated defaults. `BrickConfigurationResolver` turns competing entries into deterministic resolved entries.

## Conformance

Owns capability and maturity-level reporting.

Use this area when a project needs to explain which Bricks capabilities are available or missing. Conformance answers readiness questions across marking, static validation, explainability, policy files, runtime-aware analysis, integration, and augmentation.

## Core

Owns shared enums that define Bricks semantics.

Use this area for vocabulary that cuts across the model: element kinds, scopes, decisions, severities, evidence levels, lifecycle states, dependency layers, and enforcement modes. Keep these types small and stable because many other areas depend on them.

## Dependencies

Owns dependency-observation coverage reports.

Use this area when showing which dependency sources are observable, partially observable, not observable, or lack enough evidence. It helps teams understand blind spots before trusting architecture diagnostics.

## Export

Owns machine-readable architecture export documents.

Use this area when Bricks data must leave the evaluator as role maps, dependency graphs, resolution traces, or validated export JSON. These documents support tooling, dashboards, audits, and downstream analysis.

## Governance

Owns governance readiness reports.

Use this area when assessing policy ownership, exception handling, role-pack evolution, compatibility expectations, and related governance requirements. It explains operational discipline around rules, not only rule execution.

## Identity

Owns typed identifiers.

Use this area for value objects that keep ids explicit: elements, dimensions, policies, dependency kinds, roles, and rules. Typed ids prevent accidental string mixing while keeping serialization simple.

## IO

Owns file-level JSON helpers.

Use this area for save/load helpers that persist Bricks reports, role maps, dependency graphs, and traces. Keep serialization format decisions in the owning report/export areas and file orchestration here.

## Members

Owns member-cardinality evaluation.

Use this area when a type-level marker requires members with specific marker attributes. It supports exactly-one, exact-count, all-members, exclusive-choice, range, and forbidden-member contracts.

## Model

Owns structural facts.

Use this area for facts gathered before evaluation: `BrickElement`, `BrickDependency`, `BrickSourceLocation`, and `BrickViolation`. These are the central data carriers that rules, reports, exports, and adoption workflows share.

## Policies

Owns policy documents, imports, aliases, composition, validation, and role assignment from policy data.

Use this area when building or loading architecture policy. `BrickPolicy` is the executable rule container, while `BrickPolicyDocument` represents exchange format and `BrickPolicyComposer` merges platform, product, team, and disabled imports.

## Profiles

Owns built-in profile definitions.

Use this area for curated policy and role-pack bundles such as layered or hexagonal profiles. Profiles are higher-level presets and should compose existing Bricks primitives rather than introduce alternate evaluation rules.

## Reflection

Owns reflection access facts.

Use this area when runtime or analyzer evidence involves reflection-based access. Reflection facts carry confidence because they are often less direct than compiler-confirmed type references.

## Reports

Owns final human and tool reports.

Use this area when converting violations and summaries into JSON or SARIF. Reports should present already-evaluated facts and avoid changing policy results.

## Roadmap

Owns staged adoption planning.

Use this area when explaining what a project has completed, what is partial, and what is still missing across Bricks roadmap stages. It turns capability gaps into staged work.

## Roles

Owns role metadata, assignments, role packs, conflict detection, and role resolution.

Use this area when converting attributes, aliases, external configuration, conventions, imports, or generated mappings into effective roles for elements. `BrickRoleResolver` decides role output before rules evaluate dependencies.

## Rules

Owns rule definitions, filters, messages, diagnostic id governance, and deterministic evaluation.

Use this area for dependency permissions and requirements. `BrickRuleEvaluator` evaluates `BrickPolicy` against dependencies and resolved roles. Keep this area deterministic and side-effect free.

## Runtime

Owns runtime activation and dependency-injection evidence.

Use this area when architecture dependencies come from composition roots, service registrations, factories, or runtime activation patterns rather than direct static type references.

## Visibility

Owns friend-assembly and visibility evidence.

Use this area when architecture boundaries are affected by assembly visibility grants. It models friend access as dependency evidence so policy can review it like other cross-boundary relationships.
