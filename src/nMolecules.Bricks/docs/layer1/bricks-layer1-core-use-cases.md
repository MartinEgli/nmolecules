# NMolecules.Bricks — Use Cases: Layer 1 Core

Layer: 1 — Core Use Cases  
Depends on: `../foundational-concept.md`, `bricks-layer1-core-v2.md`  
Status: March 2026

Authoritative model reference: `../foundational-concept.md`.
This document is the entry point for Layer 1 use cases.
Each concrete use case now lives in its own file under `./use-cases/`.

## Why Layer 1 Use Cases Exist

Layer 1 is easiest to understand when shown in isolation:

- role assignment without prebuilt role packs
- rule evaluation without style-specific semantics
- violation output without extra building blocks
- resolution behavior without architecture-pack conventions

These use cases answer the question:
"What is the smallest useful Bricks system before any higher-level packs exist?"

## Use Case Index

### Assignment And Adaptation

| # | Title | File |
|---|---|---|
| UC-L1-01 | Direct assignment: one type | `use-cases/uc-l1-01-direct-role-assignment-one-type.md` |
| UC-L1-02 | Direct assignment: two types | `use-cases/uc-l1-02-direct-role-assignment-two-types.md` |
| UC-L1-03 | Alias adaptation: custom marker on existing class | `use-cases/uc-l1-03-alias-maps-type-to-canonical-role.md` |
| UC-L1-09 | External assignment: untouchable existing type | `use-cases/uc-l1-09-external-policy-assigns-role-to-existing-type.md` |
| UC-L1-10 | Alias adaptation: base type to derived types | `use-cases/uc-l1-10-base-type-alias-adapts-derived-types.md` |
| UC-L1-11 | Alias adaptation: interface to implementing types | `use-cases/uc-l1-11-interface-alias-adapts-implementing-types.md` |
| UC-L1-12 | Alias adaptation: inherited interface to implementations | `use-cases/uc-l1-12-inherited-interface-alias-reaches-implementations.md` |
| UC-L1-13 | Scoped assignment: namespace to contained types | `use-cases/uc-l1-13-namespace-role-flows-into-contained-types.md` |
| UC-L1-06 | Scoped assignment: assembly to contained type | `use-cases/uc-l1-06-assembly-role-flows-into-one-type.md` |
| UC-L1-14 | Convention assignment: namespace pattern | `use-cases/uc-l1-14-convention-assigns-role-by-namespace-pattern.md` |
| UC-L1-15 | Inference assignment: structural context | `use-cases/uc-l1-15-inference-derives-role-from-structural-context.md` |

### Resolution

| # | Title | File |
|---|---|---|
| UC-L1-16 | Resolution: direct element overrides namespace | `use-cases/uc-l1-16-direct-element-role-overrides-namespace-role.md` |
| UC-L1-17 | Resolution: additive roles accumulate | `use-cases/uc-l1-17-additive-roles-accumulate-on-one-element.md` |
| UC-L1-19 | Resolution: duplicate roles collapse unless parameterized | `use-cases/uc-l1-19-duplicate-roles-collapse-unless-parameterized.md` |
| UC-L1-07 | Resolution: stronger assignment suppresses weaker | `use-cases/uc-l1-07-stronger-assignment-suppresses-weaker.md` |
| UC-L1-08 | Resolution: equal-precedence exclusive conflict | `use-cases/uc-l1-08-equal-precedence-exclusive-conflict.md` |

### Dependency Rules

| # | Title | File |
|---|---|---|
| UC-L1-04 | Forbidden dependency: basic role-to-role rule | `use-cases/uc-l1-04-forbidden-dependency-between-two-roles.md` |
| UC-L1-20 | Forbidden dependency: explicit allowed target types | `use-cases/uc-l1-20-forbidden-dependency-except-allowed-target-types.md` |
| UC-L1-18 | Forbidden dependency: complete source-target matrix | `use-cases/uc-l1-18-forbidden-source-target-matrix.md` |
| UC-L1-21 | Forbidden dependency: coverage across all member bodies and operations | `use-cases/uc-l1-21-type-usage-is-checked-across-all-member-bodies.md` |
| UC-L1-05 | Required dependency: one target per source type | `use-cases/uc-l1-05-required-dependency-per-type.md` |

### Coverage Status

Forbidden source-target combinations are fully covered by `UC-L1-18`.
That matrix explicitly defines all 81 combinations for the Layer 1 shapes:

- type
- derived type
- interface
- inherited interface
- member
- property
- constructor
- destructor
- namespace

## Reading Order

Recommended order:

1. direct assignment
2. adaptation of existing or untouchable types
3. scoped assignment
4. convention and inference
5. resolution rules
6. basic forbidden dependency
7. explicit type allowlists
8. complete forbidden source-target matrix
9. dependency coverage inside all member bodies
10. required dependencies
