# NMolecules.Bricks AI-Assisted Enforcement

Concept version: 3.0
Revision: 2026-06-24

Version 3.0 extends deterministic Bricks enforcement with advisory AI
assistance.

The governing principle is:

> Bricks decides deterministically. AI explains, assists, and proposes.

AI output is not the source of truth for enforcement. Build failures, active
violations, suppressions, baselines, severity escalation, and policy changes
remain deterministic and reviewable.

## Implemented Core Surface

The core model currently covers the deterministic-to-AI handoff:

- `BrickAiViolationComment`
- `BrickRemediationOption`
- `BrickRemediationKind`
- `BrickRemediationRisk`
- `BrickAiCommentDocument`
- `BrickAiCommentJsonSerializer`
- `BrickRuleProposal`
- `BrickRuleProposalEvidence`
- `BrickRuleLifecycleState`
- `BrickAiTrustBoundary`

`BrickAiViolationComment` requires a `BrickViolation`. This keeps every
AI-ready explanation traceable to deterministic Bricks evidence.

## AI-Ready Comment Schema

The JSON schema identifier is:

```text
NMolecules.Bricks.AIComment/1.0
```

An exported comment includes:

- rule id and rule name
- deterministic decision and severity
- evidence level
- source and target elements with resolved roles
- dependency kind and dependency layer when available
- problem summary
- architectural reason
- remediation options
- recommended remediation option
- AI repair hints
- suppression guidance

## Rule Proposals

AI-generated structural rules are represented as proposals, not active rules.

Allowed lifecycle states are:

- `Candidate`
- `Draft`
- `Observing`
- `Warning`
- `Rejected`
- `Deprecated`

AI-generated proposals cannot start as `Enforced`. Promotion to build-breaking
enforcement belongs to an explicit review and policy workflow.

Required proposal evidence includes observed structure, positive examples,
negative examples, false-positive risks, affected scopes, and migration impact.

## Trust Boundary

`BrickAiTrustBoundary.Default` keeps AI assistance off and disallows:

- automatic rule enforcement
- silent policy mutation
- automatic suppression creation
- automatic baseline creation

Even when rule suggestions are enabled, the model keeps enforcement separate
from explanation and proposal generation.

## Remaining Product Work

The following v3.0 items are intentionally outside this core-model slice:

- Roslyn analyzer integration that emits AI comments from real diagnostics
- IDE or PR comment rendering
- markdown comment rendering
- persistence format for rule proposal review queues
- human approval workflow for promoting proposals into policy
- CI switches and MSBuild properties for AI modes
