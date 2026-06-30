# nMolecules Bricks Extensions

`NMolecules.Bricks.Extensions` contains the operational Bricks model that builds
on the core package:

- adoption documents, baselines and suppressions
- benchmarking, configuration and dependency coverage reports
- conformance, governance and roadmap reporting
- JSON file helpers, export documents, runtime wiring, reflection and visibility

Use `NMolecules.Bricks` for the core modelling surface: attributes, roles,
policies, rules, member contracts, identifiers, source locations, dependencies,
violations and the basic element model.

Use `NMolecules.Bricks.Extensions` when samples, analyzers, CI tooling or
architecture dashboards need reporting, operational workflows or richer export
types. Use `NMolecules.Bricks.Ai` for AI governance, remediation comments and
advisory rule proposal workflows. Both extension packages reference the core
package and keep the same `NMolecules.Bricks` namespace so existing code can
opt into the additional project reference without changing public type names.
