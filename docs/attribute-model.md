# nMolecules Attribute Model

Status: March 3, 2026

This document describes the current attribute surface in the core repository and the intended model for the next expansion stage.

## Goal

The attribute model should make DDD and architectural roles explicit in code so that:

- developers can express modeling intent clearly
- Roslyn analyzers can evaluate those roles consistently
- Visual Studio and Visual Studio Code can build on the same semantics

## Current Attribute Surface

### DDD

- `[AggregateRoot]`
- `[BoundedContext]`
- `[Entity]`
- `[Factory]`
- `[Identity]`
- `[Module]`
- `[Repository]`
- `[Service]`
- `[DomainService]`
- `[ApplicationService]`
- `[ValueObject]`

## Service Role Direction

The model now distinguishes between three service markers:

### `[Service]`

- legacy compatibility marker
- still supported
- too broad for long-term modeling

### `[DomainService]`

- domain-level behavior that does not naturally belong in an entity or value object
- intended to represent domain semantics, not application orchestration

### `[ApplicationService]`

- use-case orchestration role
- intended to coordinate domain objects and supporting abstractions
- must not be treated as a domain building block

## Open Modeling Questions

The biggest remaining attribute-model questions are:

- whether `BoundedContext` should carry stronger metadata such as a name
- whether `Module` needs a richer projection than the current assembly/module marker
- how long `Service` should remain part of the public compatibility surface

## Next Core-Level Changes

1. keep the service role split stable
2. clarify `BoundedContext` semantics
3. clarify `Module` semantics
4. align XML docs, README examples, and analyzer expectations
