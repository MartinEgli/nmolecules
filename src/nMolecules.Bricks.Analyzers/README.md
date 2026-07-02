# nMolecules Bricks Analyzers

This package contains Roslyn analyzers for `NMolecules.Bricks`.
It is the behavior source of truth for shipped Bricks analyzer diagnostics; the
integration analyzer surface remains an IDE/host compatibility layer.

## Analyzer Areas

| Analyzer | Purpose |
| --- | --- |
| `BrickMetadataAnalyzer` | Validates policy, role, rule, dependency and member-contract configuration metadata. |
| `BrickDependencyRuleAnalyzer` | Evaluates assembly-level `RuleAttribute` declarations against signature and member-body type dependencies. |
| `BrickMemberContractAnalyzer` | Enforces direct and custom-attribute-backed member cardinality contracts on classes and structs. |

## Diagnostics

| Id | Purpose |
| --- | --- |
| `XMoleculesBricks0001` | A declared brick dependency rule is violated, or a dependency uses the same source and target element. |
| `XMoleculesBricks0002` | Brick role, policy, rule, dependency or member-contract configuration is invalid, duplicated or incomplete. |
| `XMoleculesBricks0003` | An exactly-one member contract is violated. |
| `XMoleculesBricks0004` | An all-members contract is violated. |
| `XMoleculesBricks0005` | A fixed member-count contract is violated. |
| `XMoleculesBricks0006` | An exclusive-choice member contract is violated. |
| `XMoleculesBricks0007` | A member-count range contract is violated. |
| `XMoleculesBricks0008` | A forbidden-member contract is violated. |
| `XMoleculesBricks0009` | A unique named-member contract is violated. |
| `XMoleculesBricks0010` | A required named-member contract is violated. |

`XMoleculesBricks0002` also covers duplicate Bricks metadata: repeated
non-empty `RuleAttribute` IDs, repeated effective rule signatures, conflicting rule modes
for the same effective source/target rule, repeated effective roles on the same type,
conflicting role combinations, contradictory rule filters, and member-contract
combination conflicts.

Visual Studio and MSBuild load the analyzer from the NuGet analyzer path:

```text
analyzers/dotnet/cs/NMolecules.Bricks.Analyzers.dll
```

For local development, reference the project as an analyzer:

```xml
<ProjectReference
  Include="..\..\src\nMolecules.Bricks.Analyzers\nMolecules.Bricks.Analyzers.csproj"
  OutputItemType="Analyzer"
  ReferenceOutputAssembly="false" />
```

Severity can be configured through `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.XMoleculesBricks0001.severity = error
dotnet_diagnostic.XMoleculesBricks0002.severity = warning
dotnet_diagnostic.XMoleculesBricks0003.severity = error
dotnet_diagnostic.XMoleculesBricks0004.severity = error
dotnet_diagnostic.XMoleculesBricks0005.severity = error
dotnet_diagnostic.XMoleculesBricks0006.severity = error
dotnet_diagnostic.XMoleculesBricks0007.severity = error
dotnet_diagnostic.XMoleculesBricks0008.severity = error
dotnet_diagnostic.XMoleculesBricks0009.severity = error
dotnet_diagnostic.XMoleculesBricks0010.severity = error
```
