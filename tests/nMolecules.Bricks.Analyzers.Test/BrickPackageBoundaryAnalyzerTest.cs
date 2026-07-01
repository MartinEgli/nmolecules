using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace NMolecules.Bricks.Analyzers.Test
{
    public class BrickPackageBoundaryAnalyzerTest
    {
        [Fact]
        public void AnalyzerPackageDoesNotReferenceRuntimePackage()
        {
            var project = LoadProject("src/nMolecules.Bricks.Analyzers/nMolecules.Bricks.Analyzers.csproj");

            Assert.DoesNotContain(
                ProjectReferences(project),
                reference => reference.Replace('\\', '/').Contains("src/nMolecules.Bricks/", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void AnalyzerPackageDoesNotUseRuntimeNamespaceDirectly()
        {
            var analyzerSources = Directory.GetFiles(
                Path.Combine(FindRepositoryRoot(), "src", "nMolecules.Bricks.Analyzers"),
                "*.cs",
                SearchOption.TopDirectoryOnly);

            var offendingFiles = analyzerSources
                .Where(path => File.ReadAllText(path).Contains("using NMolecules.Bricks;", StringComparison.Ordinal))
                .Select(path => Path.GetFileName(path))
                .OrderBy(path => path)
                .ToArray();

            Assert.Empty(offendingFiles);
        }

        [Fact]
        public void RuntimePackageDoesNotReferenceOptionalBricksPackages()
        {
            var project = LoadProject("src/nMolecules.Bricks/nMolecules.Bricks.csproj");
            var optionalPackages = new[]
            {
                "nMolecules.Bricks.Analyzers",
                "nMolecules.Bricks.Ai",
                "nMolecules.Bricks.Extensions",
                "nMolecules.Bricks.Extensions.Assessment",
                "nMolecules.Bricks.Extensions.Core",
                "nMolecules.Bricks.Extensions.Reporting",
                "nMolecules.Bricks.Extensions.Runtime"
            };

            var projectReferences = ProjectReferences(project)
                .Select(reference => reference.Replace('\\', '/'))
                .ToArray();

            foreach (var optionalPackage in optionalPackages)
            {
                Assert.DoesNotContain(
                    projectReferences,
                    reference => reference.Contains(optionalPackage, StringComparison.OrdinalIgnoreCase));
            }
        }

        private static XDocument LoadProject(string relativePath) =>
            XDocument.Load(Path.Combine(FindRepositoryRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar)));

        private static IEnumerable<string> ProjectReferences(XDocument project) =>
            project
                .Descendants("ProjectReference")
                .Select(reference => reference.Attribute("Include")?.Value)
                .Where(reference => !string.IsNullOrWhiteSpace(reference));

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "docs")) &&
                    Directory.Exists(Path.Combine(directory.FullName, "src")) &&
                    Directory.Exists(Path.Combine(directory.FullName, "tests")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not find repository root from " + AppContext.BaseDirectory);
        }
    }
}
