# nMolecules Attribute Model

Stand: 2026-03-03

Dieses Dokument beschreibt den aktuellen Stand der Attribute im Kernrepo und das Zielmodell fuer die naechste Ausbaustufe.

## Ziel

Das Attributmodell soll fachliche DDD- und Architekturrollen explizit im Code markieren, damit:

- Entwickler Modellierungsabsichten klar ausdruecken koennen
- Roslyn-Analyzer dieselben Rollen technisch auswerten koennen
- Visual Studio und Visual Studio Code auf derselben Fachsemantik aufbauen

## Bereits vorhandene Attribute

### DDD

- `[AggregateRoot]`
- `[BoundedContext]`
- `[Entity]`
- `[Factory]`
- `[Identity]`
- `[Module]`
- `[Repository]`
- `[Service]`
- `[ValueObject]`

### Eventing

- `[DomainEvent]`
- `[DomainEventHandler]`
- `[DomainEventPublisher]`

### Architektur

- `[ApplicationLayer]`
- `[DomainLayer]`
- `[InfrastructureLayer]`
- `[UserInterfaceLayer]`

## Einordnung des Ist-Zustands

Der Kern ist bereits brauchbar, aber fachlich noch nicht vollstaendig ausmodelliert.

Starke Punkte:

- die grundlegenden DDD-Bausteine sind vorhanden
- Eventing und Layering sind bereits angelegt
- die Attribute sind klein, fokussiert und analyzertauglich

Luecken:

- `[Service]` ist fachlich zu unscharf fuer spaetere Regeln
- fuer Application Services existiert noch kein eigener Marker
- fuer manche Regeln fehlt eine klare Entscheidung, ob Konvention oder Attribut verwendet wird
- Eventing und Architektur sind im README noch nicht voll dokumentiert

## Zielmodell fuer Phase 1

Phase 1 soll den Regelkern fuer Analyzer und IDE-Integrationen stabilisieren.

### Fest zu unterstuetzende Konzepte

- Aggregate Root
- Entity
- Value Object
- Repository
- Factory
- Domain Service
- Domain Event
- Domain Event Handler
- Domain Event Publisher
- Bounded Context
- Module
- Application Layer
- Domain Layer
- Infrastructure Layer
- User Interface Layer

## Offene Designentscheidungen

### 1. `Service` vs. `DomainService`

Aktueller Stand:

- Es existiert `[Service]`

Problem:

- fuer Analyzer-Regeln ist unklar, ob damit Domain Service, Application Service oder allgemein irgendein Service gemeint ist

Entscheidungsvorschlag:

- `[Service]` kurzfristig beibehalten
- `[DomainService]` als fachlich praezisen Marker einfuehren
- `[Service]` spaeter entweder deprecaten oder als Alias dokumentieren

### 2. `ApplicationService`

Aktueller Stand:

- nicht vorhanden

Nutzen:

- saubere Trennung zwischen Application- und Domain-Schicht
- wichtig fuer spaetere Layer-Regeln und VS/VS Code-Diagnosen

Entscheidungsvorschlag:

- `[ApplicationService]` in Phase 1 aufnehmen

### 3. Aggregate-Grenzen

Aktueller Stand:

- `AggregateRoot` und `Identity` sind vorhanden

Offene Frage:

- sollen Beziehungen zu demselben Aggregate nur konventionell oder explizit modelliert werden

Entscheidungsvorschlag:

- zunaechst nur regelbasiert auf vorhandenen Attributen arbeiten
- keine komplexen Zusatzattribute einfuehren, bevor echte Bedarfsmuster vorliegen

## Phase-1-Tasks

### Inventarisierung und Entscheidung

- Bestehende Attribute mit Fachbedeutung und Zielregeln abgleichen
- Entscheidung fuer `DomainService` und `ApplicationService` treffen
- entscheiden, ob `[Service]` uebergangsweise bestehen bleibt

### Implementierung

- fehlende Kernattribute ergaenzen
- XML-Kommentare fuer oeffentliche Attribute vervollstaendigen
- Beispielcode fuer jede Attributgruppe erstellen

### Tests

- compile-style Tests fuer neue Attribute ergaenzen
- Analyzer-relevante Minimalbeispiele dokumentieren

## Nicht Teil von Phase 1

- komplexe konfigurierbare Relationship-Attribute
- persistenzspezifische Attribute
- IDE-spezifische Attribute oder Metadaten
