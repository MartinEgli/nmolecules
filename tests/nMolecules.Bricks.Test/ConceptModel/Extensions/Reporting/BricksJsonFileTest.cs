using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksJsonFileTest
    {
        [Fact]
        public void LoadPolicyReadsJsonPolicyFile()
        {
            using var temp = new TempDirectory();
            var path = temp.PathOf("bricks-policy.json");
            File.WriteAllText(path, @"{
                ""schema"": ""NMolecules.Bricks.Policy/1.0"",
                ""policy"": {
                    ""id"": ""Default"",
                    ""name"": ""Default"",
                    ""defaultDecision"": ""Allow"",
                    ""enforcement"": ""Analyze""
                }
            }");

            var document = BrickJsonFile.LoadPolicy(path);

            Assert.Equal(BrickPolicyDocument.CurrentSchema, document.Schema);
            Assert.Equal(BrickPolicyId.From("Default"), document.Policy.Id);
            Assert.Equal(BrickEnforcementMode.Analyze, document.Policy.Enforcement);
        }

        [Fact]
        public void LoadAdoptionReadsJsonAdoptionFile()
        {
            using var temp = new TempDirectory();
            var path = temp.PathOf("bricks-adoption.json");
            File.WriteAllText(path, @"{
                ""schema"": ""NMolecules.Bricks.Adoption/1.0"",
                ""generatedAt"": ""2026-06-23T19:00:00+00:00"",
                ""baselines"": [
                    { ""ruleId"": ""XMoleculesBricks0001"", ""sourcePattern"": ""Source"", ""targetPattern"": ""Target"" }
                ]
            }");

            var document = BrickJsonFile.LoadAdoption(path);

            Assert.Equal(BrickAdoptionDocument.CurrentSchema, document.Schema);
            Assert.Single(document.Baselines);
            Assert.Empty(document.Suppressions);
        }

        [Fact]
        public void SaveAdoptionCreatesParentDirectoryAndWritesJson()
        {
            using var temp = new TempDirectory();
            var path = temp.PathOf("nested", "bricks-adoption.json");
            var document = new BrickAdoptionDocument(
                DateTimeOffset.UnixEpoch,
                new[] { new BrickBaselineEntry(RuleId.From("XMoleculesBricks0001"), "Source", "Target") },
                null);

            BrickJsonFile.SaveAdoption(path, document);

            Assert.True(File.Exists(path));
            Assert.Single(BrickJsonFile.LoadAdoption(path).Baselines);
        }

        [Fact]
        public void SaveReportWritesJson()
        {
            using var temp = new TempDirectory();
            var path = temp.PathOf("report.json");
            var report = new BrickReportDocument(
                DateTimeOffset.UnixEpoch,
                new[] { Violation("type:Order") });

            BrickJsonFile.SaveReport(path, report);

            using var json = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(BrickReportDocument.CurrentSchema, json.RootElement.GetProperty("schema").GetString());
            Assert.Equal(1, json.RootElement.GetProperty("summary").GetProperty("total").GetInt32());
        }

        [Fact]
        public void SaveExportDocumentsWriteJson()
        {
            using var temp = new TempDirectory();
            var element = Element("type:Order");

            var roleMapPath = temp.PathOf("role-map.json");
            BrickJsonFile.SaveRoleMap(
                roleMapPath,
                new BrickRoleMapDocument(DateTimeOffset.UnixEpoch, new[] { new BrickRoleMapEntry(element, new[] { RoleId.From("DDD.Entity") }, false, 1, 0) }));

            var graphPath = temp.PathOf("dependency-graph.json");
            BrickJsonFile.SaveDependencyGraph(
                graphPath,
                new BrickDependencyGraphDocument(DateTimeOffset.UnixEpoch, new[] { new BrickDependencyGraphNode(element) }, null));

            var tracePath = temp.PathOf("resolution-trace.json");
            BrickJsonFile.SaveResolutionTrace(
                tracePath,
                new BrickResolutionTraceDocument(DateTimeOffset.UnixEpoch, new[] { new BrickResolutionTraceEntry(element, null, new[] { RoleId.From("DDD.Entity") }, new[] { "Applied." }, false) }));

            Assert.Equal(BrickRoleMapDocument.CurrentSchema, ReadSchema(roleMapPath));
            Assert.Equal(BrickDependencyGraphDocument.CurrentSchema, ReadSchema(graphPath));
            Assert.Equal(BrickResolutionTraceDocument.CurrentSchema, ReadSchema(tracePath));
        }

        [Fact]
        public void SaveSupportsRelativeFileWithoutParentDirectory()
        {
            using var temp = new TempDirectory();
            var previousDirectory = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(temp.Path);
                BrickJsonFile.SaveReport(
                    "report.json",
                    new BrickReportDocument(DateTimeOffset.UnixEpoch, Enumerable.Empty<BrickViolation>()));

                Assert.True(File.Exists(temp.PathOf("report.json")));
            }
            finally
            {
                Directory.SetCurrentDirectory(previousDirectory);
            }
        }

        [Fact]
        public void FileOperationsRequirePathAndDocuments()
        {
            Assert.Throws<ArgumentNullException>(() => BrickJsonFile.LoadPolicy(null));
            Assert.Throws<ArgumentNullException>(() => BrickJsonFile.LoadAdoption(null));
            Assert.Throws<ArgumentNullException>(() => BrickJsonFile.SaveAdoption(null, new BrickAdoptionDocument(DateTimeOffset.UnixEpoch, null, null)));
            Assert.Throws<ArgumentNullException>(() => BrickJsonFile.SaveAdoption("adoption.json", null));
            Assert.Throws<ArgumentNullException>(() => BrickJsonFile.SaveReport("report.json", null));
            Assert.Throws<ArgumentNullException>(() => BrickJsonFile.SaveRoleMap("role-map.json", null));
            Assert.Throws<ArgumentNullException>(() => BrickJsonFile.SaveDependencyGraph("dependency-graph.json", null));
            Assert.Throws<ArgumentNullException>(() => BrickJsonFile.SaveResolutionTrace("resolution-trace.json", null));
        }

        private static string ReadSchema(string path)
        {
            using var json = JsonDocument.Parse(File.ReadAllText(path));
            return json.RootElement.GetProperty("schema").GetString();
        }

        private static BrickViolation Violation(string sourceId) =>
            new BrickViolation(
                BrickViolationKind.DependencyRule,
                Element(sourceId),
                "Violation",
                BrickSeverity.Error,
                BrickViolationState.Active);

        private static BrickElement Element(string id) =>
            new BrickElement(BrickElementId.From(id), BrickElementKind.Type, id.Substring("type:".Length));

        private sealed class TempDirectory : IDisposable
        {
            public TempDirectory()
            {
                Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "nmolecules-bricks-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(Path);
            }

            public string Path { get; }

            public string PathOf(params string[] parts)
            {
                var result = Path;
                foreach (var part in parts)
                {
                    result = System.IO.Path.Combine(result, part);
                }

                return result;
            }

            public void Dispose()
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, true);
                }
            }
        }
    }
}
