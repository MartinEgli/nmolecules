# Bricks DDD Sample

This sample shows how an application can build a small DDD-specific Brick layer
on top of `NMolecules.Bricks`. The same scenario is covered by executable
analyzer tests in
`tests/nMolecules.Bricks.Analyzers.Test/BrickAnalyzerCoverageTest.cs`.

## 1. Define project-specific roles

Use role constants when the same role name appears in attributes, rules,
documentation and tests.

```csharp
using System;
using NMolecules.Bricks;

public static class DddBrickRoles
{
    public const string DomainModel = "DDD.DomainModel";
    public const string ApplicationService = "DDD.ApplicationService";
    public const string InfrastructureAdapter = "DDD.InfrastructureAdapter";
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
[RoleAlias(DddBrickRoles.DomainModel)]
public sealed class DomainModelAttribute : RoleAttribute
{
    public DomainModelAttribute() : base(DddBrickRoles.DomainModel)
    {
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
[RoleAlias(DddBrickRoles.ApplicationService)]
public sealed class ApplicationServiceAttribute : RoleAttribute
{
    public ApplicationServiceAttribute() : base(DddBrickRoles.ApplicationService)
    {
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
[RoleAlias(DddBrickRoles.InfrastructureAdapter)]
public sealed class InfrastructureAdapterAttribute : RoleAttribute
{
    public InfrastructureAdapterAttribute() : base(DddBrickRoles.InfrastructureAdapter)
    {
    }
}
```

## 2. Declare architecture rules

Rules are assembly metadata. They are visible to the analyzer during a normal
IDE or build analysis pass.

```csharp
[assembly: Rule(
    "DDD001",
    DddBrickRoles.DomainModel,
    DddBrickRoles.InfrastructureAdapter,
    RuleMode.ForbidDependency,
    "Domain model code must not depend on infrastructure adapters.")]
```

## 3. Passing implementation

The domain model keeps its own language. The application service may depend on
the domain model, and infrastructure may depend on domain contracts. The
forbidden dependency direction is not used.

```csharp
namespace Sales.Domain.Model
{
    [DomainModel]
    public sealed class OrderAggregate
    {
        public OrderId Id { get; init; } = new OrderId();
    }

    [DomainModel]
    public sealed class OrderId
    {
    }
}

namespace Sales.Application
{
    [ApplicationService]
    public sealed class SubmitOrderHandler
    {
        private readonly Sales.Domain.Model.OrderAggregate order = default!;
    }
}

namespace Sales.Infrastructure.Persistence
{
    [InfrastructureAdapter]
    public sealed class SqlOrderRepository
    {
    }
}
```

## 4. Violation example

This code reports `XMoleculesBricks0001`, because a domain model type depends
on an infrastructure adapter.

```csharp
namespace Sales.Domain.Model
{
    [DomainModel]
    public sealed class OrderAggregate
    {
        private readonly Sales.Infrastructure.Persistence.SqlOrderRepository repository = default!;
    }
}

namespace Sales.Infrastructure.Persistence
{
    [InfrastructureAdapter]
    public sealed class SqlOrderRepository
    {
    }
}
```

## 5. Namespace role variant

When a project already uses strict namespace conventions, the same roles can be
assigned at assembly level. Moving a type into or out of the mapped namespace
then changes how the analyzer classifies it.

```csharp
[assembly: NamespaceRole("Sales.Domain.*", DddBrickRoles.DomainModel)]
[assembly: NamespaceRole("Sales.Infrastructure.*", DddBrickRoles.InfrastructureAdapter)]
```

Use direct attributes for explicit exceptions and namespace roles for broad
project conventions. The analyzer coverage tests verify both styles.

## 6. Executable analyzer samples

The following complete snippets are checked by
`tests/nMolecules.Bricks.Analyzers.Test/BrickSampleConsistencyAnalyzerTest.cs`.
Use `analyzer-pass` for snippets that must not produce diagnostics and
`analyzer-violation <diagnostic-id>` for snippets that must produce a specific
diagnostic.

```csharp analyzer-pass
using System;
using NMolecules.Bricks;

[assembly: Rule(
    "DDD001",
    DddBrickRoles.DomainModel,
    DddBrickRoles.InfrastructureAdapter,
    RuleMode.ForbidDependency)]

public static class DddBrickRoles
{
    public const string DomainModel = "DDD.DomainModel";
    public const string ApplicationService = "DDD.ApplicationService";
    public const string InfrastructureAdapter = "DDD.InfrastructureAdapter";
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
[RoleAlias(DddBrickRoles.DomainModel)]
public sealed class DomainModelAttribute : RoleAttribute
{
    public DomainModelAttribute() : base(DddBrickRoles.DomainModel)
    {
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
[RoleAlias(DddBrickRoles.ApplicationService)]
public sealed class ApplicationServiceAttribute : RoleAttribute
{
    public ApplicationServiceAttribute() : base(DddBrickRoles.ApplicationService)
    {
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
[RoleAlias(DddBrickRoles.InfrastructureAdapter)]
public sealed class InfrastructureAdapterAttribute : RoleAttribute
{
    public InfrastructureAdapterAttribute() : base(DddBrickRoles.InfrastructureAdapter)
    {
    }
}

namespace Sales.Domain.Model
{
    [DomainModel]
    public sealed class OrderAggregate
    {
        public OrderId Id { get; init; } = new OrderId();
    }

    [DomainModel]
    public sealed class OrderId
    {
    }
}

namespace Sales.Application
{
    [ApplicationService]
    public sealed class SubmitOrderHandler
    {
        private readonly Sales.Domain.Model.OrderAggregate order = default!;
    }
}

namespace Sales.Infrastructure.Persistence
{
    [InfrastructureAdapter]
    public sealed class SqlOrderRepository
    {
    }
}
```

```csharp analyzer-violation XMoleculesBricks0001
using System;
using NMolecules.Bricks;

[assembly: Rule(
    "DDD001",
    DddBrickRoles.DomainModel,
    DddBrickRoles.InfrastructureAdapter,
    RuleMode.ForbidDependency)]

public static class DddBrickRoles
{
    public const string DomainModel = "DDD.DomainModel";
    public const string InfrastructureAdapter = "DDD.InfrastructureAdapter";
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
[RoleAlias(DddBrickRoles.DomainModel)]
public sealed class DomainModelAttribute : RoleAttribute
{
    public DomainModelAttribute() : base(DddBrickRoles.DomainModel)
    {
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
[RoleAlias(DddBrickRoles.InfrastructureAdapter)]
public sealed class InfrastructureAdapterAttribute : RoleAttribute
{
    public InfrastructureAdapterAttribute() : base(DddBrickRoles.InfrastructureAdapter)
    {
    }
}

namespace Sales.Domain.Model
{
    [DomainModel]
    public sealed class OrderAggregate
    {
        private readonly Sales.Infrastructure.Persistence.SqlOrderRepository repository = default!;
    }
}

namespace Sales.Infrastructure.Persistence
{
    [InfrastructureAdapter]
    public sealed class SqlOrderRepository
    {
    }
}
```
