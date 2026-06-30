using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace NMolecules.Bricks.Analyzers.Test
{
    public class BrickForbiddenSourceTargetMatrixDocumentationTest
    {
        private static readonly string[] SourceShapeDirectories =
        {
            "source-type",
            "source-derived-type",
            "source-interface",
            "source-inherited-interface",
            "source-member",
            "source-property",
            "source-constructor",
            "source-destructor",
            "source-namespace"
        };

        private static readonly string[] TargetShapeNames =
        {
            "target-type",
            "target-derived-type",
            "target-interface",
            "target-inherited-interface",
            "target-member",
            "target-property",
            "target-constructor",
            "target-destructor",
            "target-namespace"
        };

        [Fact]
        public void ForbiddenSourceTargetMatrixDocumentsEveryConcreteShape()
        {
            var matrixDirectory = GetMatrixDirectory();
            var files = Directory
                .GetFiles(matrixDirectory, "f-*-target-*.md", SearchOption.AllDirectories)
                .Select(path => new MatrixFile(path, matrixDirectory))
                .OrderBy(file => file.Number)
                .ToArray();

            Assert.Equal(SourceShapeDirectories.Length * TargetShapeNames.Length, files.Length);
            Assert.Equal(Enumerable.Range(1, files.Length), files.Select(file => file.Number));

            var expectedFiles = BuildExpectedMatrixFiles(matrixDirectory);
            Assert.Equal(expectedFiles, files.Select(file => file.RelativePath));

            foreach (var file in files)
            {
                var content = File.ReadAllText(file.Path);

                Assert.Contains("## Minimal Example", content, StringComparison.Ordinal);
                Assert.Contains("## Expected Result", content, StringComparison.Ordinal);
                Assert.Contains("RuleMode.ForbidDependency", content, StringComparison.Ordinal);
                Assert.Contains("forbidden dependency violation", content, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("Block04", content, StringComparison.Ordinal);
            }
        }

        private static IReadOnlyList<string> BuildExpectedMatrixFiles(string matrixDirectory)
        {
            var expected = new List<string>();
            var number = 1;

            foreach (var sourceDirectory in SourceShapeDirectories)
            {
                foreach (var targetShape in TargetShapeNames)
                {
                    var fileName = $"f-{number:00}-{targetShape}.md";
                    var path = Path.Combine(matrixDirectory, sourceDirectory, fileName);
                    expected.Add(Path.GetRelativePath(matrixDirectory, path));
                    number++;
                }
            }

            return expected;
        }

        private static string GetMatrixDirectory()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(
                    directory.FullName,
                    "src",
                    "nMolecules.Bricks",
                    "docs",
                    "layer1",
                    "use-cases",
                    "forbidden-source-target-matrix");

                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not find the Bricks forbidden source-target matrix directory.");
        }

        private readonly struct MatrixFile
        {
            private static readonly Regex NumberPattern = new Regex(
                @"f-(\d{2})-target-.*\.md$",
                RegexOptions.CultureInvariant | RegexOptions.Compiled);

            public MatrixFile(string path, string matrixDirectory)
            {
                Path = path;
                RelativePath = System.IO.Path.GetRelativePath(matrixDirectory, path);

                var match = NumberPattern.Match(System.IO.Path.GetFileName(path));
                if (!match.Success)
                {
                    throw new InvalidOperationException($"Matrix file '{path}' does not use the expected f-XX-target-*.md naming pattern.");
                }

                Number = int.Parse(match.Groups[1].Value);
            }

            public string Path { get; }

            public string RelativePath { get; }

            public int Number { get; }
        }
    }
}
