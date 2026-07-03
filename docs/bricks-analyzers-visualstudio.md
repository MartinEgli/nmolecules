# Bricks Analyzers in Visual Studio and VS Code

`NMolecules.Bricks.Analyzers` is a Roslyn analyzer package. Visual Studio and VS Code load it when it is referenced as an analyzer or installed as a NuGet analyzer package.

## Local development reference

Use this while developing the analyzer in the same solution:

```xml
<ItemGroup>
  <ProjectReference
    Include="..\..\src\nMolecules.Bricks.Analyzers\nMolecules.Bricks.Analyzers.csproj"
    OutputItemType="Analyzer"
    ReferenceOutputAssembly="false" />
</ItemGroup>
```

## NuGet package reference

Use this for consuming projects after packaging:

```xml
<PackageReference Include="NMolecules.Bricks" Version="0.2.2" />
<PackageReference Include="NMolecules.Bricks.Analyzers" Version="0.2.2" PrivateAssets="all" />
```

The analyzer package places the DLL under:

```text
analyzers/dotnet/cs/NMolecules.Bricks.Analyzers.dll
```

## Visual Studio settings

In Visual Studio 2022:

- enable full solution analysis if diagnostics should appear solution-wide
- set Error List to `Build + IntelliSense`
- rebuild once after adding the analyzer reference

## VS Code settings

Use the C# Dev Kit or OmniSharp extension with analyzer support enabled. In this workspace the superproject `.vscode/settings.json` points `nmolecules.diagnosticsTarget` at the Bricks dependency-rule violation sample so `nMolecules: Refresh Diagnostics` can populate the Problems view from the Bricks analyzer project reference.

The superproject also provides VS Code tasks:

- `bricks: test core analyzer`
- `bricks: build pass sample`
- `bricks: check metadata violations`
- `bricks: check dependency violations`
- `bricks: check member-contract violations`
- `bricks: check integration policy violations`

## Severity

Configure diagnostic severity in `.editorconfig`:

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

## Current diagnostics

| Id | Meaning |
| --- | --- |
| `XMoleculesBricks0001` | A declared brick dependency rule is violated. |
| `XMoleculesBricks0002` | Compatibility umbrella for invalid Bricks configuration. New findings use the granular `020x` IDs below. |
| `XMoleculesBricks0003` | An exactly-one member contract is violated. |
| `XMoleculesBricks0004` | An all-members contract is violated. |
| `XMoleculesBricks0005` | A fixed member-count contract is violated. |
| `XMoleculesBricks0006` | An exclusive-choice member contract is violated. |
| `XMoleculesBricks0007` | A member-count range contract is violated. |
| `XMoleculesBricks0008` | A forbidden-member contract is violated. |
| `XMoleculesBricks0009` | A unique named-member contract is violated. |
| `XMoleculesBricks0010` | A required named-member contract is violated. |
| `XMoleculesBricks0200` | Policy configuration is invalid or conflicting. |
| `XMoleculesBricks0201` | Role configuration or role-combination configuration is invalid or duplicated. |
| `XMoleculesBricks0202` | Rule configuration is invalid, duplicated or contradictory. |
| `XMoleculesBricks0203` | Dependency configuration is invalid or incomplete. |
| `XMoleculesBricks0204` | Rule-filter configuration is invalid or contradictory. |
| `XMoleculesBricks0205` | Member-contract configuration is invalid or contradictory. |
| `XMoleculesBricks0206` | Namespace-role configuration is invalid or unmatched. |
| `XMoleculesBricks0207` | Project, folder, runtime or inheritance evidence is missing or inconsistent. |
| `XMoleculesBricks0208` | Package-boundary configuration is invalid. |
| `XMoleculesBricks0209` | Analyzer sample configuration is invalid. |
