# Microservices Attributes

Status: March 7, 2026

`NMolecules.Architecture.Microservices` provides markers for common microservices design roles.

## Marker Set

- `[Microservice]`
- `[ApiGateway]`
- `[BackendForFrontend]`
- `[ServiceContract]`
- `[IntegrationEvent]`
- `[SagaOrchestrator]`
- `[SagaParticipant]`

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

The markers make service boundaries and integration roles explicit in code:

- service boundary (`Microservice`)
- entry/API composition (`ApiGateway`, `BackendForFrontend`)
- service communication contracts (`ServiceContract`)
- asynchronous integration (`IntegrationEvent`)
- distributed workflows (`SagaOrchestrator`, `SagaParticipant`)

## Namespace Guidance

Some names overlap conceptually with other architecture families.
Prefer explicit aliases when mixing families in one file:

```csharp
using Micro = NMolecules.Architecture.Microservices;
using Cqrs = NMolecules.Architecture.Cqrs;
using Storm = NMolecules.Architecture.EventStorming;
```
