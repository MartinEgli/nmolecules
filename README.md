# nMolecules

Architectural and domain-role annotations for .NET.

nMolecules provides small attribute libraries that let code express domain,
architecture, eventing, persistence, and structural-governance intent directly
in source. The libraries are part of the xMolecules family and are designed to
support human readability, static analysis, IDE tooling, and report generation.

Source: assembled from the previous `README.md`, `docs/attribute-model.md`, and
the current project structure under `src/`.

## What nMolecules Is

nMolecules helps teams make architectural language explicit in code:

- DDD concepts such as entities, aggregates, repositories, services, modules,
  and bounded contexts
- architecture-style concepts such as layered, CQRS, hexagonal, onion,
  microservices, Event Storming, and MVVM roles
- persistence mapping metadata for Entity Framework without coupling DDD
  markers to EF
- generic Bricks roles, rules, policies, violations, reports, exports,
  benchmarks, and governance models

The core idea is simple:

1. Mark code with semantic roles.
2. Let tooling resolve those roles deterministically.
3. Evaluate rules and produce diagnostics, reports, exports, or AI-ready
   explanations.

Source: previous `README.md`, `docs/attribute-model.md`, and
`src/nMolecules.Bricks/docs/foundational-concept-v2.2.md`.

## Packages

Current source projects:

- `NMolecules.DDD`
- `NMolecules.Events`
- `NMolecules.Architecture`
- `NMolecules.Architecture.Layered`
- `NMolecules.Architecture.Cqrs`
- `NMolecules.Architecture.Onion`
- `NMolecules.Architecture.Hexagonal`
- `NMolecules.Architecture.Microservices`
- `NMolecules.Architecture.EventStorming`
- `NMolecules.Architecture.Mvvm`
- `NMolecules.Bricks`
- `NMolecules.Persistence.EntityFramework`

Source: `src/*/*.csproj`.

## DDD Markers

`NMolecules.DDD` provides annotations for common tactical and contextual DDD
building blocks.

Example:

```csharp
using NMolecules.DDD;

[Entity]
public class BankAccount
{
    [Identity]
    public IBAN IBAN { get; }
}

[ValueObject]
public readonly record struct Currency;

[Repository]
public interface Accounts;

[DomainService]
public interface ExchangeRates;

[ApplicationService]
public class TransferMoney;
```

The intended naming style is domain-first. Code should not need names such as
`BankAccountEntity` or `CurrencyVO`; the annotation carries the building-block
role, while the type name stays in the domain language.

Current DDD marker surface includes:

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

Source: previous `README.md` and `docs/attribute-model.md`.

## Architecture Markers

nMolecules provides architecture marker packages for several architecture
families. The aggregate package `NMolecules.Architecture` keeps a broad
compatibility surface; style-specific packages provide narrower dependency
surfaces.

Example:

```csharp
using NMolecules.Architecture.Layered;

[UserInterfaceLayer]
public class AccountsController;

[ApplicationLayer]
public class TransferMoney;

[DomainLayer]
public class BankAccount;

[InfrastructureLayer]
public class SqlAccounts;
```

Current architecture marker families:

- Layered: `ApplicationLayer`, `DomainLayer`, `InfrastructureLayer`,
  `UserInterfaceLayer`, `InterfaceLayer`
- CQRS: `Command`, `CommandDispatcher`, `CommandHandler`, `Query`,
  `QueryHandler`, `QueryModel`, `Projection`
- Microservices: `Microservice`, `ApiGateway`, `BackendForFrontend`,
  `ServiceContract`, `IntegrationEvent`, `SagaOrchestrator`,
  `SagaParticipant`
- Event Storming: `Actor`, `Command`, `DomainEvent`, `Policy`, `ReadModel`,
  `ExternalSystem`, `Aggregate`
- MVVM: `Model`, `View`, `ViewModel`
- Onion classic: `DomainModelRing`, `DomainServiceRing`,
  `ApplicationServiceRing`, `InfrastructureRing`
- Onion simplified: `DomainRing`, `ApplicationRing`, `InfrastructureRing`
- Hexagonal: `Application`, `PrimaryAdapter`, `SecondaryAdapter`,
  `PrimaryPort`, `SecondaryPort`

Source: previous `README.md`, `docs/attribute-model.md`, and
`docs/microservices-attributes.md`.

## Entity Framework Metadata

Entity Framework mapping metadata lives in
`NMolecules.Persistence.EntityFramework` so that `NMolecules.DDD` remains
persistence-agnostic.

Example:

```csharp
using NMolecules.DDD;
using NMolecules.Persistence.EntityFramework;

[AggregateRoot]
[EfEntityType("bank_accounts", Schema = "billing")]
public class BankAccount
{
    [Identity]
    public string Id { get; private set; } = string.Empty;

    [EfConcurrencyToken(Strategy = "rowversion")]
    public byte[] Version { get; private set; } = Array.Empty<byte>();
}
```

Current EF metadata includes:

- `[EfDbContext]`
- `[EfEntityType]`
- `[EfOwnedValueObject]`
- `[EfBackingField]`
- `[EfConcurrencyToken]`
- `[EfValueConverter]`
- `[EfIgnore]`

Source: previous `README.md` and `docs/entity-framework-attributes.md`.

## Bricks

`NMolecules.Bricks` is the generic semantic role-and-rule foundation below the
specialized concept packages.

It models:

- elements, roles, role dimensions, and typed identifiers
- explicit and indirect role assignment
- deterministic role resolution
- allow, deny, and require rules
- policy composition and configuration precedence
- baselines, suppressions, adoption state, and governance
- JSON, SARIF, role-map, dependency-graph, and trace exports
- benchmark reports and benchmark baseline comparisons
- conformance, roadmap, dependency-coverage, and governance reports

Example:

```csharp
using NMolecules.Bricks;

[assembly: Rule(
    "BILL-ARCH-001",
    "Billing.Domain",
    "Billing.Infrastructure",
    RuleMode.ForbidDependency,
    "Rule {rule}: {source} must not depend on {target} via {member}")]

[Role("Billing.Domain")]
public class AccountAggregate;
```

Source: previous `README.md`, `docs/bricks.md`,
`src/nMolecules.Bricks/docs/api-catalog.md`,
`src/nMolecules.Bricks/docs/foundational-concept-v2.2.md`, and
`src/nMolecules.Bricks/docs/foundational-concept-v2.2-implementation-audit.md`.

## AI-Assisted Enforcement

The Bricks v3 concept adds an advisory AI layer without moving enforcement
authority away from deterministic Bricks rules.

The rule is:

> Bricks decides deterministically. AI explains, assists, and proposes.

Implemented core surface currently includes AI-ready violation comments,
remediation options, JSON comment export, rule proposals, proposal evidence,
proposal lifecycle state, and a trust-boundary model. AI-generated proposals
cannot start as enforced rules.

AI output is advisory. It must not silently activate rules, suppress
violations, create baselines, escalate severity, override policy, or decide
build-breaking enforcement.

Source: `src/nMolecules.Bricks/docs/ai-assisted-enforcement.md` and the
implemented types in `src/nMolecules.Bricks/BrickAiAssistedEnforcementModel.cs`.

## Installation

Install the packages you need from NuGet:

- <https://www.nuget.org/packages/NMolecules.DDD/>
- <https://www.nuget.org/packages/NMolecules.Events/>
- <https://www.nuget.org/packages/NMolecules.Architecture/>
- <https://www.nuget.org/packages/NMolecules.Architecture.Layered/>
- <https://www.nuget.org/packages/NMolecules.Architecture.Cqrs/>
- <https://www.nuget.org/packages/NMolecules.Architecture.Onion/>
- <https://www.nuget.org/packages/NMolecules.Architecture.Hexagonal/>
- <https://www.nuget.org/packages/NMolecules.Architecture.Microservices/>
- <https://www.nuget.org/packages/NMolecules.Architecture.EventStorming/>
- <https://www.nuget.org/packages/NMolecules.Architecture.Mvvm/>
- <https://www.nuget.org/packages/NMolecules.Bricks/>
- <https://www.nuget.org/packages/NMolecules.Persistence.EntityFramework/>

Source: previous `README.md`.

## Build And Test

From this repository:

```powershell
dotnet build nMolecules.sln -v minimal
dotnet test nMolecules.sln -v minimal
```

Main test projects:

- `tests/Molecules.DDD.Test`
- `tests/Molecules.Architecture.Test`
- `tests/Molecules.Events.Test`
- `tests/Molecules.Bricks.Test`
- `tests/Molecules.Persistence.EntityFramework.Test`

Source: current solution/project layout under `tests/` and previous workspace
README build instructions.

## Documentation

Current working documents:

- [Attribute model](docs/attribute-model.md)
- [Bricks](docs/bricks.md)
- [Entity Framework attributes](docs/entity-framework-attributes.md)
- [Event Storming attributes](docs/event-storming-attributes.md)
- [Microservices attributes](docs/microservices-attributes.md)
- [Bricks API catalog](src/nMolecules.Bricks/docs/api-catalog.md)
- [Bricks developer area guide](src/nMolecules.Bricks/docs/area-guide.md)
- [Bricks v2.2 foundational concept](src/nMolecules.Bricks/docs/foundational-concept-v2.2.md)
- [Bricks v2.2 implementation audit](src/nMolecules.Bricks/docs/foundational-concept-v2.2-implementation-audit.md)
- [Bricks AI-assisted enforcement](src/nMolecules.Bricks/docs/ai-assisted-enforcement.md)

Source: current `docs/` directory and `src/nMolecules.Bricks/docs/`.

## Source Map For This README

This README is a synthesis of existing repository material, not a new external
source. The main inputs were:

- previous `README.md`: original project description, examples, package links,
  release notes, and high-level goals
- `docs/attribute-model.md`: current marker surface for DDD, architecture, EF,
  and Bricks
- `docs/bricks.md`: shipped Bricks attribute surface and customization guidance
- `docs/entity-framework-attributes.md`: EF package boundary and metadata list
- `docs/microservices-attributes.md`: microservices marker list and purpose
- `src/nMolecules.Bricks/docs/foundational-concept-v2.2.md`: Bricks conceptual
  model and boundary statement
- `src/nMolecules.Bricks/docs/foundational-concept-v2.2-implementation-audit.md`:
  implemented Bricks v2.2 evidence
- `src/nMolecules.Bricks/docs/ai-assisted-enforcement.md`: v3 advisory AI
  boundary
- `src/*/*.csproj` and `tests/*/*.csproj`: current package and test project
  inventory

Assembled on 2026-06-24.
