# Bricks — Analyse: Externe Bibliotheken via Rollen und Regeln abbilden

Layer: Konzeptanalyse (querschnittlich)  
Datum: März 2026

Diese Analyse klärt, ob und wie existierende Strukturen aus externen .NET-
Bibliotheken mit Bricks-Rollen und -Regeln abgebildet und in die eigene
Architekturpolicy integriert werden können — ohne die externen Typen zu
verändern.

---

## Kernfrage

Externe Bibliotheken liefern Typen (Interfaces, Basisklassen, Attribute,
generische Typen), die strukturell relevant sind:

- `MediatR.IRequest<T>` — Use-Case-Eintrittspunkt
- `Microsoft.EntityFrameworkCore.DbContext` — Infrastructure-Concern
- `Microsoft.AspNetCore.Mvc.ControllerBase` — Interface-Adapter
- `FluentValidation.AbstractValidator<T>` — Anwendungslogik
- `Rebus.Handlers.IHandleMessages<T>` — Event-Handler

Können diese Typen in Bricks-Policies einbezogen werden? Können Regeln
über sie definiert werden? Können Verletzungen erkannt werden wenn eigener
Code diese Typen falsch verwendet?

---

## Verfügbare Mechanismen

Bricks bietet vier Mechanismen für externe Typen, die nicht annotiert werden
können:

| Mechanismus | Wo definiert | Wann verwenden |
|---|---|---|
| `RoleAliasAttribute` | Eigene Bridge-Klasse oder Assembly-Ebene | Typ ist bekannt, stabil, in vielen Projekten gleich |
| Externe Config (JSON/YAML) | `bricks-policy.json` | Typ kommt aus Paket das man nicht referenzieren will, oder gilt nur in diesem Projekt |
| Convention-basierte Zuweisung | Policy-Config | Typ folgt erkennbarem Namensmuster |
| Source Generator | Build-Zeit | Muster ist programmatisch erkennbar (z.B. alle DbContext-Ableitungen) |

---

## Analyse nach Bibliothek

### 1. MediatR

**Relevante externe Typen:**

| Externer Typ | Strukturelle Bedeutung | Gewünschte Bricks-Rolle |
|---|---|---|
| `IRequest<T>` | Use-Case-Eintrittspunkt | `CA.UseCases` oder `HEX.Port.Driving` |
| `IRequestHandler<TReq, TRes>` | Use-Case-Implementierung | `CA.UseCases` oder `HEX.Core` |
| `INotification` | Domain- oder Integration-Event | `DomainEvent` oder `IntegrationEvent` |
| `INotificationHandler<T>` | Event-Handler | `DomainEventHandler` |
| `IPipelineBehavior<,>` | Querschnittlicher Concern | `Shared` oder `Platform` |

**Abbildung via `RoleAliasAttribute`:**

```csharp
// Bridge-Assembly oder zentraler Policy-Deklarations-Namespace

// IRequest<T> → Use Case Eintrittspunkt
[RoleAlias(typeof(MediatR.IRequest<>), "CA.UseCases",
    Reason = "MediatR requests are use case entry points in this architecture")]
public static class MediatRRoleAliases { }

// INotification → Domain Event (wenn das Projekt nur DomainEvents über MediatR published)
[RoleAlias(typeof(MediatR.INotification), "DomainEvent",
    Reason = "All MediatR notifications in this project are domain events")]
public static class MediatREventAliases { }
```

**Einschränkung:** Wenn das Projekt `INotification` sowohl für `DomainEvent`
als auch für `IntegrationEvent` nutzt, ist eine globale Zuweisung zu einer
Rolle falsch. Dann braucht man eine differenziertere Bridge:

```csharp
// Eigene Bridge-Interfaces trennen die Bedeutung
[RoleAlias(typeof(MediatR.INotification), "DomainEvent")]
public interface IDomainEventNotification : MediatR.INotification { }

[RoleAlias(typeof(MediatR.INotification), "IntegrationEvent")]
public interface IIntegrationEventNotification : MediatR.INotification { }
```

**Bewertung: vollständig abbildbar** — MediatR arbeitet mit bekannten, stabilen
generischen Interfaces. Role Aliases auf Assembly- oder Bridge-Ebene decken
alle relevanten Typen ab.

---

### 2. Entity Framework Core

**Relevante externe Typen:**

| Externer Typ | Strukturelle Bedeutung | Gewünschte Bricks-Rolle |
|---|---|---|
| `DbContext` | Persistence-Infrastructure | `HEX.Adapter.Driven` oder `InfrastructureLayer` |
| `DbSet<T>` | Repository-Abstraktion | `HEX.Adapter.Driven` |
| `IEntityTypeConfiguration<T>` | Mapping-Konfiguration | `InfrastructureService` |
| `Migration` | Datenbankschema-Änderung | `Generated` oder `InfrastructureService` |

**Problem:** `DbContext`-Ableitungen sind oft projektspezifisch benannt
(`OrderDbContext`, `AppDbContext`). Ein globaler Alias auf `DbContext` würde
alle Ableitungen erfassen — das ist gewünscht.

```csharp
// Alle DbContext-Ableitungen gehören zur Infrastruktur
[RoleAlias(typeof(Microsoft.EntityFrameworkCore.DbContext),
    "InfrastructureLayer",
    Reason = "All DbContext derivations are infrastructure concerns")]
public static class EfCoreRoleAliases { }
```

**Wichtige Regel die sich daraus ergibt:**

```json
{
  "name": "No-DbContext-in-Core",
  "description": "Domain and use case layers must not reference DbContext",
  "sourceRole": "DomainLayer",
  "targetRole": "InfrastructureLayer",
  "decision": "Deny",
  "severity": "Error"
}
```

Diese Regel ist bereits im Architecture-Pack enthalten. Durch den Alias auf
`DbContext` wird sie automatisch auch auf `DbContext`-Nutzung in der Domäne
angewendet — ohne zusätzliche Konfiguration.

**Einschränkung — DbSet:** `DbSet<T>` als Property auf dem `DbContext` ist
eine interne Implementierungsdetail. Ob ein `DbSet<Order>` eine eigene Rolle
braucht, hängt davon ab ob die Policy Aussagen darüber machen soll. In den
meisten Architekturen reicht die `DbContext`-Rolle.

**Bewertung: gut abbildbar** — der Alias auf `DbContext` reicht für die meisten
Architekturregeln. `Migration`-Klassen können via Convention (Namespace
`Migrations.*` → `Generated`) zugewiesen werden.

---

### 3. ASP.NET Core

**Relevante externe Typen:**

| Externer Typ | Strukturelle Bedeutung | Gewünschte Bricks-Rolle |
|---|---|---|
| `ControllerBase` | Interface-Adapter (Driving) | `CA.Adapters` + `CA.Controller` oder `HEX.Adapter.Driving` |
| `Controller` | Controller mit View-Support | `CA.Adapters` + `CA.Controller` |
| `MinimalApiEndpoint` | Driving Adapter | `HEX.Adapter.Driving` |
| `IHostedService` | Background-Service | `InfrastructureService` |
| `IMiddleware` | Querschnittlicher Concern | `Platform` |
| `BackgroundService` | Background-Service | `InfrastructureService` |

```csharp
// ASP.NET Core Role Aliases
[RoleAlias(typeof(Microsoft.AspNetCore.Mvc.ControllerBase), "CA.Adapters",
    Reason = "ASP.NET Core controllers are interface adapters")]
[RoleAlias(typeof(Microsoft.AspNetCore.Mvc.ControllerBase), "CA.Controller",
    Reason = "ASP.NET Core controllers are driving adapters")]
[RoleAlias(typeof(Microsoft.Extensions.Hosting.BackgroundService), "InfrastructureService",
    Reason = "Background services are infrastructure concerns")]
[RoleAlias(typeof(Microsoft.AspNetCore.Http.IMiddleware), "Platform",
    Reason = "Middleware is a cross-cutting platform concern")]
public static class AspNetCoreRoleAliases { }
```

**Daraus folgende Regeln automatisch aktiv:**

- `CA.Core` darf nicht von `CA.Adapters` abhängen → Controller-Typen dürfen
  nicht im Domain-Layer referenziert werden
- `CA.UseCases` darf nicht von `CA.Adapters` abhängen → Use Cases dürfen
  nicht direkt `ControllerBase` kennen

**Bewertung: vollständig abbildbar** — alle ASP.NET-Core-Typen haben stabile
Basisklassen oder Interfaces die als Alias-Quelle dienen.

---

### 4. FluentValidation

**Relevante externe Typen:**

| Externer Typ | Strukturelle Bedeutung | Gewünschte Bricks-Rolle |
|---|---|---|
| `AbstractValidator<T>` | Validierungslogik | Kontextabhängig (s.u.) |
| `IValidator<T>` | Validierungs-Interface | Kontextabhängig |

**Das Architektur-Dilemma:** Wo gehört Validierung hin? Die Antwort ist
architekturstil-abhängig und kann in Bricks explizit konfiguriert werden:

**Option A — Validierung ist ein Use-Case-Concern (CA.UseCases):**

```csharp
[RoleAlias(typeof(FluentValidation.AbstractValidator<>), "CA.UseCases",
    Reason = "Validators are part of the use case / application layer")]
public static class FluentValidationAliases { }
```

**Option B — Validierung gehört in den Core (Domänen-Invarianten):**

```csharp
[RoleAlias(typeof(FluentValidation.AbstractValidator<>), "CA.Core",
    Reason = "Validators enforce domain invariants and belong in the core")]
public static class FluentValidationAliases { }
```

**Option C — Unterscheidung nach validiertem Typ:**

Wenn `AbstractValidator<PlaceOrderCommand>` ein Use-Case-Validator ist und
`AbstractValidator<Money>` ein Domain-Validator — dann reicht ein globaler
Alias nicht. Dann braucht man:

```csharp
// Eigene Marker-Interfaces
[RoleAlias(typeof(AbstractValidator<>), "CA.UseCases")]
public interface IUseCaseValidator<T> : IValidator<T> { }

[RoleAlias(typeof(AbstractValidator<>), "CA.Core")]
public interface IDomainValidator<T> : IValidator<T> { }
```

**Bewertung: abbildbar, aber Architekturentscheidung erforderlich** —
FluentValidation selbst ist neutral. Bricks kann die Entscheidung abbilden,
trifft sie aber nicht.

---

### 5. Rebus / MassTransit / NServiceBus (Message Bus)

**Relevante externe Typen:**

| Externer Typ | Strukturelle Bedeutung | Gewünschte Bricks-Rolle |
|---|---|---|
| `IHandleMessages<T>` (Rebus) | Driven-Adapter (consumes messages) | `HEX.Adapter.Driven` oder `DomainEventHandler` |
| `IConsumer<T>` (MassTransit) | Message Consumer | `HEX.Adapter.Driven` |
| `IMessageHandler<T>` (NServiceBus) | Message Handler | `HEX.Adapter.Driven` |
| Bus-Interface (`IBus`, `IPublishEndpoint`) | Driven Port (Outbound) | `HEX.Port.Driven` |

```csharp
// Rebus Handler sind Driven Adapters
[RoleAlias(typeof(Rebus.Handlers.IHandleMessages<>), "HEX.Adapter.Driven",
    Reason = "Rebus message handlers are driven adapters connecting the message bus to the core")]
public static class RebusAliases { }

// MassTransit Consumer
[RoleAlias(typeof(MassTransit.IConsumer<>), "HEX.Adapter.Driven",
    Reason = "MassTransit consumers are driven adapters")]
public static class MassTransitAliases { }
```

**Wichtige Folgefrage:** Soll der Handler direkt Domänenlogik enthalten, oder
soll er nur einen Driven Port (z.B. `IDomainEventHandler`) aufrufen?

Mit Bricks kann diese Entscheidung erzwungen werden:

```json
{
  "name": "Bus-Handler-no-direct-Core",
  "description": "Message bus handlers must delegate to a use case port, not call core directly",
  "sourceRole": "HEX.Adapter.Driven",
  "targetRole": "AggregateRoot",
  "decision": "Deny",
  "severity": "Warning"
}
```

**Bewertung: abbildbar** — alle Message-Bus-Frameworks haben generische Handler-
Interfaces. Die Frage der Delegation (Handler → Port → Core vs. Handler direkt)
ist eine Architekturentscheidung die Bricks erzwingen kann.

---

### 6. AutoMapper / Mapster

**Relevante externe Typen:**

| Externer Typ | Strukturelle Bedeutung | Gewünschte Bricks-Rolle |
|---|---|---|
| `Profile` (AutoMapper) | Mapping-Konfiguration | `CA.Adapters` (Daten-Transformation) |
| `IMapper` | Mapping-Service | `CA.Adapters` oder `Shared` |
| `ITypeConverter<,>` | Spezialisiertes Mapping | `CA.Adapters` |

```csharp
[RoleAlias(typeof(AutoMapper.Profile), "CA.Adapters",
    Reason = "AutoMapper profiles are data transformation concerns in the adapter layer")]
public static class AutoMapperAliases { }
```

**Wichtige Regel:**

```json
{
  "name": "No-Mapper-in-Core",
  "description": "Domain core must not depend on mapper infrastructure",
  "sourceRole": "CA.Core",
  "targetRole": "CA.Adapters",
  "decision": "Deny"
}
```

Da `Profile` nun `CA.Adapters` trägt, wird diese Regel automatisch auf
`AutoMapper.Profile`-Nutzung im Core angewendet.

**Bewertung: vollständig abbildbar.**

---

## Strukturelle Grenzen — Was Bricks nicht direkt abbilden kann

Nicht alle externen Strukturen sind via Alias vollständig abbildbar. Die
folgenden Grenzen sind real:

### Grenze 1: Generische Typen mit unterschiedlicher Rollenbedeutung je Typparameter

```csharp
// MediatR IRequestHandler<TRequest, TResponse>
// Wenn TRequest im DomainLayer liegt, soll der Handler CA.Core sein.
// Wenn TRequest im UseCasesLayer liegt, soll der Handler CA.UseCases sein.
// Ein globaler Alias auf IRequestHandler kann diese Unterscheidung nicht treffen.
```

**Lösung:** Bridge-Interfaces je Kontext, oder Zuweisung via externer Config
auf Namespace-Ebene statt auf Typ-Ebene.

**Status:** Abbildbar mit zusätzlichem Aufwand; kein generischer Alias.

---

### Grenze 2: DI-Registrierungen als strukturelle Aussagen

```csharp
// services.AddScoped<IOrderRepository, SqlOrderRepository>()
// Diese Aussage macht SqlOrderRepository zur Implementierung von IOrderRepository.
// Bricks kann das ohne Source Generator nicht sehen — DI ist nicht compile-time sichtbar.
```

**Folge:** Die Regel "SqlOrderRepository darf nur als Implementierung von
IOrderRepository registriert werden, nicht als eigenständiger Service" kann
Bricks ohne Source Generator nicht erzwingen.

**Status:** V1-Lücke. Source Generator (Phase 2) kann DI-Muster lesen und
als `BrickDependency`-Artefakte emittieren.

---

### Grenze 3: Attribute als strukturelle Markierungen aus externen Libraries

Manche Bibliotheken verwenden Attribute als strukturelle Marker:

```csharp
[ApiController]     // ASP.NET Core
[Authorize]         // ASP.NET Core Security
[EventHandler]      // benutzerdefiniert
```

Ein Alias auf ein Attribut-Type ist konzeptionell möglich, aber der Analyzer
müsste Attribut-Nutzung (nicht Typ-Vererbung) als Rollenquelle auswerten.
Das ist ein anderer Mechanismus als der bestehende `RoleAliasAttribute`-Pfad.

**Status:** Nicht im aktuellen Konzept. Erfordert einen neuen Zuweisungsmodus
`BrickAssignmentMode.AttributePresence`. Konzeptionell erweiterbar aber V2+.

---

### Grenze 4: Mehrfache Rollen für denselben externen Typ je Verwendungskontext

```csharp
// HttpClient kann sein:
// - Infrastruktur-Dependency (wenn direkt injiziert)
// - Teil eines Gateways (wenn in einem Adapter eingepackt)
// Ein globaler Alias kann diese Kontextabhängigkeit nicht ausdrücken.
```

**Lösung:** Regel statt Alias. Statt `HttpClient` eine Rolle zu geben, eine
Deny-Regel definieren:

```json
{
  "name": "No-HttpClient-in-Core",
  "sourceRole": "CA.Core",
  "targetRole": "System.Net.Http.HttpClient",
  "decision": "Deny"
}
```

**Status:** Erfordert dass Bricks konkrete Typen (nicht nur Rollen) als
Regel-Targets unterstützt — ein neuer Selektor-Typ `BrickTypeSelector`.
Konzeptionell erweiterbar, nicht im aktuellen Modell.

---

### Grenze 5: Externe Typen in generierten Codefiles

```csharp
// Protobuf-generierte Klassen implementieren IMessage (Google.Protobuf)
// EF Core Migrations erben von Migration (Microsoft.EntityFrameworkCore)
// gRPC-generierte Services erben von grpc::ServiceBase
```

Diese Typen können via Alias zugewiesen werden, aber der Analyzer muss
wissen, dass die generierten Dateien eine `Generated`-Rolle tragen sollten —
und dass für sie andere Regeln gelten als für handgeschriebenen Code.

**Lösung:** Kombination aus `Generated`-Rolle (via Namespace-Convention) und
Alias für den externen Basistyp:

```json
{
  "roleAssignments": [
    { "namespace": "MyApp.Protos.*", "role": "Generated" },
    { "namespace": "MyApp.Migrations.*", "role": "Generated" }
  ]
}
```

```csharp
[RoleAlias(typeof(Google.Protobuf.IMessage), "Contracts",
    Reason = "Protobuf messages are cross-boundary contracts")]
public static class ProtobufAliases { }
```

**Status:** Abbildbar mit Namespace-Convention + Alias-Kombination.

---

## Erweiterungsbedarf am Konzept

Die Analyse zeigt drei Lücken die das Konzept adressieren muss:

### Lücke 1: `BrickTypeSelector` — direkter Typselektor in Regeln

Regeln müssen konkrete externe Typen als Target nennen können ohne dass diesen
eine Rolle zugewiesen wird:

```csharp
public sealed class BrickTypeSelector
{
    public required string FullyQualifiedTypeName { get; init; }
    public string? AssemblyName { get; init; }
    public bool IncludeSubtypes { get; init; } = true;
}
```

Verwendung in einer Regel:

```json
{
  "name": "No-HttpClient-in-Core",
  "sourceRole": "CA.Core",
  "targetType": "System.Net.Http.HttpClient",
  "decision": "Deny"
}
```

Dies erlaubt Regeln über externe Typen ohne Rollenzuweisung — wichtig für
Typen, die zu generisch für eine Rolle sind.

### Lücke 2: `BrickAssignmentMode.AttributePresence`

Rollenszuweisung wenn ein bestimmtes Attribut auf einem Typ vorhanden ist:

```csharp
public enum BrickAssignmentMode
{
    DirectAttribute,
    ExternalConfiguration,
    Convention,
    Inference,
    Alias,
    AttributePresence  // neu: Role wird zugewiesen wenn Ziel-Attribut vorhanden ist
}
```

Konfiguration:

```json
{
  "roleAssignments": [
    {
      "mode": "AttributePresence",
      "attributeType": "Microsoft.AspNetCore.Mvc.ApiControllerAttribute",
      "role": "CA.Adapters",
      "reason": "Types marked [ApiController] are interface adapters"
    }
  ]
}
```

### Lücke 3: `BrickRoleAlias` mit Typparameter-Einschränkung

Für generische Typen mit kontextabhängiger Bedeutung:

```csharp
public sealed class BrickRoleAlias
{
    public required Type SourceType { get; init; }
    public required string CanonicalRoleName { get; init; }
    public string? Reason { get; init; }

    // NEU: Einschränkung auf Typparameter-Constraint
    // z.B. "gilt nur wenn TRequest : IDomainCommand"
    public Type? TypeParameterConstraint { get; init; }
    public int? TypeParameterIndex { get; init; }
}
```

---

## Gesamtbewertung

| Kategorie | Abbildbar? | Mechanismus | Einschränkung |
|---|---|---|---|
| Stabile Basisklassen (ControllerBase, DbContext) | ✓ Vollständig | `RoleAliasAttribute` | Keine |
| Generische Interfaces (IRequest<T>, IConsumer<T>) | ✓ Vollständig | `RoleAliasAttribute` | Globale Zuweisung; Typparameter-Differenzierung via Bridge |
| Typparameter-abhängige Bedeutung | ◑ Teilweise | Bridge-Interfaces oder Namespace-Config | Zusätzlicher Aufwand; Lücke 3 adressiert das langfristig |
| Attribut-basierte Marker ([ApiController]) | ✗ Noch nicht | Erfordert `AttributePresence`-Modus (Lücke 2) | V2 |
| Konkrete Typen als Regel-Target ohne Rolle | ✗ Noch nicht | Erfordert `BrickTypeSelector` (Lücke 1) | V2 |
| DI-Registrierungen als strukturelle Aussagen | ✗ Noch nicht | Source Generator (Phase 2) | V1-Lücke, bekannt |
| Generierter Code (Protobuf, EF Migrations) | ✓ Via Convention | Namespace-Convention + Alias | Keine |
| Mehrere Rollen je externem Typ | ✓ Vollständig | Mehrere `RoleAliasAttribute` | Keine |

**Fazit:** Die überwiegende Mehrheit der relevanten externen Strukturen kann
heute via `RoleAliasAttribute` und externer Config abgebildet werden. Drei
Fälle erfordern Konzepterweiterungen: Attribut-basierte Zuweisung, direkter
Typselektor in Regeln, und Typparameter-Einschränkung auf Aliases. Alle drei
sind konzeptionell erweiterbar ohne das Kernmodell zu ändern.

---

## Empfohlene Erweiterungen im Konzeptdokument (Layer 1)

Die drei Lücken sollten als explizite Near-Term-Expansion-Areas in Layer 1
aufgenommen werden:

1. `BrickTypeSelector` als direkter Typselektor in `BrickRule`
2. `BrickAssignmentMode.AttributePresence` als neuer Zuweisungsmodus
3. `BrickRoleAlias.TypeParameterConstraint` für generische Aliase

Diese Erweiterungen ändern keine bestehenden Modelltypen — sie fügen
neue Fähigkeiten zu `BrickRuleSelector`, `BrickAssignmentMode` und
`BrickRoleAlias` hinzu.

---

## Empfohlene Layer-2-Erweiterung: Library Adapter Packs

Die Bibliotheks-Aliase sollten als eigenständige Layer-2-Pakete geliefert
werden — ähnlich wie Role Packs, aber für externe Abhängigkeiten:

| Paket | Inhalt |
|---|---|
| `nMolecules.Bricks.Adapters.MediatR` | Aliases für IRequest, INotification, IRequestHandler, IPipelineBehavior |
| `nMolecules.Bricks.Adapters.EfCore` | Aliases für DbContext, DbSet, IEntityTypeConfiguration, Migration |
| `nMolecules.Bricks.Adapters.AspNetCore` | Aliases für ControllerBase, IMiddleware, BackgroundService, IHostedService |
| `nMolecules.Bricks.Adapters.FluentValidation` | Aliases für AbstractValidator, IValidator |
| `nMolecules.Bricks.Adapters.MassTransit` | Aliases für IConsumer, IPublishEndpoint |
| `nMolecules.Bricks.Adapters.Rebus` | Aliases für IHandleMessages, IBus |

Jedes Paket liefert:

- Vordefinierte `RoleAliasAttribute`-Deklarationen für die Library-Typen
- Optionale Default-Regeln die mit dem Role Pack aktiviert werden können
- Dokumentation welche Architekturentscheidung der Alias trifft

**Wichtig:** Diese Pakete sind optional und opinionated. Sie treffen
Architekturentscheidungen (z.B. "MediatR INotification = DomainEvent").
Projekte die andere Entscheidungen treffen, definieren ihre eigenen Aliases.
