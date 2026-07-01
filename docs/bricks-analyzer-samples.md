# Bricks Analyzer Samples

These samples are intentionally small. Each code block is compiled by
`BrickSampleConsistencyAnalyzerTest`, so the documented pass and violation
expectations stay aligned with the analyzers.

## Namespace Role Violation

```csharp analyzer-violation XMoleculesBricks0001
using NMolecules.Bricks;

[assembly: NamespaceRole("Sales.Domain.*", "Domain")]
[assembly: NamespaceRole("Sales.Infrastructure", "Infrastructure")]
[assembly: Rule("R1", "Domain", "Infrastructure", RuleMode.ForbidDependency)]

namespace Sales.Domain.Model
{
    public sealed class Order
    {
        private readonly Sales.Infrastructure.SqlGateway gateway = default!;
    }
}

namespace Sales.Infrastructure
{
    public sealed class SqlGateway
    {
    }
}
```

## Rule Filter Pass

```csharp analyzer-pass
using NMolecules.Bricks;

[assembly: Rule("R1", "Domain", "Infrastructure", RuleMode.ForbidDependency)]
[assembly: ExcludedTargetNameContains("R1", "Allowed")]

[Role("Domain")]
public sealed class Order
{
    private readonly AllowedInfrastructureGateway gateway = default!;
}

[Role("Infrastructure")]
public sealed class AllowedInfrastructureGateway
{
}
```

## Default Deny With Allow Rule

```csharp analyzer-pass
using NMolecules.Bricks;

[assembly: Policy("P1", defaultDecision: BrickPermissionDefault.Deny)]
[assembly: Rule("R1", "Application", "Domain", RuleMode.AllowDependency)]

[Role("Application")]
public sealed class SubmitOrderHandler
{
    private readonly Order order = default!;
}

[Role("Domain")]
public sealed class Order
{
}
```

## Attribute Type Argument Violation

```csharp analyzer-violation XMoleculesBricks0001
using System;
using NMolecules.Bricks;

[assembly: Rule("R1", "Domain", "Infrastructure", RuleMode.ForbidDependency)]

public sealed class UsesTypeAttribute : Attribute
{
    public UsesTypeAttribute(Type type)
    {
    }
}

[Role("Domain")]
[UsesType(typeof(SqlGateway))]
public sealed class Order
{
}

[Role("Infrastructure")]
public sealed class SqlGateway
{
}
```

## Extension Method Violation

```csharp analyzer-violation XMoleculesBricks0001
using NMolecules.Bricks;

[assembly: Rule("R1", "Domain", "Infrastructure", RuleMode.ForbidDependency)]

[Role("Domain")]
public sealed class Order
{
    public void Save()
    {
        this.Store();
    }
}

[Role("Infrastructure")]
public static class InfrastructureExtensions
{
    public static void Store(this Order order)
    {
    }
}
```

## Interface Role Propagation Violation

```csharp analyzer-violation XMoleculesBricks0001
using NMolecules.Bricks;

[assembly: Rule("R1", "Contract", "Infrastructure", RuleMode.ForbidDependency)]

[Role("Contract")]
public interface IContractMessage
{
}

public sealed class InvoiceMessage : IContractMessage
{
    private readonly SqlGateway gateway = default!;
}

[Role("Infrastructure")]
public sealed class SqlGateway
{
}
```

## Member Contract Pass

```csharp analyzer-pass
using System;
using NMolecules.Bricks;

public sealed class IdentifierAttribute : Attribute
{
}

[RequireExactlyOneMember(typeof(IdentifierAttribute))]
public sealed class Customer
{
    [Identifier]
    public string Id { get; init; } = string.Empty;
}
```

## Member Contract Violation

```csharp analyzer-violation XMoleculesBricks0003
using System;
using NMolecules.Bricks;

public sealed class IdentifierAttribute : Attribute
{
}

[RequireExactlyOneMember(typeof(IdentifierAttribute))]
public sealed class Customer
{
}
```
