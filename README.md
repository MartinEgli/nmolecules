# nMolecules – Architectural Abstractions for .NET

A set of libraries to help developers work with architectural concepts in .NET.
Member of the xMolecules family.
Goals:

* Express that a piece of code (namespace, class, method...) implements an architectural concept.
* Make it easy for the human reader to determine what kind of architectural concepts a given piece of code is.
* Allow tool integration (to do interesting stuff like generating persistence or static architecture analysis to check for validations of the architectural rules.)

## Expressing DDD Concepts

Example:

```csharp
using NMolecules.DDD;

[Entity]
public class BankAccount
{
    [Identity]
    public IBAN IBAN { get; }

    /* ... */
}

[ValueObject]
public readonly record struct Currency { /* ... */ }

[Repository]
public interface Accounts { /* ... */ }

[DomainService]
public interface ExchangeRates { /* ... */ }

[ApplicationService]
public class TransferMoney { /* ... */ }
```

When we take Ubiquitous Language serious, we want names (for classes, methods, etc.) that only contain words from the domain language.
That means the titles of the building blocks should not be part of the names.
So in a banking domain we don't want `BankAccountEntity`, `CurrencyVO` or even `AccountRepository` as types.
Instead, we want `BankAccount`, `Currency` and `Accounts` – like in the example above.

Still, we want to express that a given class (or other architectural element) is a special building block; i.e. uses a design pattern.
nMolecules provide a set of standard annotations for the building blocks known from DDD.

In addition to entities, repositories and value objects, the library can also distinguish between domain services and
application services so that later tooling can apply more precise rules.

`[BoundedContext]` and `[Module]` can be used on assemblies/modules to add stable metadata (`Id`, `Name`, `Value`,
`Description`), with `[Module]` additionally supporting `BoundedContextId` for explicit context-module mapping.

## Expressing Eventing Concepts

TODO

## Entity Framework Mapping Metadata

Entity Framework-specific mapping hints are available in a dedicated package so core DDD markers stay persistence-agnostic.

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

## Composable Bricks

`NMolecules.Bricks` provides a generic customization layer for custom role attributes and generic rule definitions.

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
public class AccountAggregate
{
}
```

## Expressing Architecture

nMolecules provides annotations to mark several architectural styles explicitly:

```csharp
using NMolecules.Architecture.Layered;

[UserInterfaceLayer]
public class AccountsController { /* ... */ }

[ApplicationLayer]
public class TransferMoney { /* ... */ }

[DomainLayer]
public class BankAccount { /* ... */ }

[InfrastructureLayer]
public class SqlAccounts { /* ... */ }
```

Additional concepts available in the same architecture package are:

- layered architecture: `ApplicationLayer`, `DomainLayer`, `InfrastructureLayer`, `UserInterfaceLayer`, `InterfaceLayer`
- CQRS architecture: `Command`, `CommandDispatcher`, `CommandHandler`, `Query`, `QueryHandler`, `QueryModel`, `Projection`
- microservices architecture: `Microservice`, `ApiGateway`, `BackendForFrontend`, `ServiceContract`, `IntegrationEvent`, `SagaOrchestrator`, `SagaParticipant`
- Event Storming design model: `Actor`, `Command`, `DomainEvent`, `Policy`, `ReadModel`, `ExternalSystem`, `Aggregate`
- MVVM architecture: `Model`, `View`, `ViewModel`
- onion architecture:
  - classic: `DomainModelRing`, `DomainServiceRing`, `ApplicationServiceRing`, `InfrastructureRing`
  - simplified: `DomainRing`, `ApplicationRing`, `InfrastructureRing`
- hexagonal architecture: `Application`, `PrimaryAdapter`, `SecondaryAdapter`, `PrimaryPort`, `SecondaryPort`

These annotations are intended to support static rule enforcement and tooling integration, not only documentation.

## Installation

To use nMolecules in your project just install it from the NuGet Gallery.

<https://www.nuget.org/packages/NMolecules.DDD/>
<https://www.nuget.org/packages/NMolecules.Events/>
<https://www.nuget.org/packages/NMolecules.Architecture/>
<https://www.nuget.org/packages/NMolecules.Bricks/>
<https://www.nuget.org/packages/NMolecules.Persistence.EntityFramework/>

## Working Documents

For the current extension work, see:

- [docs/attribute-model.md](docs/attribute-model.md)
- [docs/microservices-attributes.md](docs/microservices-attributes.md)
- [docs/event-storming-attributes.md](docs/event-storming-attributes.md)
- [docs/bricks.md](docs/bricks.md)
- [docs/entity-framework-attributes.md](docs/entity-framework-attributes.md)

## Release Instructions

Increment the version number in one or several .csproj files and the GitHub Actions will push a new release to NuGet.

Manual steps:

* In GitHub: Create a release that points to the automatically created tag vX.Y.Z
* In NuGet: Add Readme.
* In NuGet: unlist old versions.

When the NuGet secret gets obsolete, generate a new on. See <https://netlicensing.io/blog/2020/09/01/publish-nuget-packages-using-github-actions/>
