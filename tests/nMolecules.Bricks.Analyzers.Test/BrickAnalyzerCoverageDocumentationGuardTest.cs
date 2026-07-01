using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace NMolecules.Bricks.Analyzers.Test
{
    public class BrickAnalyzerCoverageDocumentationGuardTest
    {
        private static readonly string[] RequestedAnalyzerNames =
        {
            "BrickInheritanceDependencyAnalyzer",
            "BrickDefaultPolicyAnalyzer",
            "BrickRuleFilterAnalyzer",
            "BrickNamespaceRoleAnalyzer",
            "BrickProjectEvidenceAnalyzer",
            "BrickXmlDocumentationAnalyzer",
            "BrickSampleConsistencyAnalyzer",
            "BrickPackageBoundaryAnalyzer",
            "BrickFolderEvidenceAnalyzer",
            "BrickRuntimeEvidenceAnalyzer"
        };

        private static readonly string[] DiagnosticIds =
        {
            "XMoleculesBricks0001",
            "XMoleculesBricks0002",
            "XMoleculesBricks0003",
            "XMoleculesBricks0004",
            "XMoleculesBricks0005",
            "XMoleculesBricks0006",
            "XMoleculesBricks0007",
            "XMoleculesBricks0008",
            "XMoleculesBricks0009",
            "XMoleculesBricks0010",
            "XMoleculesBricks0011"
        };

        [Fact]
        public void AnalyzerCoverageMatrixKeepsRequestedAnalyzersCovered()
        {
            var rows = ReadMarkdownRows("docs", "bricks-analyzer-coverage-matrix.md");

            foreach (var analyzerName in RequestedAnalyzerNames)
            {
                var row = Assert.Single(rows.Where(candidate => candidate.Cells[0].Contains(analyzerName, StringComparison.Ordinal)));
                Assert.Equal("Covered", row.Cells[1]);
            }
        }

        [Fact]
        public void AnalyzerCoverageMatrixKeepsDiagnosticIdsCovered()
        {
            var rows = ReadMarkdownRows("docs", "bricks-analyzer-coverage-matrix.md");

            foreach (var diagnosticId in DiagnosticIds)
            {
                var row = Assert.Single(rows.Where(candidate => candidate.Cells[0].Contains(diagnosticId, StringComparison.Ordinal)));
                Assert.Equal("Covered", row.Cells[1]);
            }
        }

        [Fact]
        public void AnalyzerCoverageDocumentsOnlyKnownStatuses()
        {
            var knownStatuses = new HashSet<string>(StringComparer.Ordinal)
            {
                "Covered",
                "Partial",
                "Planned",
                "Runtime-only",
                "Unsupported",
                "Runtime/model-only"
            };

            foreach (var path in new[]
            {
                Path.Combine("docs", "bricks-analyzer-coverage-matrix.md"),
                Path.Combine("docs", "bricks-analyzer-usecase-audit.md")
            })
            {
                foreach (var status in ReadStatusCells(path))
                {
                    var normalized = StripMarkdown(status);
                    if (knownStatuses.Contains(normalized))
                    {
                        continue;
                    }

                    Assert.Fail($"Unexpected analyzer coverage status '{status}' in '{path}'.");
                }
            }
        }

        private static IReadOnlyList<MarkdownRow> ReadMarkdownRows(params string[] pathParts)
        {
            var path = Path.Combine(pathParts);
            var fullPath = FindRepositoryFile(path);
            return File.ReadAllLines(fullPath)
                .Where(line => line.StartsWith("| ", StringComparison.Ordinal))
                .Where(line => !line.Contains("---", StringComparison.Ordinal))
                .Select(line => new MarkdownRow(
                    line.Split('|')
                        .Skip(1)
                        .SkipLast(1)
                        .Select(cell => cell.Trim())
                        .ToArray()))
                .Where(row => row.Cells.Count > 1)
                .ToArray();
        }

        private static string FindRepositoryFile(string relativePath)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, relativePath);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException($"Could not find repository file '{relativePath}'.", relativePath);
        }

        private static string StripMarkdown(string value) =>
            Regex.Replace(value, @"[`*_]", string.Empty).Trim();

        private static IEnumerable<string> ReadStatusCells(string relativePath)
        {
            var fullPath = FindRepositoryFile(relativePath);
            IReadOnlyList<string> header = Array.Empty<string>();
            foreach (var line in File.ReadAllLines(fullPath))
            {
                if (!line.StartsWith("| ", StringComparison.Ordinal))
                {
                    header = Array.Empty<string>();
                    continue;
                }

                if (line.Contains("---", StringComparison.Ordinal))
                {
                    continue;
                }

                var cells = line.Split('|')
                    .Skip(1)
                    .SkipLast(1)
                    .Select(cell => cell.Trim())
                    .ToArray();

                if (cells.Any(cell => StripMarkdown(cell).Contains("Status", StringComparison.Ordinal)))
                {
                    header = cells;
                    continue;
                }

                for (var index = 0; index < cells.Length && index < header.Count; index++)
                {
                    if (StripMarkdown(header[index]).Contains("Status", StringComparison.Ordinal))
                    {
                        yield return cells[index];
                    }
                }
            }
        }

        private readonly struct MarkdownRow
        {
            public MarkdownRow(IReadOnlyList<string> cells)
            {
                Cells = cells;
            }

            public IReadOnlyList<string> Cells { get; }
        }
    }
}
