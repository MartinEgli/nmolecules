# NMolecules.Bricks — Use Cases: Layer 2 Building Blocks

Layer: 2 — Building Block Use Cases  
Depends on: `../foundational-concept.md`, `../layer1/bricks-layer1-core-v2.md`,
`bricks-layer2-building-blocks.md`  
Status: March 2026

Authoritative model reference: `../foundational-concept.md`.
This document contains use cases for reusable Layer 2 building blocks.
These scenarios use packaged roles, constraints, and rule templates on top of
the Layer 1 core.

## Why Layer 2 Use Cases Exist

Layer 2 answers a different question than Layer 1:

- not "can Bricks express rules at all?"
- but "which reusable building blocks should teams import instead of inventing
  everything themselves?"

---

## Use Case Index

| # | Title | Building Blocks Used |
|---|---|---|
| UC-L2-01 | Structural Core role pack for shared boundaries | Structural Core Pack |
| UC-L2-02 | Member-cardinality contract on a custom marker | Member Cardinality |
| UC-L2-03 | Naming convention inherited from a base interface | Naming Conventions |

---

## UC-L2-01: Structural Core Role Pack for Shared Boundaries

This is the simplest meaningful Layer 2 use case.

Goal:

- use a prebuilt role pack instead of inventing local role names
- classify code as `Contracts` and `Shared`
- rely on predefined combination semantics

Why this is the best first Layer 2 use case:

- it introduces reusable semantics without style-specific architecture
- it is smaller than DDD, Hexagonal, or Clean Architecture packs
- it demonstrates the value of importing a role pack at all

### Scenario

A team wants one reusable rule:

- interfaces intended for cross-boundary consumption carry `Contracts`
- helper types shared across domains carry `Shared`
- some types may validly carry both roles

That exact additive combination is already part of the Structural Core Pack.

### What This Proves

- Layer 2 adds semantic reuse on top of Layer 1
- combination rules can ship with a pack
- projects do not need to redefine basic structural roles from scratch

---

## UC-L2-02: Member-Cardinality Contract on a Custom Marker

Goal:

- define a custom marker such as `[UseCase]`
- require exactly one `Execute` member marker
- let the analyzer enforce the contract generically

This is the smallest example for a non-dependency building block.

---

## UC-L2-03: Naming Convention Inherited From a Base Interface

Goal:

- place a naming rule on an interface
- let implementing types inherit the convention
- surface a violation when the concrete type name does not match

This is the smallest example that demonstrates Layer 2 naming constraints.
