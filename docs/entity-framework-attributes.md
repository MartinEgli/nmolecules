# Entity Framework Attributes

Status: March 7, 2026

This document defines the dedicated Entity Framework integration metadata in nMolecules.
These attributes are intentionally separated from the DDD core markers.

## Why A Separate Package

`NMolecules.DDD` should stay persistence-agnostic.
EF-specific mapping hints therefore live in:

- `NMolecules.Persistence.EntityFramework`

This keeps domain semantics stable while still enabling EF-focused tooling and analyzers.

## Recommended Attribute Set

The current baseline contains:

- `[EfDbContext]`
  - marks a DbContext boundary
  - supports `Name`, `BoundedContextId`, `ModuleId`

- `[EfEntityType]`
  - marks an EF entity mapping target
  - supports `Table`, `Schema`, `Keyless`

- `[EfOwnedValueObject]`
  - marks value objects intended for owned/complex EF mapping
  - supports `Owner`

- `[EfBackingField]`
  - marks explicit property-to-field mapping via `FieldName`

- `[EfConcurrencyToken]`
  - marks optimistic concurrency members
  - supports `Strategy`

- `[EfValueConverter]`
  - declares converter type via `ConverterType`

- `[EfIgnore]`
  - marks members intentionally excluded from persistence mapping
  - supports `Reason`

## Usage Direction

Use these attributes as mapping metadata and analyzer surface.
Do not move business semantics from DDD markers into EF markers.

Typical pairing:

- DDD role marker (`[AggregateRoot]`, `[ValueObject]`, ...)
- optional EF mapping marker (`[EfEntityType]`, `[EfOwnedValueObject]`, ...)
