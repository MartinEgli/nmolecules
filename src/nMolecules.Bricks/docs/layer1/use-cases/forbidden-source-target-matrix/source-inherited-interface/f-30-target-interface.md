# F-30: Forbidden Dependency From Inherited Interface To Interface

Layer: 1 - Core  
Depends on: ../../../foundational-concept.md, ../../bricks-layer1-core-v2.md, ../../uc-l1-18-forbidden-source-target-matrix.md

## Goal

- define the forbidden source-target combination 'Inherited Interface -> Interface'
- make the shape combination explicit instead of leaving it implicit in the overview matrix
- keep the expected forbidden-dependency result clear and repeatable

## Scenario

We define one forbidden dependency whose source shape is 'Inherited Interface' and whose target shape is 'Interface'.
The exact normalization of this pair is defined in UC-L1-18. This file is the granular case entry for that pair.

## Policy

### Abstract Concept

```csharp
new BrickPolicy
{
    Name = "Layer1.Forbidden.F-30",
    Rules =
    [
        new BrickRule
        {
            Name = "Forbidden dependency from Inherited Interface to Interface",
            SourceRoles = BrickRoleSelector.Exact("Source"),
            TargetRoles = BrickRoleSelector.Exact("Target"),
            Dependencies = new BrickDependencySelector { Kind = null },
            Scope = BrickScope.Type,
            Decision = BrickDecision.Deny,
            Severity = BrickSeverity.Error
        }
    ],
    DefaultDecision = BrickPermissionDefault.Allow
};
```

This abstract form uses `Source` and `Target` as generic role placeholders.
The explicit example below binds them to `Billing.Domain` and `Infrastructure`.

### Explicit Example

```csharp
using NMolecules.Bricks;

[assembly: Rule(
    "F-30",
    "Billing.Domain",
    "Infrastructure",
    RuleMode.ForbidDependency,
    "Rule {rule}: {source} must not depend on {target}")]
```

The exact "Inherited Interface -> Interface" shape in this file comes from the
concrete dependency evidence shown below, not from a separate attribute
parameter in the current explicit API surface.

## Minimal Example

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public void Handle(IForbiddenPort target)
    {
    }
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenPort
{
}
```

## Expected Result

A forbidden dependency violation is produced when a source element with the selected source role depends on a target element with the forbidden target role under this source-target shape combination.

The important result is that this exact pair is covered explicitly, not only indirectly through the overview matrix.

## Variant Samples

Each sample below shows a concrete way the forbidden interface can appear in code for this target family.
Any of these occurrences can supply the forbidden dependency evidence for this source-target combination.

### Interface As Field Type

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    private IForbiddenPort _dependency;
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenPort
{
}
```

### Interface In Field Initializer

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    private object _dependency = new ForbiddenPort();
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenPort
{
}
```

### Interface As Property Type

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public IForbiddenPort Dependency { get; set; }
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenPort
{
}
```

### Interface In Property Accessor

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public object Dependency => new ForbiddenPort();
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenPort
{
}
```

### Interface As Method Parameter

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public void Handle(IForbiddenPort target)
    {
    }
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenPort
{
}
```

### Interface As Method Return Type

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public IForbiddenPort CreateDependency() => new ForbiddenPort();
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenPort
{
}
```

### Interface In Method Body

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public void Handle()
    {
        IForbiddenPort target = new ForbiddenPort();
    }
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenPort
{
}
```

### Interface In Local Function Or Nested Operation

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public void Handle()
    {
        void Inner()
        {
            Process(new ForbiddenPort());
        }

        Inner();
    }

    private static void Process(object dependency)
    {
    }
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenPort
{
}
```

### Interface As Constructor Parameter

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public SourceComponent(IForbiddenPort target)
    {
    }
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenPort
{
}
```

### Interface In Constructor Body

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public SourceComponent()
    {
        IForbiddenPort target = new ForbiddenPort();
    }
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenPort
{
}
```

### Interface In Destructor Body

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    ~SourceComponent()
    {
        IForbiddenPort target = new ForbiddenPort();
    }
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenPort
{
}
```

## What This Proves

- the combination has its own explicit Layer 1 entry
- the forbidden-dependency engine remains consistent across all source-target shapes
- the matrix can be navigated as granular use-case files instead of one large table



