# NMolecules.Bricks AI-Assisted Enforcement

Concept version: 3.0
Revision: 2026-07-03

Version 3.0 extends deterministic Bricks enforcement with advisory AI
assistance.

The governing principle is:

> Bricks decides deterministically. AI explains, assists, and proposes.

AI output is not the source of truth for enforcement. Build failures, active
violations, suppressions, baselines, severity escalation, and policy changes
remain deterministic and reviewable.

## v3.1 Architecture Extension

The v3 runtime surface is also the base for Brick.AI code-change control and
coding-time structure guidance. The extended concept is documented in
`docs/architecture/bricks-ai-code-change-control.md` in the superproject.

The extension keeps the same authority model:

1. the analyzer or runtime produces deterministic diagnostics, violations and
   policy evidence
2. Brick.AI converts that evidence into comments, remediation options and
   proposal queues
3. an AI coding agent may request a Brick structure context before coding and
   then adapt its plan or prepare a candidate patch from deterministic evidence
4. the same analyzer and roundtrip commands verify the changed code again
5. policy mutation, suppressions, baselines and rule promotion remain reviewed

The target structural adaptation workflow uses Brick attributes as the
preferred patch style. AI may propose narrow additions such as `Role`,
`RoleAlias`, `NamespaceRole`, `TypeRole`, `Policy`, `PolicyImport`, `Rule`,
`Dependency`, `RoleCombination` or member-contract attributes, then rerun the
Bricks checks. If the new output still reports a violation, the AI must adapt
the target code or escalate a reviewed proposal instead of hiding the finding.
For greenfield or feature coding, the same concept requires AI to plan intended
roles, policy ownership, allowed dependencies and member contracts before it
writes code.

## Implemented v3 Surface

The core model covers the deterministic-to-AI handoff and the review workflow:

- `BrickAiViolationComment`
- `BrickAiCommentFactory`
- `BrickRemediationOption`
- `BrickRemediationKind`
- `BrickRemediationRisk`
- `BrickAiCommentDocument`
- `BrickAiCommentJsonSerializer`
- `BrickAiCommentMarkdownRenderer`
- `BrickRuleProposal`
- `BrickRuleProposalEvidence`
- `BrickRuleProposalQueue`
- `BrickRuleProposalQueueJsonSerializer`
- `BrickRuleProposalReview`
- `BrickRuleProposalReviewResult`
- `BrickRuleProposalReviewWorkflow`
- `BrickRuleLifecycleState`
- `BrickAiTrustBoundary`
- `BrickAiRunConfiguration`

`BrickAiViolationComment` requires a `BrickViolation`. This keeps every
AI-ready explanation traceable to deterministic Bricks evidence.

`BrickAiCommentFactory` is the adapter-ready integration point for analyzers,
CI and PR tooling. It creates comments from deterministic violations only when
the configured `BrickAiTrustBoundary` enables explanations.

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

Markdown rendering is implemented by `BrickAiCommentMarkdownRenderer`. It uses
the same deterministic comment document and therefore cannot introduce new
findings, severities or policy decisions.

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

Proposal queues are persisted with:

```text
NMolecules.Bricks.RuleProposalQueue/1.0
```

`BrickRuleProposalQueueJsonSerializer` round-trips proposal queues for CI,
review systems and analyzer maintainers.

## Human Review And Promotion

Promotion is explicit and deterministic:

1. AI creates a `BrickRuleProposal`.
2. The proposal is persisted in a `BrickRuleProposalQueue`.
3. A human reviewer records a `BrickRuleProposalReview`.
4. `BrickRuleProposalReviewWorkflow` validates reviewer, rationale, proposal id,
   required evidence and explicit `Enforced` target state.
5. Only then does the workflow produce a deterministic `BrickRule`.

No proposal mutates a policy by itself. The promoted rule must still be added to
a policy through the normal reviewed policy workflow.

## Trust Boundary

`BrickAiTrustBoundary.Default` keeps AI assistance off and disallows:

- automatic rule enforcement
- silent policy mutation
- automatic suppression creation
- automatic baseline creation

Even when rule suggestions are enabled, the model keeps enforcement separate
from explanation and proposal generation.

## CI And Analyzer Configuration

`BrickAiRunConfiguration` maps CI/MSBuild-style properties into the trust
boundary and output choices.

Supported property names:

- `NMoleculesBricksAiMode`
- `NMoleculesBricksAiCommentFormat`
- `NMoleculesBricksAiAllowRuleProposals`
- `NMoleculesBricksAiAllowAutoEnforcement`
- `NMoleculesBricksAiAllowSilentPolicyMutation`
- `NMoleculesBricksAiOutputDirectory`
- `NMoleculesBricksAiProposalQueuePath`

Unsafe automation is normalized closed unless silent policy mutation is also
explicitly enabled. The default remains `Off`.

## Adapter Boundary

v3 core is complete as an adapter-ready implementation. IDE extensions, PR bots
and CI tasks consume the JSON, Markdown and proposal queue formats. Those
adapters may choose where to display or store artifacts, but they do not become
enforcement authorities.

## Validation

The shipped v3 surface is covered by the Bricks runtime test suite and the
sample corpus:

- focused AI-assisted enforcement tests: 29 pass
- Bricks runtime suite: 451/451 pass
- `nMolecules.Bricks` line coverage: 100.00%
- `nMolecules.Bricks` branch coverage: 100.00%
- Bricks roundtrip: pass, including analyzer samples and example coverage tests
