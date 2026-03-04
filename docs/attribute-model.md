# nMolecules Attribute Model

Status: March 4, 2026

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

## Context Metadata Direction

`[BoundedContext]` and `[Module]` now expose aligned metadata surfaces:

- `Id`
- `Name`
- `Value` (concise alias)
- `Description`

`[Module]` additionally exposes:

- `BoundedContextId`

This provides a stable core metadata model for catalogs, reports, and analyzer/tool integrations.

## Open Modeling Questions

The biggest remaining attribute-model question is now:

- how long `Service` should remain part of the public compatibility surface

## Next Core-Level Changes

1. keep the service role split stable
2. keep XML docs, README examples, and analyzer expectations synchronized
3. deepen architecture rule families (`Onion`, `Hexagonal`) on top of the started baseline enforcement
