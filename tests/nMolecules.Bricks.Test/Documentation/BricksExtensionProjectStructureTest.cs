using System;
using System.IO;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksExtensionProjectStructureTest
    {
        private static readonly string[] ExtensionAreas =
        {
            "Assessment",
            "Core",
            "Reporting",
            "Runtime"
        };

        [Fact]
        public void GranularExtensionProjectsHaveMatchingReadmeFiles()
        {
            var root = FindRepositoryRoot();

            foreach (var area in ExtensionAreas)
            {
                var projectDirectory = Path.Combine(root, "src", "nMolecules.Bricks.Extensions." + area);
                var readme = Path.Combine(projectDirectory, "README.md");
                var project = Path.Combine(projectDirectory, "nMolecules.Bricks.Extensions." + area + ".csproj");

                Assert.True(Directory.Exists(projectDirectory), "Missing extension project directory " + projectDirectory);
                Assert.True(File.Exists(readme), "Missing extension README " + readme);
                Assert.True(File.Exists(project), "Missing extension project file " + project);
                Assert.Contains("<PackageReadmeFile>README.md</PackageReadmeFile>", File.ReadAllText(project));
                Assert.Contains("README.md", File.ReadAllText(project));
            }
        }

        [Fact]
        public void GranularExtensionProjectsHaveMatchingTestFolders()
        {
            var root = FindRepositoryRoot();

            foreach (var area in ExtensionAreas)
            {
                var testDirectory = Path.Combine(
                    root,
                    "tests",
                    "nMolecules.Bricks.Test",
                    "ConceptModel",
                    "Extensions",
                    area);

                Assert.True(Directory.Exists(testDirectory), "Missing extension test directory " + testDirectory);
                Assert.NotEmpty(Directory.GetFiles(testDirectory, "*.cs"));
            }
        }

        [Fact]
        public void ExtensionMetaPackageReferencesEveryGranularExtensionProject()
        {
            var root = FindRepositoryRoot();
            var metaProject = File.ReadAllText(Path.Combine(
                root,
                "src",
                "nMolecules.Bricks.Extensions",
                "nMolecules.Bricks.Extensions.csproj"));

            foreach (var area in ExtensionAreas)
            {
                Assert.Contains("nMolecules.Bricks.Extensions." + area, metaProject);
            }
        }

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
