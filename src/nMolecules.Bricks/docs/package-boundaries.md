# nMolecules Bricks Package Boundaries

Status: accepted for the Bricks concept v3 roundtrip on 2026-06-27.

This document records the package-boundary decision for the public Bricks
surface. It answers which areas belong to the deterministic core, which areas
are optional tooling surfaces, and which areas are candidates for a later
physical package split.

## Decision

The current runtime package remains `NMolecules.Bricks` and keeps one public
namespace, `NMolecules.Bricks`, for source compatibility. Public types are not
moved in the current roundtrip.

`NMolecules.Bricks.Analyzers` stays the only physically separate Bricks package
today. It consumes the stable Bricks contract and emits diagnostics, but the
runtime package owns the typed model, policies, rules, role resolution, reports,
examples and documentation contracts.

Future package splits must be additive first. A split must keep the namespace
stable, provide a deprecation or forwarding path, keep the roundtrip green, and
preserve 100% line and branch coverage for the analyzer and runtime surfaces.

## Logical Packages

| Logical package | Source areas | Boundary |
| --- | --- | --- |
| `NMolecules.Bricks` core contract | `Attributes`, `Dependencies`, `Elements`, `Members`, `Policies`, `Roles`, `Rules`, `Violations` | Deterministic source of truth for annotations, elements, dependency facts, role resolution, policies, member contracts, rule evaluation and violation results. Keep this together because analyzers, samples and runtime evaluators depend on the same vocabulary. |
| `NMolecules.Bricks.Evidence` candidate | `Reflection`, `Runtime`, `Visibility` | Optional evidence and observability surface for reflection, dependency injection, runtime activation and friend-assembly access. These areas feed deterministic rules but are not the minimal static rule engine. |
| `NMolecules.Bricks.Reporting` candidate | `Adoption`, `Export`, `IO`, `Reports` | Exchange, projection, persistence and final reporting surface. Consumers that only need rule evaluation should not have to depend on every report writer once a physical split is introduced. |
| `NMolecules.Bricks.Planning` candidate | `Benchmarking`, `Conformance`, `Governance`, `Roadmap` | Readiness, maturity, benchmark, governance and staged adoption planning. These areas explain and plan adoption; they do not decide analyzer diagnostics. |
| `NMolecules.Bricks.Ai` candidate | `Ai` | Advisory AI-assisted explanation and proposal workflow. It must never become enforcement authority; deterministic Bricks rules remain the source of truth. |
| `NMolecules.Bricks.Analyzers` | analyzer package outside `src/nMolecules.Bricks` | Roslyn diagnostics and packaged analyzer delivery. It should depend on deterministic contracts and not on planning or AI-only surfaces. |

## Roadmap Placement

`Roadmap` classes are useful for adoption planning, but they are not required to
execute deterministic analysis. They belong with `Governance`, `Conformance`
and `Benchmarking` in the future `NMolecules.Bricks.Planning` candidate.

Keep the current public types for compatibility. If a physical split is later
introduced, move Roadmap through an additive package release first and only
remove or obsolete the old location after a documented migration window.

## Other Decomposition Findings

`Ai` is the clearest optional split after Planning because it is advisory,
workflow-oriented and intentionally separated from enforcement.

`Adoption`, `Export`, `IO` and `Reports` form a reporting/exchange boundary.
They should split only after report schemas and file helpers have stable
versioning guarantees.

`Dependencies`, `Reflection`, `Runtime` and `Visibility` form an evidence
boundary. They should split only when the analyzer/runtime integration can still
produce one coherent `BrickViolation` model without adapter friction.

`Profiles` may later become `NMolecules.Bricks.Profiles` if built-in clean-code,
layered, hexagonal or other presets grow independently. For now, profiles stay
with the core contract because they compose existing rules and policies rather
than introduce alternate evaluation semantics.

`Attributes`, `Dependencies`, `Elements`, `Members`, `Policies`, `Roles`,
`Rules` and `Violations` should not be split in the current concept v3 line.
Splitting these would fragment the analyzer contract and make rule, sample and
documentation coverage harder to keep exact.

## Split Guardrails

1. Add the new package before moving consumers.
2. Keep public namespaces source-compatible unless a major-version migration is
   deliberately planned.
3. Keep analyzer diagnostics deterministic and independent from AI or planning
   packages.
4. Keep sample evidence for pass and violation behavior in the roundtrip.
5. Keep the API catalog, enum behavior guide, area guide and this package guide
   synchronized by tests.
6. Run `tools\validate-bricks-roundtrip.ps1` and
   `tools\validate-bricks-coverage.ps1` before accepting the split.
