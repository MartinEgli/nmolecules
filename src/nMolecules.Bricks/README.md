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

## Examples

The most complete executable examples live in the companion sample repository:

- `../nmolecules.brick-examples/samples/bricks/implementation-samples/function-coverage/PolicyAndResolutionExamples.cs`
- `../nmolecules.brick-examples/samples/bricks/implementation-samples/function-coverage/ViolationAndRuntimeExamples.cs`
- `../nmolecules.brick-examples/samples/bricks/violations/SelfDependencyViolationExample.cs`

The examples start with small role and policy declarations and then build up to
analyzer-facing dependency checks, runtime policy evaluation and violation
reporting.
