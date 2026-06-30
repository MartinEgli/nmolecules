# nMolecules Bricks AI

`NMolecules.Bricks.Ai` contains the AI-facing advisory layer for Bricks:

- AI-ready violation comments for deterministic Bricks violations
- remediation options and risk classification
- markdown and JSON rendering for review comments
- advisory rule proposals, proposal queues and human review workflows
- CI/MSBuild-facing AI run configuration and trust boundaries

The package references `NMolecules.Bricks` and keeps the public namespace
`NMolecules.Bricks`. AI suggestions stay advisory: deterministic Bricks rules,
violations and promoted policies remain the source of truth.
