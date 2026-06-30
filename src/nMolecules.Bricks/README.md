# nMolecules Bricks

`NMolecules.Bricks` is the reusable core package for modelling architectural
bricks, roles, dependency rules and deterministic violations.

Use this package when an application, analyzer, sample or build tool needs to:

- describe architectural elements with stable identifiers
- attach and resolve roles
- model dependencies between elements
- evaluate allow, deny and required-dependency policies
- surface deterministic violations that can be rendered by analyzers or reports

The package intentionally stays independent from operational dashboards, AI
advice and file-system workflows. Use `NMolecules.Bricks.Extensions` for richer
reporting and governance workflows, and `NMolecules.Bricks.Ai` for advisory AI
comments and rule proposal workflows.

## Project structure

The core project is organised by Bricks concepts instead of generic buckets:

- `Attributes`: CLR attributes used to declare roles, policies, rules and member contracts
- `Elements`: architectural element identifiers, metadata and source locations
- `Dependencies`: observed dependency evidence and dependency identifiers
- `Policies`: policies, policy documents, policy composition and policy defaults
- `Roles`: role definitions, role assignments, dimensions and role resolution
- `Rules`: dependency rules, rule filters, scopes and diagnostic id governance
- `Members`: member cardinality evaluation
- `Violations`: deterministic violation results, severity and violation lifecycle state

All public types still use the `NMolecules.Bricks` namespace so moving files
between folders does not change the framework API.

## Examples

The most complete executable examples live in the companion sample repository:

- `../nmolecules.brick-examples/samples/bricks/implementation-samples/function-coverage/PolicyAndResolutionExamples.cs`
- `../nmolecules.brick-examples/samples/bricks/implementation-samples/function-coverage/ViolationAndRuntimeExamples.cs`
- `../nmolecules.brick-examples/samples/bricks/violations/SelfDependencyViolationExample.cs`

The examples start with small role and policy declarations and then build up to
analyzer-facing dependency checks, runtime policy evaluation and violation
reporting.
