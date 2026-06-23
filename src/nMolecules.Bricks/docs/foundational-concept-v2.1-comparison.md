# Bricks Foundational Concept v2.1 Comparison

Status: June 23, 2026

Compared documents:

- Current repository baseline: `foundational-concept.md`
- Stored candidate concept: `foundational-concept-v2.1.md`

## Summary

The v2.1 concept is not just a wording revision. It is a broader governance and
operability revision of the current foundational concept.

The current repository version is stronger in some evaluation details, especially
scope-based rule evaluation, permission versus requirement evaluation, role
combination classes, explicit violation kinds, and assignment precedence.

The v2.1 candidate is stronger in productization concerns: public API boundaries,
policy composition, configuration, conformance levels, evidence confidence,
baseline and suppression separation, diagnostic ID governance, schema
versioning, and a staged V1/V1.1/V1.2/V2 roadmap.

The best next step is to merge the two concepts rather than replace the current
document wholesale.

## Major Additions In v2.1

v2.1 adds these major concepts that are absent or only implicit in the current
repository baseline:

- clear split between Core, Packs, Profiles, and Policies
- clear split between Concept Model, Analysis Model, and Result Model
- explicit public API boundary
- stable API candidates versus analyzer-internal candidates
- versioned policy and report schemas
- first-class role dimensions
- typed identifiers for roles, dimensions, policies, dependency kinds, and
  elements
- evidence and confidence levels
- policy imports and composition modes
- configuration model and precedence
- assignment providers as an extension point
- compatibility mapping from existing nMolecules packages into Bricks roles
- baseline model
- suppression model
- explicit distinction between baseline and suppression
- diagnostic ID governance ranges
- conformance levels from marking through runtime-aware analysis
- V1, V1.1, V1.2, and V2 roadmap boundaries
- Roslyn symbol identity, multi-targeting, conditional compilation, and source
  generator considerations

## Concepts Already Covered Better In Current Baseline

The current `foundational-concept.md` has several details that should be
preserved when incorporating v2.1:

- `BrickScope` as an explicit rule and violation dimension
- evaluation units for global, assembly, namespace, type, and member scope
- `Allow`, `Deny`, and `Require` as separate rule decisions
- separation of permission evaluation and requirement evaluation
- explicit evaluation pipeline
- role-combination classes: additive, exclusive, incompatible
- duplicate role instance collapse rules
- combination-aware role resolution
- explicit applied, suppressed, and conflict assignments in resolved role output
- violation kinds for dependency, requirement, role combination, and role
  resolution
- optional target and dependency kind on violations
- dependency usage detection across member bodies, constructors, destructors,
  operators, conversions, inheritance, and interface implementation

## Important Semantic Differences

### Rule Decisions

Current baseline:

- uses `Allow`, `Deny`, and `Require`
- treats severity separately from structural decision
- avoids mixing `Warn` and `Ignore` into decision semantics

v2.1:

- lists `Allow`, `Deny`, `Warn`, and `Ignore`
- later separates diagnostic and governance concepts more clearly

Recommendation:

Keep the current model: `Warn` belongs to severity and `Ignore` belongs to
policy defaults, exceptions, suppressions, or disabled rules.

### Assignment Precedence

Current baseline:

- uses structured `BrickAssignmentPrecedence`
- separates specificity from authority
- keeps equal-precedence exclusive conflicts visible

v2.1:

- returns to numeric `Priority` and `Specificity` in the sketch

Recommendation:

Keep the current structured precedence model and add v2.1's provider and
dimension concepts around it.

### Matrix Model

Current baseline:

- primary matrix is `Role x Role x DependencyKind x Scope`

v2.1:

- primary matrix is `Role x Role x DependencyKind x DependencyLayer x EvidenceLevel`

Recommendation:

Use a combined target model:

```text
Role x Role x DependencyKind x Scope x DependencyLayer x EvidenceLevel
```

For V1, keep `Scope` and static dependency kinds mandatory. Add dependency
layers and evidence levels as staged extensions.

### Baseline And Suppression

Current baseline:

- mentions operational open points around adoption and governance
- does not define first-class baseline or suppression records

v2.1:

- defines `BrickBaselineEntry`
- defines `BrickSuppression`
- defines `BrickViolationState`
- explicitly separates baselined, suppressed, expired baseline, and expired
  suppression states

Recommendation:

Adopt v2.1 almost directly here.

### API Boundary

Current baseline:

- focuses on concept model and analyzer growth
- does not explicitly classify public API, analyzer internals, and export
  contracts

v2.1:

- defines stable public API candidates
- defines analyzer-internal candidates
- requires schema-versioned export contracts

Recommendation:

Adopt v2.1. This should become a decision record because it strongly affects
package design and compatibility promises.

## Recommended Integration Plan

1. Keep `foundational-concept.md` as the authoritative merged target.
2. Treat `foundational-concept-v2.1.md` as an input candidate, not as a direct
   replacement.
3. Merge v2.1 sections into the current document in this order:
   - Core, Packs, Profiles, and Policies
   - Concept, Analysis, and Result model layering
   - Public API boundary
   - Configuration model and policy composition
   - Evidence levels and dependency layers
   - Baseline and suppression model
   - Diagnostic ID governance
   - Conformance levels
   - staged V1/V1.1/V1.2/V2 roadmap
4. Preserve the current document's stronger rule semantics:
   - `Require` decision
   - scope model
   - permission and requirement evaluation split
   - role-combination classes
   - structured assignment precedence
   - explicit violation kinds
5. After the merge, derive a separate V1 API surface specification.

## Practical Conclusion

v2.1 is the better adoption and productization layer. The current baseline is
the better rule-evaluation core.

The strongest concept is a merged v2.2 that uses the current baseline as the
semantic engine and v2.1 as the governance, API, configuration, reporting, and
roadmap layer.
