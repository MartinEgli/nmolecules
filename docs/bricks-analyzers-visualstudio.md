# Bricks Analyzers in Visual Studio

`NMolecules.Bricks.Analyzers` is a Roslyn analyzer package. Visual Studio loads it when it is referenced as an analyzer or installed as a NuGet analyzer package.

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

## Severity

Configure diagnostic severity in `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.XMoleculesBricks0001.severity = warning
dotnet_diagnostic.XMoleculesBricks0002.severity = warning
dotnet_diagnostic.XMoleculesBricks0003.severity = warning
```

## Current diagnostics

| Id | Meaning |
| --- | --- |
| `XMoleculesBricks0001` | `RoleAttribute` has an empty role name. |
| `XMoleculesBricks0002` | `RuleAttribute` has empty id, source role, or target role. |
| `XMoleculesBricks0003` | `DependencyAttribute` has empty id, source, target, or kind. |
