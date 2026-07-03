using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NMolecules.Bricks.Analyzers;
using Xunit;

namespace NMolecules.Bricks.Analyzers.Test
{
    public class BrickSampleConsistencyAnalyzerTest
    {
        [Theory]
        [MemberData(nameof(AnalyzerSamples))]
        public async Task DocumentedAnalyzerSamplesStayConsistentWithExpectedDiagnostics(
            string document,
            string sampleName,
            string source,
            string[] expectedDiagnosticIds)
        {
            Assert.False(string.IsNullOrWhiteSpace(document));
            Assert.False(string.IsNullOrWhiteSpace(sampleName));

            var diagnostics = await AnalyzeAsync(source);

            Assert.Equal(
                expectedDiagnosticIds.OrderBy(id => id).ToArray(),
                diagnostics.Select(diagnostic => diagnostic.Id).OrderBy(id => id).ToArray());
        }

        public static IEnumerable<object[]> AnalyzerSamples()
        {
            foreach (var sample in ReadAnalyzerSamples("docs/bricks-ddd-sample.md")
                .Concat(ReadAnalyzerSamples("docs/bricks-analyzer-samples.md")))
            {
                yield return new object[]
                {
                    sample.Document,
                    sample.Name,
                    sample.Source,
                    sample.ExpectedDiagnosticIds
                };
            }
        }

        private static IEnumerable<AnalyzerSample> ReadAnalyzerSamples(string relativePath)
        {
            var path = Path.Combine(FindRepositoryRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar));
            var lines = File.ReadAllLines(path);
            for (var index = 0; index < lines.Length; index++)
            {
                var line = lines[index].Trim();
                if (!line.StartsWith("```csharp analyzer-", StringComparison.Ordinal))
                {
                    continue;
                }

                var marker = line.Substring("```csharp ".Length).Trim();
                var sourceLines = new List<string>();
                index++;
                while (index < lines.Length && lines[index].Trim() != "```")
                {
                    sourceLines.Add(lines[index]);
                    index++;
                }

                yield return new AnalyzerSample(
                    relativePath,
                    marker,
                    string.Join(Environment.NewLine, sourceLines),
                    ParseExpectedDiagnosticIds(marker));
            }
        }

        private static string[] ParseExpectedDiagnosticIds(string marker)
        {
            var parts = marker.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1 && parts[0] == "analyzer-pass")
            {
                return Array.Empty<string>();
            }

            if (parts.Length >= 2 && parts[0] == "analyzer-violation")
            {
                return parts.Skip(1).ToArray();
            }

            throw new InvalidOperationException("Unsupported analyzer sample marker '" + marker + "'.");
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

        private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync(string source)
        {
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
                new BrickMetadataAnalyzer(),
                new BrickNamespaceRoleMetadataAnalyzer(),
                new BrickDependencyRuleAnalyzer(),
                new BrickMemberContractAnalyzer());

            return await BrickAnalyzerTestFixture.AnalyzeAsync(
                "SampleConsistencyFixture",
                source,
                analyzers);
        }

        private readonly struct AnalyzerSample
        {
            public AnalyzerSample(string document, string name, string source, string[] expectedDiagnosticIds)
            {
                Document = document;
                Name = name;
                Source = source;
                ExpectedDiagnosticIds = expectedDiagnosticIds;
            }

            public string Document { get; }

            public string Name { get; }

            public string Source { get; }

            public string[] ExpectedDiagnosticIds { get; }
        }
    }
}
