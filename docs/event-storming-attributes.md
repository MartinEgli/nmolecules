# Event Storming Attributes

Status: March 7, 2026

`NMolecules.Architecture.EventStorming` provides a dedicated marker family for Event Storming design artifacts.

## Marker Set

- `[Actor]`
- `[Command]`
- `[DomainEvent]`
- `[Policy]`
- `[ReadModel]`
- `[ExternalSystem]`
- `[Aggregate]`

All markers target:

- assembly
- module
- class
- interface
- struct

All markers expose optional metadata:

- `Name` (default: empty string)
- `Description` (default: empty string)

## Purpose

This marker family is intended for design-level modeling and documentation in code.
It complements DDD and CQRS markers by making Event Storming board elements explicit and machine-readable.

## Namespace Guidance

There are intentionally overlapping names with other families (for example `Command` and `DomainEvent`).
Use explicit namespace imports when multiple families are used in the same file:

```csharp
using Storm = NMolecules.Architecture.EventStorming;
using Cqrs = NMolecules.Architecture.Cqrs;
```
