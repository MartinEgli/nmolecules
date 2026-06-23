# UC-L1-18: Forbidden Source-Target Matrix Across Structural Shapes

Layer: 1 — Core  
Depends on: `../../foundational-concept.md`, `../bricks-layer1-core-v2.md`, `uc-l1-04-forbidden-dependency-between-two-roles.md`

## Goal

- enumerate all forbidden source-target shape combinations for Layer 1
- make explicit how each shape is normalized into the core model
- cover both source and target for:
  - types
  - derived types
  - interfaces
  - inherited interfaces
  - members
  - properties
  - constructors
  - destructors
  - namespaces

## Why This Is A Matrix And Not 81 Separate Files

The forbidden-dependency mechanism is the same across all combinations:

- resolve effective roles
- detect a dependency
- evaluate the matching rule
- emit one normalized `BrickViolation`

What changes is the structural shape of the source and target, and how that
shape is normalized into Layer 1 core concepts.

This document is therefore the complete coverage matrix for forbidden
combinations. `UC-L1-04` remains the minimal executable example.

For target families such as class, derived class, interface, and inherited
interface, the granular files combine the target family with its concrete
occurrence locations in code.

Examples:

- class as parameter
- class as field or member type
- class in a property
- class in a constructor
- interface in a method return type

The matrix is also split into fully granular files under:

`./forbidden-source-target-matrix/`

There is:

- one folder per source shape
- one file per source-target combination
- one folder-level README per source shape

Coverage statement:

- 9 source shapes
- 9 target shapes
- 81 source-target combinations
- all combinations are defined below
- member-level evidence includes usages inside methods, functions, procedures,
  field initializers, property accessors, constructors, destructors, and nested
  operations

## Normalization Rules

Before listing the matrix, the shape categories must be normalized:

| Shape | Normalized element kind | Primary scope | Notes |
|---|---|---|---|
| Type | `Type` | `Type` | direct type-level dependency |
| Derived type | `Type` | `Type` | same as type; inheritance is part of discovery |
| Interface | `Type` | `Type` | interface symbols are type-level elements |
| Inherited interface | `Type` | `Type` | interface inheritance chain still normalizes to type-level |
| Member | `Member` | `Member` | generic method/field/event/member case |
| Property | `Member` | `Member` | specialized member shape |
| Constructor | `Member` | `Member` | specialized member shape |
| Destructor | `Member` | `Member` | specialized member shape |
| Namespace | `Namespace` | `Namespace` | aggregated dependency over contained elements |

Interpretation:

- `Type`, `Derived type`, `Interface`, and `Inherited interface` are distinct
  use-case shapes, but the core engine evaluates them as type-level elements.
- `Member`, `Property`, `Constructor`, and `Destructor` are distinct use-case
  shapes, but the core engine evaluates them as member-level elements.
- `Namespace` is not a raw symbol-to-symbol dependency in the same sense; it is
  an aggregated structural view over contained dependencies.

## Complete Matrix

Legend:

- `Direct` = direct source-target shape pair in the normalized model
- `Normalized` = shape-specific case, but normalized to the stated core kinds
- `Aggregated` = namespace-level view over contained element dependencies

### Source: Type

| ID | Target shape | Evaluation form | Notes |
|---|---|---|---|
| F-01 | Type | Direct | classic type-to-type forbidden dependency |
| F-02 | Derived type | Normalized to `Type -> Type` | target inheritance does not change rule semantics |
| F-03 | Interface | Normalized to `Type -> Type` | type depends on interface contract |
| F-04 | Inherited interface | Normalized to `Type -> Type` | interface hierarchy still resolves as type-level target |
| F-05 | Member | Source role inherited into `Member -> Member` | target is the addressed member itself |
| F-06 | Property | Source role inherited into `Member -> Member` | target is the addressed property |
| F-07 | Constructor | Source role inherited into `Member -> Member` | target is the invoked constructor |
| F-08 | Destructor | Source role inherited into `Member -> Member` | target is the destructor member |
| F-09 | Namespace | Aggregated | namespace target is a grouped structural view |

### Source: Derived Type

| ID | Target shape | Evaluation form | Notes |
|---|---|---|---|
| F-10 | Type | Normalized to `Type -> Type` | source inheritance does not change rule semantics |
| F-11 | Derived type | Normalized to `Type -> Type` | both ends are inheritance-based shapes |
| F-12 | Interface | Normalized to `Type -> Type` | derived source depends on interface target |
| F-13 | Inherited interface | Normalized to `Type -> Type` | target contract reached through interface hierarchy |
| F-14 | Member | Mixed shape | member evidence may be more precise than type-level source |
| F-15 | Property | Mixed shape | property target as specialized member |
| F-16 | Constructor | Mixed shape | ctor target as specialized member |
| F-17 | Destructor | Mixed shape | destructor target as specialized member |
| F-18 | Namespace | Aggregated | namespace target is grouped from contained dependencies |

### Source: Interface

| ID | Target shape | Evaluation form | Notes |
|---|---|---|---|
| F-19 | Type | Normalized to `Type -> Type` | interface-level dependency to concrete type |
| F-20 | Derived type | Normalized to `Type -> Type` | target inheritance is secondary to type-level rule |
| F-21 | Interface | Direct at type level | interface-to-interface forbidden relationship |
| F-22 | Inherited interface | Normalized to `Type -> Type` | interface hierarchy on target side |
| F-23 | Member | Mixed shape | member target beneath interface role boundary |
| F-24 | Property | Mixed shape | property target beneath interface role boundary |
| F-25 | Constructor | Mixed shape | constructor target beneath interface role boundary |
| F-26 | Destructor | Mixed shape | destructor target beneath interface role boundary |
| F-27 | Namespace | Aggregated | namespace target groups downstream dependencies |

### Source: Inherited Interface

| ID | Target shape | Evaluation form | Notes |
|---|---|---|---|
| F-28 | Type | Normalized to `Type -> Type` | inherited interface as source shape |
| F-29 | Derived type | Normalized to `Type -> Type` | target is inherited concrete hierarchy |
| F-30 | Interface | Normalized to `Type -> Type` | direct interface target |
| F-31 | Inherited interface | Normalized to `Type -> Type` | both ends are interface hierarchies |
| F-32 | Member | Mixed shape | member target below inherited interface source |
| F-33 | Property | Mixed shape | property target below inherited interface source |
| F-34 | Constructor | Mixed shape | constructor target below inherited interface source |
| F-35 | Destructor | Mixed shape | destructor target below inherited interface source |
| F-36 | Namespace | Aggregated | namespace target summarizes grouped target surface |

### Source: Member

| ID | Target shape | Evaluation form | Notes |
|---|---|---|---|
| F-37 | Type | Mixed shape with member evidence | common case: a method touches a forbidden type |
| F-38 | Derived type | Mixed shape with member evidence | target remains type-level despite inheritance |
| F-39 | Interface | Mixed shape with member evidence | member depends on interface target |
| F-40 | Inherited interface | Mixed shape with member evidence | target interface chain remains type-level |
| F-41 | Member | Direct at member level | member-to-member forbidden dependency |
| F-42 | Property | Direct at member level | property target as specialized member |
| F-43 | Constructor | Direct at member level | constructor target as specialized member |
| F-44 | Destructor | Direct at member level | destructor target as specialized member |
| F-45 | Namespace | Aggregated | namespace target is grouped upward from member evidence |

### Source: Property

| ID | Target shape | Evaluation form | Notes |
|---|---|---|---|
| F-46 | Type | Normalized to `Member -> Type` | property source is a specialized member |
| F-47 | Derived type | Normalized to `Member -> Type` | target inheritance does not change type normalization |
| F-48 | Interface | Normalized to `Member -> Type` | property depends on interface target |
| F-49 | Inherited interface | Normalized to `Member -> Type` | target interface chain remains type-level |
| F-50 | Member | Normalized to `Member -> Member` | property source to generic member target |
| F-51 | Property | Direct specialized member pair | property-to-property forbidden dependency |
| F-52 | Constructor | Normalized to `Member -> Member` | property source to constructor target |
| F-53 | Destructor | Normalized to `Member -> Member` | property source to destructor target |
| F-54 | Namespace | Aggregated | namespace target summarized from property evidence |

### Source: Constructor

| ID | Target shape | Evaluation form | Notes |
|---|---|---|---|
| F-55 | Type | Normalized to `Member -> Type` | common object creation source |
| F-56 | Derived type | Normalized to `Member -> Type` | target inheritance is secondary |
| F-57 | Interface | Normalized to `Member -> Type` | constructor depends on interface target |
| F-58 | Inherited interface | Normalized to `Member -> Type` | inherited interface target |
| F-59 | Member | Normalized to `Member -> Member` | constructor source to member target |
| F-60 | Property | Normalized to `Member -> Member` | constructor source to property target |
| F-61 | Constructor | Direct specialized member pair | constructor-to-constructor forbidden dependency |
| F-62 | Destructor | Normalized to `Member -> Member` | constructor source to destructor target |
| F-63 | Namespace | Aggregated | namespace target grouped from constructor evidence |

### Source: Destructor

| ID | Target shape | Evaluation form | Notes |
|---|---|---|---|
| F-64 | Type | Normalized to `Member -> Type` | rare but still expressible |
| F-65 | Derived type | Normalized to `Member -> Type` | target inheritance is secondary |
| F-66 | Interface | Normalized to `Member -> Type` | destructor depends on interface target |
| F-67 | Inherited interface | Normalized to `Member -> Type` | inherited interface target |
| F-68 | Member | Normalized to `Member -> Member` | destructor source to member target |
| F-69 | Property | Normalized to `Member -> Member` | destructor source to property target |
| F-70 | Constructor | Normalized to `Member -> Member` | destructor source to constructor target |
| F-71 | Destructor | Direct specialized member pair | destructor-to-destructor forbidden dependency |
| F-72 | Namespace | Aggregated | namespace target grouped from destructor evidence |

### Source: Namespace

| ID | Target shape | Evaluation form | Notes |
|---|---|---|---|
| F-73 | Type | Aggregated namespace-to-type view | target type is reached through contained dependencies |
| F-74 | Derived type | Aggregated namespace-to-type view | target inheritance still collapses to type view |
| F-75 | Interface | Aggregated namespace-to-type view | target interface still collapses to type view |
| F-76 | Inherited interface | Aggregated namespace-to-type view | target interface chain still collapses to type view |
| F-77 | Member | Aggregated namespace-to-member view | grouped member targets inside a namespace |
| F-78 | Property | Aggregated namespace-to-member view | grouped property targets inside a namespace |
| F-79 | Constructor | Aggregated namespace-to-member view | grouped constructor targets inside a namespace |
| F-80 | Destructor | Aggregated namespace-to-member view | grouped destructor targets inside a namespace |
| F-81 | Namespace | Direct aggregated namespace pair | canonical namespace-to-namespace forbidden dependency |

## Recommended Rule Shapes

Use these preferred rule shapes when modeling forbidden combinations:

- `Type -> Type` for all type-family and interface-family combinations
- `Member -> Member` for member, property, constructor, and destructor cases
- `Namespace -> Namespace` for aggregated package or module boundaries

Mixed-shape combinations should normally be reported from the most precise
evidence level available:

- if the evidence is a concrete property, constructor, or method use, prefer
  member-level evidence
- if the policy intent is architectural grouping, aggregate upward to namespace
  scope

## Practical Guidance

If you want full forbidden-combination coverage in policy design, the practical
minimum is:

1. one type-level forbidden rule family
2. one member-level forbidden rule family
3. one namespace-level forbidden rule family
4. clear normalization rules for inherited types, interfaces, properties,
   constructors, and destructors

That yields complete coverage of the matrix without requiring 81 distinct rule
implementations.
