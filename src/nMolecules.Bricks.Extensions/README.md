# nMolecules Bricks Extensions

`NMolecules.Bricks.Extensions` is the compatibility meta package for the
operational Bricks model that builds on the core package. New consumers can
reference smaller packages directly:

- `NMolecules.Bricks.Extensions.Core` for configuration and built-in profiles.
- `NMolecules.Bricks.Extensions.Runtime` for runtime wiring, reflection and
  visibility evidence.
- `NMolecules.Bricks.Extensions.Assessment` for benchmarking, conformance,
  dependency coverage, governance and roadmap reporting.
- `NMolecules.Bricks.Extensions.Reporting` for adoption documents, export
  documents, JSON file helpers and final report formats.

Use `NMolecules.Bricks` for the core modelling surface: attributes, roles,
policies, rules, member contracts, identifiers, source locations, dependencies,
violations and the basic element model.

Use the meta package when samples, analyzers, CI tooling or architecture
dashboards need the whole extension surface. Use the granular packages when a
consumer only needs one operational area. Use `NMolecules.Bricks.Ai` for AI
governance, remediation comments and advisory rule proposal workflows. All
extension packages reference the core package and keep the same
`NMolecules.Bricks` namespace so existing code can opt into additional project
references without changing public type names.
