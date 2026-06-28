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
```

## Current diagnostics

| Id | Meaning |
| --- | --- |
| `XMoleculesBricks0001` | A declared brick dependency rule is violated. |
| `XMoleculesBricks0002` | Brick role, policy, rule, dependency or member-contract configuration is invalid. |
| `XMoleculesBricks0003` | An exactly-one member contract is violated. |
| `XMoleculesBricks0004` | An all-members contract is violated. |
| `XMoleculesBricks0005` | A fixed member-count contract is violated. |
| `XMoleculesBricks0006` | An exclusive-choice member contract is violated. |
