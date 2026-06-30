# Bricks public API documentation standard

Bricks is treated as a framework surface. Every public type should be reusable
from application code, analyzers, samples and build tooling without requiring a
developer to read the implementation first.

## Required public documentation

Every public class, record, struct, enum, interface, constructor, property and
method should have XML documentation that answers these questions:

- What concept does this API represent?
- When should a framework consumer use it?
- Which invariant, null-handling or immutability rule matters?
- Which related API should be used next?
- Which executable sample shows the API in context?

Use `<summary>` for the short contract, `<remarks>` for use cases and
boundaries, `<param>` and `<exception>` for constructor semantics, and
`<seealso>` for related Bricks concepts.

## Example links

XML documentation should point developers to executable examples where the type
is used in a realistic flow. Prefer these sample families:

- Core policy and role resolution:
  `../nmolecules.brick-examples/samples/bricks/implementation-samples/function-coverage/PolicyAndResolutionExamples.cs`
- Runtime evaluation and violation reporting:
  `../nmolecules.brick-examples/samples/bricks/implementation-samples/function-coverage/ViolationAndRuntimeExamples.cs`
- Self-dependency violations:
  `../nmolecules.brick-examples/samples/bricks/violations/SelfDependencyViolationExample.cs`
- Analyzer diagnostics:
  `../nmolecules.brick-examples/samples/bricks/analyzer/`

## Reusability rules

Public Bricks APIs should keep these framework rules:

- Constructors validate required collaborators and tolerate optional text by
  normalising it to an empty string only when that is part of the contract.
- Collections exposed from public objects are immutable snapshots or read-only
  views.
- Policy evaluation is deterministic and does not call AI, mutate policies or
  create suppressions.
- Analyzer-facing types keep stable identifiers so documentation, diagnostics
  and examples can link to the same concept.

## Iteration workflow

For each documentation roundtrip:

1. Pick a small public API group, for example core model, attributes, policies
   or analyzer diagnostics.
2. Add XML documentation and example links.
3. Build the package with XML documentation generation enabled.
4. Run the matching unit or analyzer tests.
5. Extend samples when a documented use case is not executable yet.
6. Document remaining gaps before moving to the next group.
