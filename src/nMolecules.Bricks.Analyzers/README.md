# nMolecules Bricks Analyzers

This package contains Roslyn analyzers for `NMolecules.Bricks`.

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
dotnet_diagnostic.XMoleculesBricks0001.severity = warning
dotnet_diagnostic.XMoleculesBricks0002.severity = warning
dotnet_diagnostic.XMoleculesBricks0003.severity = warning
```
