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
| `BrickNameConventionAnalyzer` | Enforces `NameConvention*` element-name constraints, one-of alternatives, aliases and overrides. |

## Diagnostics

| Id | Purpose |
| --- | --- |
| `XMoleculesBricks0001` | A declared brick dependency rule is violated, or a dependency uses the same source and target element. |
| `XMoleculesBricks0002` | Compatibility umbrella for invalid Bricks configuration; new findings use the granular `020x` IDs below. |
| `XMoleculesBricks0003` | An exactly-one member contract is violated. |
| `XMoleculesBricks0004` | An all-members contract is violated. |
| `XMoleculesBricks0005` | A fixed member-count contract is violated. |
| `XMoleculesBricks0006` | An exclusive-choice member contract is violated. |
| `XMoleculesBricks0007` | A member-count range contract is violated. |
| `XMoleculesBricks0008` | A forbidden-member contract is violated. |
| `XMoleculesBricks0009` | A unique named-member contract is violated. |
| `XMoleculesBricks0010` | A required named-member contract is violated. |
| `XMoleculesBricks0011` | Public Bricks API declarations are missing XML documentation. |
| `XMoleculesBricks0020` | A type does not satisfy an active name convention or any active alternative. |
| `XMoleculesBricks0021` | A type has conflicting active required name conventions with no override. |
| `XMoleculesBricks0022` | A name-convention alias references a source without a convention. |
| `XMoleculesBricks0023` | A name-convention override references a source that is not active. |
| `XMoleculesBricks0200` | Policy configuration is invalid, conflicting, self-importing or linked to a missing owner policy. |
| `XMoleculesBricks0201` | Role configuration or role-combination configuration is invalid, duplicated, ambiguous, linked to a missing policy or references another combination instead of roles. |
| `XMoleculesBricks0202` | Rule configuration is invalid, duplicated, contradictory or linked to a missing policy. |
| `XMoleculesBricks0203` | Dependency configuration is invalid, incomplete or linked to a missing policy. |
| `XMoleculesBricks0204` | Rule-filter configuration is invalid or contradictory. |
| `XMoleculesBricks0205` | Member-contract configuration is invalid or contradictory. |
| `XMoleculesBricks0206` | Namespace-role configuration is invalid or unmatched. |
| `XMoleculesBricks0207` | Project, folder, runtime or inheritance evidence is missing or inconsistent. |
| `XMoleculesBricks0208` | Package-boundary configuration is invalid. |
| `XMoleculesBricks0209` | Analyzer sample configuration is invalid. |

Configuration diagnostics include structured `Diagnostic.Properties` such as
`BrickConfigurationKind`, `RuleId`, `PolicyId`, `SourceRole`, `TargetRole`,
`Source`, `Target`, `ContractKind` and `CorrelationMode`. Rule and member-contract
violations include `BrickViolationKind` so SARIF, IDE tooling and roundtrip scripts can
group related findings deterministically.

The granular families still cover the full duplicate/conflict surface that was
previously grouped under `XMoleculesBricks0002`: repeated effective rule signatures,
conflicting rule modes, repeated effective roles, conflicting role combinations,
missing policy references, contradictory rule filters, and member-contract
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
dotnet_diagnostic.XMoleculesBricks0011.severity = warning
dotnet_diagnostic.XMoleculesBricks0020.severity = error
dotnet_diagnostic.XMoleculesBricks0021.severity = error
dotnet_diagnostic.XMoleculesBricks0022.severity = warning
dotnet_diagnostic.XMoleculesBricks0023.severity = warning
dotnet_diagnostic.XMoleculesBricks0200.severity = warning
dotnet_diagnostic.XMoleculesBricks0201.severity = warning
dotnet_diagnostic.XMoleculesBricks0202.severity = warning
dotnet_diagnostic.XMoleculesBricks0203.severity = warning
dotnet_diagnostic.XMoleculesBricks0204.severity = warning
dotnet_diagnostic.XMoleculesBricks0205.severity = warning
dotnet_diagnostic.XMoleculesBricks0206.severity = warning
dotnet_diagnostic.XMoleculesBricks0207.severity = warning
dotnet_diagnostic.XMoleculesBricks0208.severity = warning
dotnet_diagnostic.XMoleculesBricks0209.severity = warning
```
