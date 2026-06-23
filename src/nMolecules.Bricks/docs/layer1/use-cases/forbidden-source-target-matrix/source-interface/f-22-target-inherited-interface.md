# F-22: Forbidden Dependency From Interface To Inherited Interface

Layer: 1 - Core  
Depends on: ../../../foundational-concept.md, ../../bricks-layer1-core-v2.md, ../../uc-l1-18-forbidden-source-target-matrix.md

## Goal

- define the forbidden source-target combination 'Interface -> Inherited Interface'
- make the shape combination explicit instead of leaving it implicit in the overview matrix
- keep the expected forbidden-dependency result clear and repeatable

## Scenario

We define one forbidden dependency whose source shape is 'Interface' and whose target shape is 'Inherited Interface'.
The exact normalization of this pair is defined in UC-L1-18. This file is the granular case entry for that pair.

## Policy

### Abstract Concept

```csharp
new BrickPolicy
{
    Name = "Layer1.Forbidden.F-22",
    Rules =
    [
        new BrickRule
        {
            Name = "Forbidden dependency from Interface to Inherited Interface",
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
    "F-22",
    "Billing.Domain",
    "Infrastructure",
    RuleMode.ForbidDependency,
    "Rule {rule}: {source} must not depend on {target}")]
```

The exact "Interface -> Inherited Interface" shape in this file comes from the
concrete dependency evidence shown below, not from a separate attribute
parameter in the current explicit API surface.

## Minimal Example

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public void Handle(IForbiddenSpecializedPort target)
    {
    }
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public interface IForbiddenSpecializedPort : IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenSpecializedPort
{
}
```

## Expected Result

A forbidden dependency violation is produced when a source element with the selected source role depends on a target element with the forbidden target role under this source-target shape combination.

The important result is that this exact pair is covered explicitly, not only indirectly through the overview matrix.

## Variant Samples

Each sample below shows a concrete way the forbidden inherited interface can appear in code for this target family.
Any of these occurrences can supply the forbidden dependency evidence for this source-target combination.

### Inherited interface As Field Type

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    private IForbiddenSpecializedPort _dependency;
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public interface IForbiddenSpecializedPort : IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenSpecializedPort
{
}
```

### Inherited interface In Field Initializer

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

public interface IForbiddenSpecializedPort : IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenSpecializedPort
{
}
```

### Inherited interface As Property Type

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public IForbiddenSpecializedPort Dependency { get; set; }
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public interface IForbiddenSpecializedPort : IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenSpecializedPort
{
}
```

### Inherited interface In Property Accessor

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

public interface IForbiddenSpecializedPort : IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenSpecializedPort
{
}
```

### Inherited interface As Method Parameter

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public void Handle(IForbiddenSpecializedPort target)
    {
    }
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public interface IForbiddenSpecializedPort : IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenSpecializedPort
{
}
```

### Inherited interface As Method Return Type

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public IForbiddenSpecializedPort CreateDependency() => new ForbiddenPort();
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public interface IForbiddenSpecializedPort : IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenSpecializedPort
{
}
```

### Inherited interface In Method Body

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public void Handle()
    {
        IForbiddenSpecializedPort target = new ForbiddenPort();
    }
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public interface IForbiddenSpecializedPort : IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenSpecializedPort
{
}
```

### Inherited interface In Local Function Or Nested Operation

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

public interface IForbiddenSpecializedPort : IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenSpecializedPort
{
}
```

### Inherited interface As Constructor Parameter

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public SourceComponent(IForbiddenSpecializedPort target)
    {
    }
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public interface IForbiddenSpecializedPort : IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenSpecializedPort
{
}
```

### Inherited interface In Constructor Body

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    public SourceComponent()
    {
        IForbiddenSpecializedPort target = new ForbiddenPort();
    }
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public interface IForbiddenSpecializedPort : IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenSpecializedPort
{
}
```

### Inherited interface In Destructor Body

```csharp
using NMolecules.Bricks;

[Role("Billing.Domain")]
public sealed class SourceComponent
{
    ~SourceComponent()
    {
        IForbiddenSpecializedPort target = new ForbiddenPort();
    }
}

[Role("Infrastructure")]
public interface IForbiddenPort
{
}

public interface IForbiddenSpecializedPort : IForbiddenPort
{
}

public sealed class ForbiddenPort : IForbiddenSpecializedPort
{
}
```

## What This Proves

- the combination has its own explicit Layer 1 entry
- the forbidden-dependency engine remains consistent across all source-target shapes
- the matrix can be navigated as granular use-case files instead of one large table



